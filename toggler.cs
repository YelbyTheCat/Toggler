using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static helperMethods;
using AvatarDescriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using ExpressionParameters = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters;
using ExpressionParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter;
using VRCMenu = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu;
using MenuParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Parameter;
using UnityEditor.Animations;

/*
 * Creates various types of toggles for the game known as VRChat
 * Author: Yelby
 */
public class toggler : EditorWindow
{
    // Global
    GameObject avatar = null;
    ExpressionParameters expressionParameters = null;
    AvatarDescriptor avatarDescriptor = null;
    AnimatorController controller = null;
    string filePath = "Assets/Yelby/Programs/Toggler";

    // Global Booleans
    bool options = false;
    bool objectToggle_bool = true;
    bool unityVisible_bool = true;
    bool isParameterOn_bool = false;
    bool saveState_bool = true;
    bool writeDefaults_bool = false;

    //Toolbar
    int toolBar = 0;
    string[] toolBarSections = { "Bool", "Int" };

    //Simple
    GameObject objectToggle;
    GameObject objectToggleSwap;
    VRCMenu menu;
    bool swap = false;

    // Any
    List<ObjectInformation> objectsList = new List<ObjectInformation>();
    List<MenuCounter> menuList = new List<MenuCounter>();
    string parameterName = "";
    Vector2 scrollPose;
    bool skinnedMeshesOnly = true;
    int activeIndex = -1;

    [MenuItem("Yelby/Toggler")]
    public static void ShowWindow() { GetWindow<toggler>("Toggler 3.1.1"); }

    private void OnGUI()
    {
        GUILayout.Label("Version: 3.1.1");

        toolBar = GUILayout.Toolbar(toolBar, toolBarSections);

        // Get Descriptor
        avatarDescriptor = EditorGUILayout.ObjectField("Avatar: ", avatarDescriptor, typeof(AvatarDescriptor), true) as AvatarDescriptor;
        if (avatarDescriptor == null) return;

        // Get avatar
        avatar = null;
        avatar = avatarDescriptor.gameObject;
        avatar.name = avatar.name.Trim();

        // Get FX controller
        controller = null;
        if (avatarDescriptor.baseAnimationLayers[4].animatorController != null)
            controller = (AnimatorController)avatarDescriptor.baseAnimationLayers[4].animatorController;
        if (controller == null)
        {
            GUILayout.Label("FX Controller Missing");
            return;
        }

        // Get expression parameter
        expressionParameters = null;
        if (avatarDescriptor.expressionParameters != null)
            expressionParameters = avatarDescriptor.expressionParameters;
        if (expressionParameters == null)
        {
            GUILayout.Label("Expression Parameters Missing");
            return;
        }

        switch (toolBar)
        {
            case 0:
                // Default / Main Toggle
                objectToggle = EditorGUILayout.ObjectField("Object: ", objectToggle, typeof(GameObject), true) as GameObject;
                if (objectToggle != null && !checkParent(avatar, objectToggle))
                    objectToggle = null;

                if (objectToggle == null) return;

                // Secondary Object
                if (swap)
                {
                    objectToggleSwap = EditorGUILayout.ObjectField("Object Swap: ", objectToggleSwap, typeof(GameObject), true) as GameObject;
                    if (objectToggleSwap != null && !checkParent(avatar, objectToggleSwap))
                        objectToggleSwap = null;

                    if (objectToggleSwap == objectToggle)
                        objectToggleSwap = null;
                }
                else
                    objectToggleSwap = null;

                menu = EditorGUILayout.ObjectField("VRC Menu: ", menu, typeof(VRCMenu), true) as VRCMenu;
                if (menu != null)
                {
                    if (menu.controls.Count >= VRCMenu.MAX_CONTROLS)
                        menu = null;

                    if (menu != null)
                        EditorGUILayout.LabelField((menu.controls.Count + 1) + "/" + VRCMenu.MAX_CONTROLS, GUILayout.MaxWidth(25));
                }

                options = EditorGUILayout.Foldout(options, "Options");
                if (options)
                {
                    GUILayout.BeginVertical();
                    swap = EditorGUILayout.Toggle("Swap", swap);
                    objectToggle_bool = EditorGUILayout.Toggle((swap ? "Object 1 " : "") + "Default ON", objectToggle_bool);
                    if (!swap) unityVisible_bool = EditorGUILayout.Toggle("Unity ON", unityVisible_bool);
                    isParameterOn_bool = EditorGUILayout.Toggle("Menu ON is Reversed", isParameterOn_bool);
                    saveState_bool = EditorGUILayout.Toggle("Save", saveState_bool);
                    writeDefaults_bool = EditorGUILayout.Toggle("Write defaults", writeDefaults_bool);
                    GUILayout.EndVertical();
                }

                if (swap && (objectToggle == null || objectToggleSwap == null))
                    return;

                if (GUILayout.Button("Create Toggle"))
                {
                    objectsList = new List<ObjectInformation>();
                    ObjectInformation objectInfo = new ObjectInformation { item = objectToggle, isUnity = true };
                    if (menu != null) objectInfo.menu = menu;
                    objectsList.Add(objectInfo);

                    if (objectToggleSwap != null) objectsList.Add(new ObjectInformation { item = objectToggleSwap, isUnity = false });

                    if (swap && !objectToggle_bool) objectsList = reverse(objectsList);

                    string type = "bool";
                    // Folders
                    createParentFolder();
                    createChildFolder(type);

                    // Animation
                    createAnimation(avatar.name, objectsList, "", type, filePath);

                    // Parameters Menu
                    EditorUtility.SetDirty(expressionParameters);
                    if(isParameterOn_bool)
                        createParameter(expressionParameters, objectToggle.name, !objectToggle_bool ? 1.0f : 0.0f, saveState_bool, type);
                    else
                        createParameter(expressionParameters, objectToggle.name, objectToggle_bool ? 1.0f : 0.0f, saveState_bool, type);

                    // FX Menu
                    createFXParameter(controller, objectToggle_bool, objectToggle.name, type);
                    createFXLayer(controller, objectToggle.name);
                    fillFXLayer(avatar.name, controller, objectToggle.name, objectsList, objectToggle_bool, type, writeDefaults_bool, filePath);

                    // Visibility
                    if (swap)
                        setVisibility(objectsList);
                    else
                        objectToggle.SetActive(unityVisible_bool);

                    // Adding Menu
                    addParameterMenuEntry(objectsList, objectToggle.name, type);
                }

                break;
            case 1:
                parameterName = EditorGUILayout.TextField("Parameter Name:", parameterName).Trim();
                options = EditorGUILayout.Foldout(options, "Options");
                if (options)
                {
                    GUILayout.BeginVertical();
                    skinnedMeshesOnly = EditorGUILayout.Toggle("Skinned Meshes Only", skinnedMeshesOnly);
                    saveState_bool = EditorGUILayout.Toggle("Save", saveState_bool);
                    writeDefaults_bool = EditorGUILayout.Toggle("Write defaults", writeDefaults_bool);
                    GUILayout.EndVertical();
                }


                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Selected"))
                {
                    GameObject[] holdingArray = Selection.gameObjects;

                    if (holdingArray.Length == 0) return;

                    foreach (GameObject gameObject in holdingArray)
                    {
                        if (skinnedMeshesOnly)
                        {
                            if (!gameObject.GetComponent<SkinnedMeshRenderer>())
                                continue;
                        }

                        if (!checkParent(avatar, gameObject)) continue;
                        if (objectsList.Count != 0 && checkList(objectsList, gameObject)) continue;

                        ObjectInformation info = new ObjectInformation { item = gameObject };
                        objectsList.Add(info);
                    }
                }

                if (GUILayout.Button("Clear List"))
                {
                    objectsList = new List<ObjectInformation>();
                }

                EditorGUILayout.EndHorizontal();

                if (parameterName != "" && objectsList.Count > 1 && objectsList[1].item != null)
                    if (GUILayout.Button("Create Toggles"))
                    {
                        if (objectsList[objectsList.Count - 1].item == null)
                            objectsList.RemoveAt(objectsList.Count - 1);

                        string type = "int";
                        // Folders
                        createParentFolder();
                        createChildFolder(type);

                        // Create Animations
                        createAnimation(filePath);
                        createAnimation(avatar.name, objectsList, parameterName, type, filePath);

                        // Create Parameter
                        EditorUtility.SetDirty(expressionParameters);
                        createParameter(expressionParameters, parameterName, activeIndex == -1 ? 0 : activeIndex, saveState_bool, type);

                        // FX Menu
                        createFXParameter(controller, objectToggle_bool, parameterName, type);
                        createFXLayer(controller, parameterName);
                        fillFXLayer(avatar.name, controller, parameterName, objectsList, true, type, writeDefaults_bool, filePath);

                        // Visibility
                        setVisibility(objectsList);

                        // Adding Menu
                        addParameterMenuEntry(objectsList, parameterName, type);

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                ui_ObjectInformation();

                break;
        }
    }

    // UIs
    private void ui_ObjectInformation()
    {
        // Adds blanks to the bottom
        if (objectsList.Count == 0 || objectsList[objectsList.Count - 1].item != null)
            objectsList.Add(new ObjectInformation());

        bool keepActive = false;
        if (objectsList.Count >= 2)
        {
            // Removes null elements
            for (int i = 0; i < objectsList.Count - 1; i++)
            {
                if (objectsList[i].item == null)
                {
                    objectsList.Remove(objectsList[i]);
                    i = 0;
                }
            }

            // Removes duplicates and non-childs
            for (int i = 0; i < objectsList.Count; i++)
            {
                // Object is not a child
                if (objectsList[i].item != null && !checkParent(avatar, objectsList[i].item))
                {
                    objectsList.RemoveAt(i);
                    i = 0;
                }

                // Duplicate
                if (hasDouble(objectsList, objectsList[i].item, out int index))
                {
                    objectsList.RemoveAt(index);
                    i = 0;
                }
            }

            // If there is one turned on hide the rest
            for (int i = 0; i < objectsList.Count; i++)
            {
                if (objectsList[i].isDefault)
                {
                    keepActive = true;
                    activeIndex = i + 1;
                    break;
                }
                activeIndex = -1;
            }

            // Re-arrange
            if(keepActive)
            {
                int index = activeIndex - 1;
                ObjectInformation movingObject = objectsList[index];
                objectsList.RemoveAt(index);
                objectsList.Insert(0, movingObject);
            }
        }

        // Display Only
        menuList = new List<MenuCounter>();
        scrollPose = EditorGUILayout.BeginScrollView(scrollPose);
        for (int i = 0; i < objectsList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 80;
            if (keepActive && !objectsList[i].isDefault)
            {
                GUI.enabled = false;
                objectsList[i].isDefault = EditorGUILayout.Toggle(i + 1 + " Default ON", objectsList[i].isDefault, GUILayout.ExpandWidth(false));
            }
            else
            {
                GUI.enabled = true;
                objectsList[i].isDefault = EditorGUILayout.Toggle((keepActive ? i : (i + 1)) + " Default ON", objectsList[i].isDefault, GUILayout.ExpandWidth(false));
            }
            EditorGUIUtility.labelWidth = 60;
            GUI.enabled = true;
            objectsList[i].isUnity = EditorGUILayout.Toggle("Unity ON", objectsList[i].isUnity, GUILayout.ExpandWidth(false));
            objectsList[i].item = EditorGUILayout.ObjectField(objectsList[i].item, typeof(GameObject), true) as GameObject;
            objectsList[i].menu = EditorGUILayout.ObjectField(objectsList[i].menu, typeof(VRCMenu), true) as VRCMenu;
            if (objectsList[i].menu != null)
            {
                if (objectsList[i].menu.controls.Count == VRCMenu.MAX_CONTROLS)
                {
                    objectsList[i].menu = null;
                    continue;
                }

                if (contains(menuList, objectsList[i].menu, out int menuLocation))
                {
                    menuList[menuLocation].count++;
                }
                else
                {
                    menuList.Add(new MenuCounter
                    {
                        menu = objectsList[i].menu,
                    });
                }
                int expressionCount = objectsList[i].menu.controls.Count + count(menuList, objectsList[i].menu);
                if (expressionCount > 8)
                {
                    objectsList[i].menu = null;
                    continue;
                }

                EditorGUILayout.LabelField(expressionCount + "/" + VRCMenu.MAX_CONTROLS, GUILayout.MaxWidth(25));
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    // Creation Methods
    private void createParentFolder()
    {
        if (!AssetDatabase.IsValidFolder(filePath + "/" + avatar.name))
            AssetDatabase.CreateFolder(filePath, avatar.name);
    }

    private void createChildFolder(string typeOfToggle)
    {
        string parentFolder = filePath + "/" + avatar.name;
        if (!AssetDatabase.IsValidFolder(parentFolder + "/" + typeOfToggle))
            AssetDatabase.CreateFolder(parentFolder, typeOfToggle);
    }
}
public class ObjectInformation
{
    public GameObject item = null;
    public bool isDefault = false;
    public bool isUnity = false;
    public VRCMenu menu = null;
}

public class MenuCounter
{
    public VRCMenu menu = null;
    public int count = 1;
}
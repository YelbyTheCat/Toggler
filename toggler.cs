#if VRC_SDK_VRCSDK3
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;
using ExpressionParameters = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters;
using ExpressionParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter;
using VRCMenu = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu;
using MenuParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Parameter;
using Descriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using UnityEditor.Animations;

public class toggler : EditorWindow
{
    //Created by Yelby

    //Attributes
    GameObject avatar;
    AnimatorController controller;
    ExpressionParameters parameters;
    GameObject obj;
    GameObject obj2;
    bool swap = false;
    List<GameObject> objs = new List<GameObject>();
    List<bool> objsToggleAnimation = new List<bool>();
    List<bool> objsToggleUnity = new List<bool>();
    string outfitName = "";

    //Advanced Section
    bool options = false;
    bool saved = true;
    bool startActiveVRC = true;
    bool startActiveUnity = false;
    bool writeDefaults = false;
    bool addMask = false;

    //Outfit
    List<OutfitInventory> outfitList = new List<OutfitInventory>();
    List<string> motionPaths = new List<string>();
    Vector2 scrollPose;
    VRCMenu characterMenu;
    //GameObject tempObject;

    //Toolbar
    int toolBar = 0;
    string[] toolBarSections = { "Simple", "Any", /*"Outfits"*/ };

    [MenuItem("Yelby/Toggler")]
    public static void ShowWindow()
    {
        GetWindow<toggler>("Toggler");
    }

    private void OnGUI()
    {
        GUILayout.Label("Version: 2.2");

        toolBar = GUILayout.Toolbar(toolBar, toolBarSections);

        //Gather info
        avatar = EditorGUILayout.ObjectField("Avatar: ", avatar, typeof(GameObject), true) as GameObject;
        if(avatar != null)
        {
            var SDKRef = avatar.GetComponent<Descriptor>();
            if (SDKRef != null)
            {
                //Controller
                if (SDKRef.baseAnimationLayers[4].animatorController != null)
                {
                    //controller = EditorGUILayout.ObjectField("FX Controller: ", controller, typeof(AnimatorController), true) as AnimatorController;
                    controller = (AnimatorController)SDKRef.baseAnimationLayers[4].animatorController;
                }
                else
                {
                    controller = null;
                    Debug.LogError("Missing FX Controller");
                }
                
                //Parameter
                if(SDKRef.expressionParameters != null)
                {
                    //parameters = EditorGUILayout.ObjectField("Parameters: ", parameters, typeof(ExpressionParameters), true) as ExpressionParameters;
                    parameters = SDKRef.expressionParameters as ExpressionParameters;
                }
                else
                {
                    parameters = null;
                    Debug.LogError("Missing Parameter on avatar");
                }
            }
            else
            {
                avatar = null;
            }
        }

        switch(toolBar)
        {
            case 0:
                //Object
                obj = EditorGUILayout.ObjectField("Object: ", obj, typeof(GameObject), true) as GameObject;
                if(swap)
                {
                    obj2 = EditorGUILayout.ObjectField("Object 2: ", obj2, typeof(GameObject), true) as GameObject;
                    if (obj == obj2)
                    {
                        obj2 = null;
                    }
                }
                //Options
                options = EditorGUILayout.Foldout(options, "Options");
                if (options)
                {
                    swap = EditorGUILayout.Toggle("Swap Mode", swap);
                    saved = EditorGUILayout.Toggle("Saved", saved);
                    writeDefaults = EditorGUILayout.Toggle("Write Defaults", writeDefaults);
                    addMask = EditorGUILayout.Toggle("Add Layer Mask", addMask);

                    if(!swap)
                    {
                        startActiveVRC = EditorGUILayout.Toggle("Default ON", startActiveVRC);
                        startActiveUnity = EditorGUILayout.Toggle("Unity ON", startActiveUnity);
                    }
                }

                //Make Toggle button
                if (avatar != null && controller != null && parameters != null && obj != null)
                {
                    if (GUILayout.Button("Make Toggle"))
                    {
                        CreateFolders(avatar, obj);

                        if (swap)
                        {
                            CreateToggleAnimations(avatar, obj, obj2);
                        }
                        else
                        {
                            CreateToggleAnimations(avatar, obj);
                        }

                        AddAvatarParameters(parameters, obj, saved, startActiveVRC);
                        AddParameter(controller, obj);
                        AddLayer(avatar, controller, obj);

                        if(swap)
                        {
                            FillLayer(avatar, controller, obj, obj2);
                            obj2.SetActive(false);
                        }
                        else
                        {
                            FillLayer(avatar, controller, obj, startActiveVRC);
                            obj.SetActive(startActiveUnity);
                        }
                        Debug.Log("Finished");
                    }
                }
                else
                {
                    GUI.enabled = false;
                    GUILayout.Button("Make Toggle");
                }
                break;
            case 1:
                outfitName = EditorGUILayout.TextField("Parameter Name", outfitName);
                //Add and Clear
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Object"))
                {
                    objs.Add(default);
                    objsToggleAnimation.Add(default);
                    objsToggleUnity.Add(default);
                }
                if (GUILayout.Button("Clear"))
                {
                    objs.RemoveRange(0, objs.Count);
                    objsToggleAnimation.RemoveRange(0, objsToggleAnimation.Count);
                    objsToggleUnity.RemoveRange(0, objsToggleUnity.Count);
                }
                if (GUILayout.Button("Remove Last"))
                {
                    objs.RemoveRange(objs.Count-1, 1);
                    objsToggleAnimation.RemoveRange(objsToggleAnimation.Count-1, 1);
                    objsToggleUnity.RemoveRange(objsToggleUnity.Count-1, 1);
                }
                if (GUILayout.Button("Clear Null"))
                {
                    objsToggleAnimation = removeNull(objs, objsToggleAnimation);
                    objsToggleUnity = removeNull(objs, objsToggleUnity);
                    objs = removeNull(objs);
                }
                GUILayout.EndHorizontal();

                //Options
                options = EditorGUILayout.Foldout(options, "Options");
                if (options)
                {
                    GUILayout.BeginVertical();
                    saved = EditorGUILayout.Toggle("Saved", saved);
                    writeDefaults = EditorGUILayout.Toggle("Write Defaults", writeDefaults);
                    addMask = EditorGUILayout.Toggle("Add Layer Mask", addMask);
                    GUILayout.EndVertical();
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Objects");
                GUILayout.Label("Default ON");
                GUILayout.Label("Unity ON");
                GUILayout.EndHorizontal();

                //List
                for (var i = 0; i < objs.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    objs[i] = EditorGUILayout.ObjectField(objs[i], typeof(GameObject), true) as GameObject;
                    objsToggleAnimation[i] = EditorGUILayout.Toggle(objsToggleAnimation[i]);
                    objsToggleUnity[i] = EditorGUILayout.Toggle(objsToggleUnity[i]);
                    GUILayout.EndHorizontal();
                }

                if (avatar != null && controller != null && parameters != null && objs.Count != 0 && !outfitName.Equals(""))
                {
                    if (GUILayout.Button("Create Toggles"))
                    {
                        objsToggleAnimation = removeNull(objs, objsToggleAnimation);
                        objsToggleUnity = removeNull(objs, objsToggleUnity);
                        objs = removeNull(objs);
                        CreateFolders(avatar, outfitName);
                        CreateToggleAnimations(avatar, objs, objsToggleAnimation ,outfitName);
                        AddAvatarParameters(parameters, outfitName, saved);
                        AddParameter(controller, outfitName);
                        AddLayer(avatar, controller, outfitName);
                        FillLayer(avatar, controller, objs, outfitName, objsToggleAnimation);
                        SetActive(objs, objsToggleUnity);
                        Debug.Log("Finished");
                    }
                }
                else
                {
                    GUI.enabled = false;
                    GUILayout.Button("Create Toggles");
                }
                break;
            case 2:
                if(avatar != null)
                {
                    writeDefaults = EditorGUILayout.Toggle("Write Defaults", writeDefaults);
                    if (GUILayout.Button("Import FX"))
                    {
                        GetCatfits(controller, outfitList);
                    }

                    CreateOutfitUI();

                    if(GUILayout.Button("Create Outfits"))
                    {
                        motionPaths.Clear();
                        AddLayers(controller, outfitList);
                        AddParameters(controller, outfitList);
                        CreateAnimations(avatar, outfitList, motionPaths);
                        FillLayer(controller, motionPaths, outfitList);
                    }
                }
                break;
        }
    }

    //~~~~~Methods~~~~~
    /*Bool*/
    private void CreateFolders(GameObject avatar, GameObject tObj)
    {
        string path = "Assets/Yelby/Programs/Toggle";
        if(!AssetDatabase.IsValidFolder(path + "/" + avatar.name))
        {
            AssetDatabase.CreateFolder(path, avatar.name);
            path += avatar.name;
            Debug.Log("Folder: " + path + " created");
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void CreateToggleAnimations(GameObject avatar, GameObject tObj)
    {
        string path = "Assets/Yelby/Programs/Toggle/" + avatar.name;
        if (!AssetDatabase.IsValidFolder(path))
        {
            Debug.LogError(path + " not available");
            return;
        }

        createAnimation(path, tObj, true);
        createAnimation(path, tObj, false);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void AddAvatarParameters(ExpressionParameters tParams, GameObject tObj, bool saved, bool def)
    {
        //Check if parameter already exists
        var paraName = tParams.parameters;
        for(int i = 0; i < tParams.parameters.Length; i++)
        {
            if(tObj.name == paraName[i].name)
            {
                Debug.LogWarning("Avatar Parameter: " + tObj.name + " already exists");
                bool option = EditorUtility.DisplayDialog("Toggler", "Avatar Parameter " + tObj.name + " already exists. Override?", "Yes", "No");
                if (option)
                {
                    paraName[i].valueType = ExpressionParameters.ValueType.Bool;
                    paraName[i].saved = saved;
                    if (def)
                        paraName[i].defaultValue = 1.0f;
                    else
                        paraName[i].defaultValue = 0.0f;
                    return;
                }
                else
                    return;
            }
        }

        //Add new Parameter
        ExpressionParameter[] parameterArray = new ExpressionParameter[tParams.parameters.Length + 1];
        for (int i = 0; i < tParams.parameters.Length; i++) //Copy parameter
        {
            parameterArray[i] = tParams.parameters[i];
        }

        //Make Parameter
        ExpressionParameter parameter = new VRCExpressionParameters.Parameter();
        parameter.name = tObj.name;
        if (def)
            parameter.defaultValue = 1.0f;
        else
            parameter.defaultValue = 0.0f;
        parameter.valueType = ExpressionParameters.ValueType.Bool;
        parameter.saved = saved;

        //Add to array and set back to orginal parameter
        parameterArray[parameterArray.Length - 1] = parameter;
        tParams.parameters = parameterArray;
        Debug.Log("Avatar Parameter: " + tObj.name + " created");

        EditorUtility.SetDirty(parameters);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void AddParameter(AnimatorController tController, GameObject tObj)
    {
        int size = tController.parameters.Length;
        AnimatorControllerParameter[] animParams = tController.parameters;
        
        //Check list if a name exists
        for(int i = 0; i < size; i++)
        {
            if(tObj.name == animParams[i].name)
            {
                Debug.LogWarning("Layer Parameter: " + tObj.name + " already exists");
                bool option = EditorUtility.DisplayDialog("Toggler", "Layer Parameter " + tObj.name + " already exists. Override?", "Yes", "No");
                if (option)
                {
                    tController.RemoveParameter(animParams[i]);
                    tController.AddParameter(tObj.name, AnimatorControllerParameterType.Bool);
                    AssetDatabase.Refresh();
                    return;
                }
                else
                    return;
            }
        }

        //If it doesn't exist, creates it
        tController.AddParameter(tObj.name, AnimatorControllerParameterType.Bool);
        Debug.Log("Layer Parameter: " + tObj.name + " created");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void AddLayer(GameObject avatar, AnimatorController tController, GameObject tObj)
    {
        //int size = tController.layers.Length;
        AnimatorControllerLayer[] animLayers = tController.layers;

        //Check list if a name exists
        for (int i = 0; i < tController.layers.Length; i++)
        {
            if(tObj.name == animLayers[i].name)
            {
                Debug.LogWarning("Layer: " + tObj.name + " already exists");
                bool option = EditorUtility.DisplayDialog("Toggler", "Layer Name " + tObj.name + " already exists. Override?", "Yes", "No");
                if(option)
                {
                    tController.RemoveLayer(i);
                    tController.AddLayer(tObj.name);
                    animLayers = tController.layers;
                    animLayers[tController.layers.Length-1].defaultWeight = 1.0f;
                    animLayers[tController.layers.Length - 1].avatarMask = addMask ? CreateEmptyMask(avatar, "Assets/Yelby/Programs/Toggle/" + avatar.name) : null;
                    tController.layers = animLayers;
                    Debug.Log("Layer: " + tObj.name + " created");
                    AssetDatabase.Refresh();
                    return;
                }
                else
                {
                    return;
                }
            }
        }

        //If it doesn't exist, creates it
        tController.AddLayer(tObj.name);
        animLayers = tController.layers;
        animLayers[tController.layers.Length - 1].defaultWeight = 1.0f;
        animLayers[tController.layers.Length - 1].avatarMask = addMask ? CreateEmptyMask(avatar, "Assets/Yelby/Programs/Toggle/" + avatar.name) : null;
        tController.layers = animLayers;
        Debug.Log("Layer: " + tObj.name + " created");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void FillLayer(GameObject avatar, AnimatorController tController, GameObject tObj, bool def)
    {
        //Find layer created earlier
        int tLayer = 0;
        for(int i = 0; i < tController.layers.Length; i++)
        {
            if(tController.layers[i].name == tObj.name)
            {
                tLayer = i;
                break;
            }
        }

        //Create States
        string path = "Assets/Yelby/Programs/Toggle/" + avatar.name + "/" + tObj.name;
        AnimatorStateMachine states = tController.layers[tLayer].stateMachine;
        Vector3 location = new Vector3(-20, 120);

        //Start
        Motion defMotion = AssetDatabase.LoadAssetAtPath(path + (def ? "_OFF" : "_ON") + ".anim", typeof(AnimationClip)) as Motion;
        AnimatorState defState = createState(defMotion, states, location);

        //End
        Motion tranMotion = AssetDatabase.LoadAssetAtPath(path + (def ? "_ON" : "_OFF") + ".anim", typeof(AnimationClip)) as Motion;
        location[1] += 50;
        AnimatorState tranState = createState(tranMotion, states, location);

        //Transitions
        createTransition(defState, tranState, false, 0.0f, (def ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot), 0, tObj);
        createTransition(tranState, false, 0, (def ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If), 0, tObj);

        //Other
        states.anyStatePosition = new Vector3(0, 0);
        states.entryPosition = new Vector3(0, 50);
        states.exitPosition = new Vector3(0, 240);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    //Swap
    private void CreateToggleAnimations(GameObject avatar, GameObject obj, GameObject obj2)
    {
        string path = "Assets/Yelby/Programs/Toggle/" + avatar.name;
        if (!AssetDatabase.IsValidFolder(path))
        {
            Debug.LogError(path + " not available");
            return;
        }

        createAnimation(path, obj, obj2, true);
        createAnimation(path, obj, obj2, false);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    private void FillLayer(GameObject avatar, AnimatorController controller, GameObject obj, GameObject obj2)
    {
        string path = "Assets/Yelby/Programs/Toggle/" + avatar.name + "/";
        int layerLocation = layerIndex(controller.layers, obj.name);
        if (layerLocation == -1)
        {
            Debug.LogWarning("Layer Doesn't Exist");
            return;
        }

        //Default Modules
        AnimatorStateMachine states = controller.layers[layerLocation].stateMachine;
        states.anyStatePosition = new Vector3(0, 0);
        states.entryPosition = new Vector3(0, 50);
        states.exitPosition = new Vector3(0, 240);

        //Get Animations
        Motion defaultAnimation = AssetDatabase.LoadAssetAtPath(path + obj.name + "_ON.anim", typeof(AnimationClip)) as Motion;
        Motion swapAnimation = AssetDatabase.LoadAssetAtPath(path + obj2.name + "_ON.anim", typeof(AnimationClip)) as Motion;

        //Create States
        Vector3 location = new Vector3(-20, 120);
        AnimatorState defaultState = createState(defaultAnimation, states, location);
        location[1] += 50;
        AnimatorState swapState = createState(swapAnimation, states, location);

        //Transitions
        createTransition(defaultState, swapState, false, 0.0f, AnimatorConditionMode.If, 0, obj);
        createTransition(swapState, false, 0, AnimatorConditionMode.IfNot, 0, obj);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    /*Any*/
    private void CreateFolders(GameObject avatar, string outfitName)
    {
        string path = "Assets/Yelby/Programs/Toggle";

        //Checks for avatar Folder
        if (!AssetDatabase.IsValidFolder(path + "/" + avatar.name))
        {
            AssetDatabase.CreateFolder(path, avatar.name);
            Debug.Log("Folder: " + path + "/" + avatar.name + " created");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!AssetDatabase.IsValidFolder(path + "/" + avatar.name + "/" + outfitName))
        {
            AssetDatabase.CreateFolder(path + "/" + avatar.name, outfitName);
            Debug.Log("Folder: " + path + "/" + avatar.name + "/" + outfitName + " created");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void CreateToggleAnimations(GameObject avatar, List<GameObject> tObjs, List<bool> togglesAnimation, string outfitName)
    {
        string path = "Assets/Yelby/Programs/Toggle/" + avatar.name + "/" + outfitName;
        if (!AssetDatabase.IsValidFolder(path))
        {
            Debug.LogError(path + " not available");
            return;
        }

        //Create Default
        createAnimation(path, tObjs, togglesAnimation, outfitName);

        //Create on/off animations
        for (int i = 0; i < tObjs.Count; i++)
        {
            createAnimation(path, tObjs[i], tObjs, true);
            createAnimation(path, tObjs[i], tObjs, false);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void AddAvatarParameters(ExpressionParameters tParams, string outfitName, bool saved)
    {
        //Check if parameter already exists
        var paraName = tParams.parameters;
        for (int i = 0; i < tParams.parameters.Length; i++)
        {
            if (outfitName == paraName[i].name)
            {
                Debug.LogWarning("Avatar Parameter: " + outfitName + " already exists");
                bool option = EditorUtility.DisplayDialog("Toggler", "Avatar Parameter " + outfitName + " already exists. Override?", "Yes", "No");
                if (option)
                {
                    paraName[i].valueType = ExpressionParameters.ValueType.Int;
                    paraName[i].saved = saved;
                    paraName[i].defaultValue = 0.0f;
                    return;
                }
                else
                    return;
            }
        }

        //Add new Parameter
        ExpressionParameter[] parameterArray = new ExpressionParameter[tParams.parameters.Length + 1];
        for (int i = 0; i < tParams.parameters.Length; i++) //Copy parameter
        {
            parameterArray[i] = tParams.parameters[i];
        }

        //Make Parameter
        ExpressionParameter parameter = new ExpressionParameter();
        parameter.valueType = ExpressionParameters.ValueType.Int;
        parameter.name = outfitName;
        parameter.defaultValue = 0;
        parameter.saved = saved;

        //Add to array and set back to orginal parameter
        parameterArray[parameterArray.Length - 1] = parameter;
        tParams.parameters = parameterArray;
        Debug.Log("Avatar Parameter: " + outfitName + " created");

        EditorUtility.SetDirty(parameters);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void AddParameter(AnimatorController tController, string outfitName)
    {
        int size = tController.parameters.Length;
        AnimatorControllerParameter[] animParams = tController.parameters;

        //Check list if a name exists
        for (int i = 0; i < size; i++)
        {
            if (outfitName == animParams[i].name)
            {
                Debug.LogWarning("Layer Parameter: " + outfitName + " already exists");
                bool option = EditorUtility.DisplayDialog("Toggler", "Layer Parameter " + outfitName + " already exists. Override?", "Yes", "No");
                if (option)
                {
                    tController.RemoveParameter(animParams[i]);
                    tController.AddParameter(outfitName, AnimatorControllerParameterType.Int);
                    AssetDatabase.Refresh();
                    return;
                }
                else
                    return;
            }
        }

        //If it doesn't exist, creates it
        tController.AddParameter(outfitName, AnimatorControllerParameterType.Int);
        Debug.Log("Layer Parameter: " + outfitName + " created");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void AddLayer(GameObject avatar, AnimatorController tController, string outfitName)
    {
        //int size = tController.layers.Length;
        AnimatorControllerLayer[] animLayers = tController.layers;

        //Check list if a name exists
        for (int i = 0; i < tController.layers.Length; i++)
        {
            if (outfitName == animLayers[i].name)
            {
                Debug.LogWarning("Layer: " + outfitName + " already exists");
                bool option = EditorUtility.DisplayDialog("Toggler", "Layer Name " + outfitName + " already exists. Override?", "Yes", "No");
                if (option)
                {
                    tController.RemoveLayer(i);
                    tController.AddLayer(outfitName);
                    animLayers = tController.layers;
                    animLayers[tController.layers.Length - 1].defaultWeight = 1.0f;
                    animLayers[tController.layers.Length - 1].avatarMask = addMask ? CreateEmptyMask(avatar, "Assets/Yelby/Programs/Toggle/" + avatar.name) : null;
                    tController.layers = animLayers;
                    Debug.Log("Layer: " + outfitName + " created");
                    AssetDatabase.Refresh();
                    return;
                }
                else
                {
                    return;
                }
            }
        }

        //If it doesn't exist, creates it
        tController.AddLayer(outfitName);
        animLayers = tController.layers;
        animLayers[tController.layers.Length - 1].defaultWeight = 1.0f;
        animLayers[tController.layers.Length - 1].avatarMask = addMask ? CreateEmptyMask(avatar, "Assets/Yelby/Programs/Toggle/" + avatar.name) : null;
        tController.layers = animLayers;
        Debug.Log("Layer: " + outfitName + " created");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void FillLayer(GameObject avatar, AnimatorController tController, List<GameObject> tObjs, string outfitName, List<bool> toggle)
    {
        //Find layer created earlier
        int tLayer = 0;
        for (int i = 0; i < tController.layers.Length; i++)
        {
            if (tController.layers[i].name == outfitName)
            {
                tLayer = i;
                break;
            }
        }

        //Create States
        string path = "Assets/Yelby/Programs/Toggle/" + avatar.name + "/" + outfitName + "/";
        AnimatorStateMachine states = tController.layers[tLayer].stateMachine;
        Vector3 location = new Vector3(250, 0);

        //Other
        states.entryPosition = new Vector3(0, 0);
        states.anyStatePosition = new Vector3(0, 50);
        states.exitPosition = new Vector3(0, 100);

        //Animations
        List<Motion> motions = new List<Motion>();

        motions.Add(AssetDatabase.LoadAssetAtPath(path + outfitName + "_Default" + ".anim", typeof(AnimationClip)) as Motion);
        for (int i = 0; i < tObjs.Count; i++)
        {
            motions.Add(AssetDatabase.LoadAssetAtPath(path + tObjs[i].name + (toggle[i] ? "_OFF" : "_ON") + ".anim", typeof(AnimationClip)) as Motion);
        }

        //States
        List<AnimatorState> animatorStates = new List<AnimatorState>();
        for (int i = 0; i < motions.Count; i++)
        {
            animatorStates.Add(createState(motions[i], states, location));
            location[1] += 50;
        }

        //Transitions
        for(int i = 0; i < animatorStates.Count; i++)
        {
            createTransition(states, animatorStates[i], false, 0, AnimatorConditionMode.Equals, i, outfitName);
        }
    }

    private void SetActive(List<GameObject> tObjs, List<bool> isActive)
    {
        for (int i = 0; i < tObjs.Count; i++)
            tObjs[i].SetActive(isActive[i]);
    }

    /*Outfit*/
    private void CreateOutfitUI()
    {
        //Settings
        characterMenu = GetCharacterMenu(avatar);
        if(GUILayout.Button("Create Menus"))
        {
            CreateMenus(outfitList, characterMenu, avatar);
        }

        EditorGUILayout.BeginVertical();
        scrollPose = EditorGUILayout.BeginScrollView(scrollPose);
        for(int i = 0; i < outfitList.Count; i++)
        {
            CreateOutfit(i);
        }

        if(GUILayout.Button("Add Outfit"))
        {
            outfitList.Add(new OutfitInventory());
            CreateOutfit(outfitList.Count - 1);
        }
        GUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void CreateOutfit(int index)
    {
        var currentItem = outfitList[index];
        currentItem.show = EditorGUILayout.Foldout(currentItem.show, currentItem.name);
        if (currentItem.show)
        {
            if(GUILayout.Button("Delete"))
            {
                outfitList.RemoveAt(index);
            }

            currentItem.index = index;
            GUILayout.Label("Index: " + currentItem.index);
            currentItem.name = EditorGUILayout.TextField("Outfit Nickname", currentItem.name);

            if (currentItem.itemsList.Count == 0)
                currentItem.itemsList.Add(new OutfitItemsList());

            for (int i = 0; i < currentItem.itemsList.Count; i++)
            {
                var currentSelection = currentItem.itemsList[i];
                processList(currentSelection.items);

                currentSelection.showCategory = EditorGUILayout.Foldout(currentSelection.showCategory, currentSelection.parameter + ", " + currentSelection.type + " (" + (currentSelection.items.Count - 1) + ")");
                if (currentSelection.showCategory)
                {
                    string[] stringParams = new string[parameters.parameters.Length];
                    string[] stringParamsType = new string[parameters.parameters.Length];
                    ParamsToString(stringParams, stringParamsType);

                    currentSelection.paramIndex = EditorGUILayout.Popup(currentSelection.paramIndex, stringParams);
                    currentSelection.parameter = stringParams[currentSelection.paramIndex];
                    currentSelection.type = stringParamsType[currentSelection.paramIndex];
                    for (int item = 0; item < currentSelection.items.Count; item++)
                        currentSelection.items[item] = EditorGUILayout.ObjectField(item.ToString(), currentSelection.items[item], typeof(GameObject), true) as GameObject;

                    if (GUILayout.Button("Delete Category"))
                        currentItem.itemsList.RemoveAt(i);
                }
            }

            if (GUILayout.Button("Add Category"))
            {
                currentItem.itemsList.Add(new OutfitItemsList());
            }
        }
    }

    private void processList(List<GameObject> currentItem, OutfitInventory currentInventory)
    {
        if (currentItem.Count == 0)
            currentItem.Add(default);

        if(currentItem[currentItem.Count - 1] != null)
            currentItem.Add(default);

        for (int i = 0; i < currentItem.Count; i++)
            if ((currentItem[i] == null) && (i < currentItem.Count - 1))
                currentItem.RemoveAt(i);
    }

    private void processList(List<GameObject> currentItem)
    {
        if (currentItem.Count == 0)
            currentItem.Add(default);

        if (currentItem[currentItem.Count - 1] != null)
            currentItem.Add(default);

        for (int i = 0; i < currentItem.Count; i++)
            if ((currentItem[i] == null) && (i < currentItem.Count - 1))
                currentItem.RemoveAt(i);
    }

    private void AddLayers(AnimatorController controller, List<OutfitInventory> outfits)
    {
        for(int i = 0; i < outfits.Count; i++)
        {
            for(int j = 0; j < outfits[i].itemsList.Count; j++)
            {
                if (outfits[i].itemsList[j].items.Count == 1 && outfits[i].itemsList[j].items[0] == null)
                    continue;
                string layerName = "Catfits - " + outfits[i].itemsList[j].parameter;
                AnimatorControllerLayer[] animLayers = controller.layers;
                if (layerExists(animLayers, layerName))
                {
                    Debug.Log(layerName + " exists");
                    continue;
                }
                controller.AddLayer("Catfits - " + outfits[i].itemsList[j].parameter);
                animLayers = controller.layers;
                animLayers[animLayers.Length - 1].defaultWeight = 1.0f;
                animLayers[animLayers.Length - 1].avatarMask = addMask ? CreateEmptyMask(avatar, "Assets/Yelby/Programs/Toggle/" + avatar.name) : null;
                controller.layers = animLayers;
            }
        }
    }

    private void AddParameters(AnimatorController controller, List<OutfitInventory> outfits)
    {
        for(int i = 0; i < outfits.Count; i++)
        {
            for(int j = 0; j < outfits[i].itemsList.Count; j++)
            {
                string parameter = outfits[i].itemsList[j].parameter;
                string parameterType = outfits[i].itemsList[j].type;
                AnimatorControllerParameterType paramType = AnimatorControllerParameterType.Int;
                switch (parameterType)
                {
                    case "Int":
                        paramType = AnimatorControllerParameterType.Int;
                        break;
                    case "Bool":
                        paramType = AnimatorControllerParameterType.Bool;
                        break;
                    case "Float":
                        paramType = AnimatorControllerParameterType.Float;
                        break;
                    case "Trigger":
                        paramType = AnimatorControllerParameterType.Trigger;
                        break;
                }
                if (ParameterExists(controller.parameters, parameter))
                {
                    int parameterIndex = ParameterIndex(controller.parameters, parameter);
                    if (controller.parameters[parameterIndex].type.ToString() != parameterType)
                    {
                        AnimatorControllerParameter[] parameters = controller.parameters;
                        parameters[parameterIndex].type = paramType;
                        controller.parameters = parameters;
                    }
                    continue;
                }
                controller.AddParameter(parameter, paramType);
            }
        }
    }

    private void CreateAnimations(GameObject avatar, List<OutfitInventory> outfits, List<string> paths)
    {
        List<string> parameters = GetParameters(outfits);
        string filePath = "Assets/Yelby/Programs/Toggle/" + avatar.name + "/";

        //Create Folders
        for(int i = 0; i < outfits.Count; i++)
        {
            string outfitName = outfits[i].name;
            CreateFolders(avatar, outfitName);
        }

        //Create Animations
        for (int i = 0; i < parameters.Count; i++)
        {
            for(int j = 0; j < outfits.Count; j++)
            {
                if(ParameterExists(outfits[j].itemsList, parameters[i]))
                    createAnimation(filePath, avatar, outfits, parameters[i], j, paths);
            }
        }
    }

    private void FillLayer(AnimatorController controller, List<string> paths, List<OutfitInventory> outfits)
    {
        List<string> parameters = GetParameters(outfits);
        for(int i = 0; i < parameters.Count; i++)
        {
            for(int j = 0; j < controller.layers.Length; j++)
            {
                if(controller.layers[j].name == "Catfits - " + parameters[i])
                {
                    AnimatorStateMachine states = controller.layers[j].stateMachine;
                    if (states.states.Length != 0)
                        states.states = null;

                    //Other
                    states.entryPosition = new Vector3(0, 0);
                    states.anyStatePosition = new Vector3(0, 50);
                    states.exitPosition = new Vector3(0, 100);

                    //Animations
                    List<Motion> motions = new List<Motion>();
                    for(int k = 0; k < paths.Count; k++)
                    {
                        if (paths[k].Contains(parameters[i]))
                        {
                            Debug.Log("FillLayerPath: " + paths[k]);
                            motions.Add(AssetDatabase.LoadAssetAtPath(paths[k], typeof(AnimationClip)) as Motion);
                        }
                    }

                    //States
                    List<AnimatorState> animatorStates = new List<AnimatorState>();
                    List<int> skip = new List<int>();
                    for(int k = 0; k < motions.Count; k++)
                    {
                        Vector3 location = new Vector3(250, 0);
                        for(int p = 0; p < outfits.Count; p++)
                        {
                            if(!skip.Contains(p) && ParameterExists(outfits[p].itemsList, parameters[i]))
                            {
                                location[1] = outfits[p].index * 50;
                                skip.Add(p);
                                break;
                            }
                        }
                        animatorStates.Add(createState(motions[k], states, location));
                    }

                    //Transitions
                    for(int k = 0; k < animatorStates.Count; k++)
                    {
                        createTransition(states, animatorStates[k], false, 0, AnimatorConditionMode.Equals, k, parameters[i]);
                    }
                }
            }
        }
    }

    private void GetCatfits(AnimatorController controller, List<OutfitInventory> outfits)
    {
        //Dictionary<int, string> outfitLayers = new Dictionary<int, string>();
        outfits.Clear();
        string catfits = "Catfits - ";
        int largestNumberGlobal = -1;
        for(int i = 0; i < controller.layers.Length; i++)
        {
            //Get layer
            if (!controller.layers[i].name.Contains(catfits))
                continue;
            string parameter = controller.layers[i].name.Substring(catfits.Length);
            Debug.Log("Parameter: " + parameter);

            AnimatorStateMachine states = controller.layers[i].stateMachine;

            int largestNumberLocal = -1;
            for (int k = 0; k < states.anyStateTransitions.Length; k++)
            {
                var currentCondition = states.anyStateTransitions[k].conditions;
                if ((int)currentCondition[0].threshold > largestNumberLocal)
                    largestNumberLocal = (int)currentCondition[0].threshold;
            }

            if (largestNumberGlobal < largestNumberLocal)
            {
                int temp = largestNumberGlobal;
                largestNumberGlobal = largestNumberLocal;
                if(outfits.Count < largestNumberGlobal)
                {
                    int difference = largestNumberGlobal - temp;
                    for(int k = 0; k < difference; k++)
                    {
                        outfits.Add(new OutfitInventory());
                    }
                }
            }

            //Get motions
            for (int k = 0; k < states.anyStateTransitions.Length; k++)
            {
                var currentTransition = states.anyStateTransitions[k].destinationState;
                var currentCondition = states.anyStateTransitions[k].conditions;

                //Generate Layers
                for (int p = 0; p < currentCondition.Length; p++)
                {
                    var currentFit = outfits[(int)currentCondition[p].threshold];
                    currentFit.index = (int)currentCondition[p].threshold;
                    currentFit.name = GetOutfitname(currentTransition.name);

                    //Item List
                    OutfitItemsList itemList = new OutfitItemsList();
                    itemList.parameter = parameter;
                    itemList.paramIndex = ParameterIndex(parameter);
                    Debug.Log(parameter + ":Parameter index: " + ParameterIndex(parameter));
                    itemList.type = "int";
                    itemList.items = PullItemsFromAnimation((AnimationClip)currentTransition.motion);
                    currentFit.itemsList.Add(itemList);
                }
            }
        }
    }

    private void CreateMenus(List<OutfitInventory> outfits, VRCMenu mainMenu, GameObject avatar)
    {
        string path = "Assets/Yelby/Programs/Toggle/";
        List<VRCMenu> menuList = new List<VRCMenu>();

        //Menu for each outfit
        for(int i = 0; i < outfitList.Count; i++)
        {
            menuList.Add(CreateInstance<VRCMenu>());
            FillMenu(menuList, menuList.Count - 1, 0, outfitList[i]);
            AssetDatabase.CreateAsset(menuList[i], path + avatar.name + "/" + outfitList[i].name + "/" + outfitList[i].name + ".asset");
        }
    }

    private void FillMenu(List<VRCMenu> menuList, int currentMenuIndex, int itemIndex, OutfitInventory outfitList)
    {
        var items = outfitList.itemsList;

        if(outfitList.itemsList.Count < VRCMenu.MAX_CONTROLS)
        {
            List<VRCMenu.Control> controls = new List<VRCMenu.Control>();
            for(int i = 0; i < items.Count; i++)
            {
                VRCMenu.Control control = new VRCMenu.Control
                {
                    name = items[i].parameter,
                    parameter = GetParameter(items[i].parameter),
                    value = outfitList.index,
                    type = VRCMenu.Control.ControlType.Toggle
                };
                controls.Add(control);
            }
            menuList[currentMenuIndex].controls = controls;
        }
    }

    private MenuParameter GetParameter(string parameterName)
    {
        var parameterList = avatar.GetComponent<Descriptor>().expressionParameters.parameters;

        MenuParameter param = new MenuParameter { name = parameterName };

        for (int i = 0; i < parameterList.Length; i++)
        {
            if (parameterList[i].name == parameterName)
            {
                param.name = parameterList[i].name;
                return param;
            }
        }
        return param;
    }

    //~~~~~Helper Methods~~~~~
    //Single Object
    private void createAnimation(string filePath, GameObject obj, bool toggle)
    {
        Keyframe[] keys = new Keyframe[1];
        AnimationCurve curve;
        string objectPath = obj.transform.GetHierarchyPath(null);
        objectPath = objectPath.Substring(avatar.name.Length + 1, objectPath.Length - avatar.name.Length - 1);

        AnimationClip clip = new AnimationClip();
        clip.legacy = false;

        keys[0] = new Keyframe(0.0f, (toggle ? 1.0f : 0.0f));
        curve = new AnimationCurve(keys);
        clip.SetCurve(objectPath, typeof(GameObject), "m_IsActive", curve);
        AssetDatabase.CreateAsset(clip, filePath + "/" + obj.name + (toggle ? "_ON" : "_OFF") + ".anim");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    //Based on bool list
    private void createAnimation(string filePath, List<GameObject> tObjs, List<bool> listOther, string outfitName)
    {
        AnimationClip clip = new AnimationClip();
        clip.legacy = false;
        Keyframe[] keys = new Keyframe[1];
        AnimationCurve curve;

        for(int i = 0; i < tObjs.Count; i++)
        {
            string objectPath = tObjs[i].transform.GetHierarchyPath(null);
            keys[0] = new Keyframe(0.0f, (listOther[i] ? 1.0f : 0.0f));
            curve = new AnimationCurve(keys);
            objectPath = objectPath.Substring(avatar.name.Length + 1, objectPath.Length - avatar.name.Length - 1);
            clip.SetCurve(objectPath, typeof(GameObject), "m_IsActive", curve);
        }
        
        AssetDatabase.CreateAsset(clip, filePath + "/" + outfitName + "_Default" + ".anim");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    //All items off then bool decides
    private void createAnimation(string filePath, GameObject tObj, List<GameObject> tObjs, bool toggle)
    {
        AnimationClip clip = new AnimationClip();
        clip.legacy = false;
        Keyframe[] keys = new Keyframe[1];
        AnimationCurve curve;

        for(int i = 0; i < tObjs.Count; i++)
        {
            string objectPath = tObjs[i].transform.GetHierarchyPath(null);
            if(tObjs[i].name == tObj.name)
                keys[0] = new Keyframe(0.0f, (toggle ? 1.0f : 0.0f));
            else
                keys[0] = new Keyframe(0.0f, 0.0f);
            curve = new AnimationCurve(keys);
            objectPath = objectPath.Substring(avatar.name.Length + 1, objectPath.Length - avatar.name.Length - 1);
            clip.SetCurve(objectPath, typeof(GameObject), "m_IsActive", curve);
        }

        AssetDatabase.CreateAsset(clip, filePath + "/" + tObj.name + (toggle ? "_ON" : "_OFF") + ".anim");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void createAnimation(string filePath, GameObject avatar, List<OutfitInventory> outfits, string parameter, int index, List<string> path)
    {
        AnimationClip clip = new AnimationClip();
        clip.legacy = false;
        Keyframe[] key = new Keyframe[1];
        AnimationCurve curve;
        string currentOutfit = outfits[index].name;

        for (int i = 0; i < outfits.Count; i++) // Outfits
        {
            for(int j = 0; j < outfits[i].itemsList.Count; j++) // Categories
            {
                if (outfits[i].itemsList[j].parameter != parameter)
                    continue;

                for(int k = 0; k < outfits[i].itemsList[j].items.Count; k++) // Items
                {
                    GameObject currentObject = outfits[i].itemsList[j].items[k];
                    if (currentObject == null)
                        continue;

                    string objectPath = currentObject.transform.GetHierarchyPath(null);
                    objectPath = objectPath.Substring(avatar.name.Length + 1, objectPath.Length - avatar.name.Length - 1);

                    key[0] = new Keyframe(0.0f, (i == index) ? 1.0f : 0.0f);
                    curve = new AnimationCurve(key);
                    clip.SetCurve(objectPath, typeof(GameObject), "m_IsActive", curve);
                }
            }
        }

        //Debug.Log(currentOutfit + ":" + index + " Count:" + outfits.Count);
        //AssetDatabase.CreateAsset(clip, filePath + "/" + currentOutfit + "/" + currentOutfit + "_" + parameter + ".anim");
        AssetDatabase.CreateAsset(clip, filePath + currentOutfit + "/" + currentOutfit + "_" + parameter + ".anim");
        path.Add(filePath + currentOutfit + "/" + currentOutfit + "_" + parameter + ".anim");
    }

    /*Creates a swap animation*/
    private void createAnimation(string path, GameObject obj, GameObject obj2, bool toggle)
    {
        AnimationClip clip = new AnimationClip();
        clip.legacy = false;
        Keyframe[] key = new Keyframe[1];
        AnimationCurve curve;

        string objectPath = getObjectPath(obj);
        string objectPath2 = getObjectPath(obj2);

        key[0] = new Keyframe(0.0f, (toggle ? 1.0f : 0.0f));
        curve = new AnimationCurve(key);
        clip.SetCurve(objectPath, typeof(GameObject), "m_IsActive", curve);

        key[0] = new Keyframe(0.0f, (toggle ? 0.0f : 1.0f));
        curve = new AnimationCurve(key);
        clip.SetCurve(objectPath2, typeof(GameObject), "m_IsActive", curve);

        AssetDatabase.CreateAsset(clip, path + "/" + (toggle ? obj.name : obj2.name) + "_ON" + ".anim");
    }

    private string getObjectPath(GameObject obj)
    {
        string objectPath = obj.transform.GetHierarchyPath(null);
        objectPath = objectPath.Substring(avatar.name.Length + 1, objectPath.Length - avatar.name.Length - 1);
        return objectPath;
    }

    private AnimatorState createState(Motion motion, AnimatorStateMachine stateMachine, Vector3 location)
    {
        stateMachine.AddState(motion.name, location);
        int i = 0;
        for (i = 0; i < stateMachine.states.Length; i++)
        {
            if (stateMachine.states[i].state.name == motion.name)
            {
                stateMachine.states[i].state.motion = motion;
                stateMachine.states[i].state.writeDefaultValues = writeDefaults;
                break;
            }
        }
        return stateMachine.states[i].state;
    }

    //Transition from one state to another
    private void createTransition(AnimatorState start, AnimatorState end, bool exitTime, float duration, AnimatorConditionMode mode, float threshold, GameObject obj)
    {
        AnimatorStateTransition transition = start.AddTransition(end);
        transition.hasExitTime = exitTime;
        transition.duration = duration;
        transition.AddCondition(mode, threshold, obj.name);
    }
    
    //Transition from state to exit
    private void createTransition(AnimatorState stateToExit, bool exitTime, float duration, AnimatorConditionMode mode, float threshold, GameObject obj)
    {
        AnimatorStateTransition tranExit = stateToExit.AddExitTransition();
        tranExit.hasExitTime = exitTime;
        tranExit.duration = duration;
        tranExit.AddCondition(mode, threshold, obj.name);
    }

    //Transition from ANY to state
    private void createTransition(AnimatorStateMachine anyToState, AnimatorState location, bool exitTime, float duration, AnimatorConditionMode mode, float threshold, string outfitName)
    {
        var anyState = anyToState.AddAnyStateTransition(location);
        anyState.hasExitTime = exitTime;
        anyState.duration = duration;
        anyState.AddCondition(mode, threshold, outfitName);
    }

    private List<bool> removeNull(List<GameObject> listGameobjects, List<bool> listOther)
    {
        //myList.RemoveAll(item => item == null);
        List<bool> list = new List<bool>();
        for(int i = 0; i < listGameobjects.Count; i++)
        {
            if(listGameobjects[i] != null)
            {
                list.Add(listOther[i]);
            }
        }
        return list;
    }

    private List<GameObject> removeNull(List<GameObject> listGameobjects)
    {
        //myList.RemoveAll(item => item == null);
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < listGameobjects.Count; i++)
        {
            if (listGameobjects[i] != null)
            {
                list.Add(listGameobjects[i]);
            }
        }
        return list;
    }

    private AvatarMask CreateEmptyMask(GameObject avatar, string filepath)
    {
        AvatarMask mask = AssetDatabase.LoadAssetAtPath(filepath + "/" + avatar.name + "_EMPTY" + ".mask", typeof(AvatarMask)) as AvatarMask;
        if (mask != null)
            return mask;
        else
            mask = new AvatarMask();

        SkeletonBone[] skeleton = avatar.GetComponent<Animator>().avatar.humanDescription.skeleton;

        List<Transform> boneTransforms = new List<Transform>();
        boneTransforms = GetAllTransforms(avatar.transform);

        for(int i = 0; i < skeleton.Length; i++)
        {
            for (int j = 0; j < boneTransforms.Count; j++)
            {
                if(boneTransforms[j].name == skeleton[i].name)
                {
                    mask.AddTransformPath(boneTransforms[j], false);
                    break;
                }
            }
        }

        for (int i = 0; i < mask.transformCount; i++)
            mask.SetTransformActive(i, false);

        for (int i = 0; i < mask.humanoidBodyPartCount; i++)
            mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);

        AssetDatabase.CreateAsset(mask, filepath + "/" + avatar.name + "_EMPTY" + ".mask");
        return mask;
    }

    private List<Transform> GetAllTransforms(Transform parent)
    {
        var transformList = new List<Transform>();
        BuildTransformList(transformList, parent);
        return transformList;
    }

    private static void BuildTransformList(ICollection<Transform> transforms, Transform parent)
    {
        if (parent == null) { return; }
        foreach (Transform t in parent)
        {
            transforms.Add(t);
            BuildTransformList(transforms, t);
        }
    }

    private void ParamsToString(string[] parameterArray, string[] typeArray)
    {
        for (int i = 0; i < parameters.parameters.Length; i++)
        {
            parameterArray[i] = parameters.parameters[i].name;
            typeArray[i] = parameters.parameters[i].valueType.ToString();
        }
        return;
    }

    private void ParamsToString(string[] parameterArray)
    {
        for (int i = 0; i < parameters.parameters.Length; i++)
        {
            parameterArray[i] = parameters.parameters[i].name;
        }
        return;
    }

    private bool layerExists(AnimatorControllerLayer[] layers, string layerName)
    {
        for(int i = 0; i < layers.Length; i++)
            if (layers[i].name == layerName)
                return true;
        return false;
    }

    private int layerIndex(AnimatorControllerLayer[] layers, string layerName)
    {
        for (int i = 0; i < layers.Length; i++)
            if (layers[i].name == layerName)
                return i;
        return -1;
    }

    private bool ParameterExists(AnimatorControllerParameter[] parameters, string parameterName)
    {
        for (int i = 0; i < parameters.Length; i++)
            if (parameters[i].name == parameterName)
                return true;
        return false;
    }

    private bool ParameterExists(List<OutfitItemsList> outfits, string parameterName)
    {
        for(int i = 0; i < outfits.Count; i++)
        {
            if (outfits[i].parameter == parameterName)
                return true;
        }
        return false;
    }

    private int ParameterIndex(AnimatorControllerParameter[] parameters, string parameterName)
    {
        for (int i = 0; i < parameters.Length; i++)
            if (parameters[i].name == parameterName)
                return i;
        return -1;
    }

    private List<string> GetParameters(List<OutfitInventory> outfits)
    {
        List<string> parameters = new List<string>();
        for(int i = 0; i < outfits.Count; i++)
        {
            for(int j = 0; j < outfits[i].itemsList.Count; j++)
            {
                string param = outfits[i].itemsList[j].parameter;
                if (parameters.Contains(param))
                    continue;
                parameters.Add(param);
            }
        }
        return parameters;
    }

    private string GetOutfitname(string name)
    {
        var temp = name.ToCharArray();
        string newName = "";
        for(int i = 0; i < temp.Length; i++)
        {
            if (temp[i] == '_')
                break;
            newName += temp[i];
        }

        return newName;
    }

    private List<GameObject> PullItemsFromAnimation(AnimationClip animation)
    {
        AnimationClipCurveData[] curve = AnimationUtility.GetAllCurves(animation);

        List<GameObject> objects = new List<GameObject>();

        for(int i = 0; i < curve.Length; i++)
        {
            if(curve[i].curve.keys[0].value == 1)
            {
                GameObject tempObject = avatar.transform.Find(curve[i].path).gameObject;
                if (tempObject == null)
                    Debug.LogError("Not Found");
                else
                {
                    objects.Add(tempObject);
                }
            }
        }

        return objects;
    }

    private int ParameterIndex (string parameterName)
    {
        string[] stringParams = new string[parameters.parameters.Length];

        ParamsToString(stringParams);

        for (int i = 0; i < parameters.parameters.Length; i++)
        {
            if (stringParams[i] == parameterName)
                return i;
        }
        return 0;
    }

    private VRCMenu GetCharacterMenu(GameObject avatar)
    {
        var descript = avatar.GetComponent<Descriptor>();
        return descript.expressionsMenu;
    }
}

public class OutfitInventory
{
    public bool show = true;
    public int index = 0;
    public string name = "Outfit";

    public List<OutfitItemsList> itemsList = new List<OutfitItemsList>();
}

public class OutfitItemsList
{
    public string parameter;
    public string type;

    public bool showCategory;
    public List<GameObject> items;

    public int paramIndex;

    public OutfitItemsList()
    {
        parameter = "ParameterName";
        type = "ParameterType";
        showCategory = true;
        items = new List<GameObject>();
        paramIndex = 0;
    }
}

#endif
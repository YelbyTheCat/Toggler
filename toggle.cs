#if VRC_SDK_VRCSDK3
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;
using ExpressionParameters = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters;
using ExpressionParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter;
using static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using UnityEditor.Animations;

public class toggle : EditorWindow
{
    //Created by Yelby

    //Attributes
    GameObject avatar;
    AnimatorController controller;
    ExpressionParameters parameters;
    GameObject obj;
    List<GameObject> objs = new List<GameObject>();
    List<bool> objsToggleAnimation = new List<bool>();
    List<bool> objsToggleUnity = new List<bool>();
    string outfitName = "";

    //Advanced Section
    bool options = false;
    bool saved = true;
    bool startActiveVRC = true;
    bool startActiveUnity = false;

    //Toolbar
    int toolBar = 0;
    string[] toolBarSections = { "Simple", "Any" };

    [MenuItem("Yelby/Toggler")]
    public static void ShowWindow()
    {
        GetWindow<toggle>("Toggler");
    }

    private void OnGUI()
    {
        GUILayout.Label("Version: 2.0");

        toolBar = GUILayout.Toolbar(toolBar, toolBarSections);

        //Gather info
        avatar = EditorGUILayout.ObjectField("Avatar: ", avatar, typeof(GameObject), true) as GameObject;
        if(avatar != null)
        {
            var SDKRef = avatar.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
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
                //Options
                options = EditorGUILayout.Foldout(options, "Options");
                if (options)
                {
                    saved = EditorGUILayout.Toggle("Saved", saved);
                    startActiveVRC = EditorGUILayout.Toggle("Default ON", startActiveVRC);
                    startActiveUnity = EditorGUILayout.Toggle("Unity ON", startActiveUnity);
                }

                //Make Toggle button
                if (avatar != null && controller != null && parameters != null && obj != null)
                {
                    if (GUILayout.Button("Make Toggle"))
                    {
                        CreateFolders(avatar, obj);
                        CreateToggleAnimations(avatar, obj);
                        AddAvatarParameters(parameters, obj, saved, startActiveVRC);
                        AddParameter(controller, obj);
                        AddLayer(avatar, controller, obj);
                        FillLayer(avatar, controller, obj, startActiveVRC);
                        obj.SetActive(startActiveUnity);
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
                outfitName = EditorGUILayout.TextField("Outfit Nickname", outfitName);
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

                saved = EditorGUILayout.Toggle("Saved", saved);

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
                    animLayers[tController.layers.Length - 1].avatarMask = CreateEmptyMask(avatar, "Assets/Yelby/Programs/Toggle/" + avatar.name);
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
        animLayers[tController.layers.Length - 1].avatarMask = CreateEmptyMask(avatar, "Assets/Yelby/Programs/Toggle/" + avatar.name);
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
                    animLayers[tController.layers.Length - 1].avatarMask = CreateEmptyMask(avatar, "Assets/Yelby/Programs/Toggle/" + avatar.name);
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
        animLayers[tController.layers.Length - 1].avatarMask = CreateEmptyMask(avatar, "Assets/Yelby/Programs/Toggle/" + avatar.name);
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

    private AnimatorState createState(Motion motion, AnimatorStateMachine stateMachine, Vector3 location)
    {
        stateMachine.AddState(motion.name, location);
        int i = 0;
        for (i = 0; i < stateMachine.states.Length; i++)
        {
            if (stateMachine.states[i].state.name == motion.name)
            {
                stateMachine.states[i].state.motion = motion;
                stateMachine.states[i].state.writeDefaultValues = false;
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
}
#endif

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
    //Attributes
    GameObject avatar;
    AnimatorController controller;
    ExpressionParameters parameters;
    GameObject obj;

    //Advanced Section
    bool options = false;
    bool saved = true;
    bool startActiveVRC = true;
    bool startActiveUnity = false;

    [MenuItem("Yelby/Toggles")]
    public static void ShowWindow()
    {
        GetWindow<toggle>("Toggles");
    }

    private void OnGUI()
    {
        GUILayout.Label("Version: 1.5");

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

                //Object
                obj = EditorGUILayout.ObjectField("Object: ", obj, typeof(GameObject), true) as GameObject;
            }
            else
            {
                avatar = null;
            }
        }

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
                AddLayer(controller, obj);
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
    }

    //~~~~~Methods~~~~~
    private void CreateFolders(GameObject avatar, GameObject tObj)
    {
        string path = "Assets/Yelby/Programs/Toggle";
        if(!AssetDatabase.IsValidFolder(path + "/" + avatar.name))
        {
            AssetDatabase.CreateFolder(path, avatar.name);
            path += avatar.name;
            Debug.Log("Folder: " + path + " created");
        }
    }

    private void CreateToggleAnimations(GameObject avatar, GameObject tObj)
    {
        string path = "Assets/Yelby/Programs/Toggle/" + avatar.name;
        if (!AssetDatabase.IsValidFolder(path))
        {
            Debug.LogError(path + " not available");
            return;
        }

        Keyframe[] keys = new Keyframe[1];
        AnimationCurve curve;
        string objectPath = tObj.transform.GetHierarchyPath(null);
        objectPath = objectPath.Substring(avatar.name.Length+1, objectPath.Length-avatar.name.Length-1);

        //Off
        AnimationClip clipF = new AnimationClip();
        clipF.legacy = false;

        keys[0] = new Keyframe(0.0f, 0.0f);
        curve = new AnimationCurve(keys);
        clipF.SetCurve(objectPath, typeof(GameObject), "m_IsActive", curve);
        AssetDatabase.CreateAsset(clipF, path + "/" + tObj.name + "_OFF" + ".anim");

        AssetDatabase.Refresh();

        //ON
        AnimationClip clipO = new AnimationClip();
        clipO.legacy = false;

        keys[0] = new Keyframe(0.0f, 1.0f);
        curve = new AnimationCurve(keys);
        clipO.SetCurve(objectPath, typeof(GameObject), "m_IsActive", curve);
        AssetDatabase.CreateAsset(clipO, path + "/" + tObj.name + "_ON" + ".anim");

        AssetDatabase.Refresh();

        string temp = "Assets/Yelby/Programs/Toggle/" + avatar.name + "/" + "test_OFF";
        AnimationClip test = AssetDatabase.LoadAssetAtPath(temp, typeof(AnimationClip)) as AnimationClip;

        
    }

    private void AddAvatarParameters(ExpressionParameters tParams, GameObject tObj, bool saved, bool def)
    {
        //Check if parameter already exists
        int size = tParams.parameters.Length;
        var paraName = tParams.parameters;
        for(int i = 0; i < size; i++)
        {
            if(tObj.name == paraName[i].name)
            {
                Debug.LogWarning("Avatar Parameter: " + tObj.name + " already exists");
                bool option = EditorUtility.DisplayDialog("Toggles", "Avatar Parameter " + tObj.name + " already exists. Override?", "Yes", "No");
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
        ExpressionParameter[] parameterArray = new ExpressionParameter[size + 1];
        for (int i = 0; i < size; i++) //Copy parameter
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
        parameterArray[size] = parameter;
        tParams.parameters = parameterArray;
        Debug.Log("Avatar Parameter: " + tObj.name + " created");

        //EditorUtility.SetDirty(parameters);
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
                bool option = EditorUtility.DisplayDialog("Toggles", "Layer Parameter " + tObj.name + " already exists. Override?", "Yes", "No");
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
        AssetDatabase.Refresh();
    }

    private void AddLayer(AnimatorController tController, GameObject tObj)
    {
        int size = tController.layers.Length;
        AnimatorControllerLayer[] animLayers = tController.layers;

        //Check list if a name exists
        for (int i = 0; i < size; i++)
        {
            if(tObj.name == animLayers[i].name)
            {
                Debug.LogWarning("Layer: " + tObj.name + " already exists");
                bool option = EditorUtility.DisplayDialog("Toggles", "Layer Name " + tObj.name + " already exists. Override?", "Yes", "No");
                if(option)
                {
                    tController.RemoveLayer(i);
                    tController.AddLayer(tObj.name);
                    animLayers = tController.layers;
                    animLayers[i].defaultWeight = 1.0f;
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
        animLayers[size].defaultWeight = 1.0f;
        tController.layers = animLayers;
        Debug.Log("Layer: " + tObj.name + " created");
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
            }
        }

        //Create States
        string path = "Assets/Yelby/Programs/Toggle/" + avatar.name + "/" + tObj.name;
        AnimatorStateMachine states = tController.layers[tLayer].stateMachine;

        AnimatorState defState = new AnimatorState();
        AnimatorState tranState = new AnimatorState();

        //Default State
        if (def)
        {
            Motion defMotion = AssetDatabase.LoadAssetAtPath(path + "_OFF" + ".anim", typeof(AnimationClip)) as Motion;
            
            defState.name = defMotion.name;
            defState.motion = defMotion;
            defState.writeDefaultValues = false;
            states.AddState(defState, new Vector3(300, 120));

            //Transitioned to state
            Motion tranMotion = AssetDatabase.LoadAssetAtPath(path + "_ON" + ".anim", typeof(AnimationClip)) as Motion;
            tranState.name = tranMotion.name;
            tranState.motion = tranMotion;
            tranState.writeDefaultValues = false;
            states.AddState(tranState, new Vector3(300, 170));
        }
        else
        {
            Motion defMotion = AssetDatabase.LoadAssetAtPath(path + "_ON" + ".anim", typeof(AnimationClip)) as Motion;
            defState.name = defMotion.name;
            defState.motion = defMotion;
            defState.writeDefaultValues = false;
            states.AddState(defState, new Vector3(300, 120));

            //Transitioned to state
            Motion tranMotion = AssetDatabase.LoadAssetAtPath(path + "_OFF" + ".anim", typeof(AnimationClip)) as Motion;
            tranState.name = tranMotion.name;
            tranState.motion = tranMotion;
            tranState.writeDefaultValues = false;
            states.AddState(tranState, new Vector3(300, 170));
        }
        

        //Other
        states.anyStatePosition = new Vector3(325, 0);
        states.entryPosition = new Vector3(325, 50);
        states.exitPosition = new Vector3(325, 240);

        //Transitions
        if(def)
        {
            AnimatorStateTransition defTransition = defState.AddTransition(tranState);
            defTransition.hasExitTime = false;
            defTransition.duration = 0.0f;
            defTransition.AddCondition(AnimatorConditionMode.If, 0, tObj.name);

            AnimatorStateTransition tranExit = tranState.AddExitTransition();
            tranExit.hasExitTime = false;
            tranExit.duration = 0.0f;
            tranExit.AddCondition(AnimatorConditionMode.IfNot, 0, tObj.name);
        }
        else
        {
            AnimatorStateTransition defTransition = defState.AddTransition(tranState);
            defTransition.hasExitTime = false;
            defTransition.duration = 0.0f;
            defTransition.AddCondition(AnimatorConditionMode.IfNot, 0, tObj.name);

            AnimatorStateTransition tranExit = tranState.AddExitTransition();
            tranExit.hasExitTime = false;
            tranExit.duration = 0.0f;
            tranExit.AddCondition(AnimatorConditionMode.If, 0, tObj.name);
        }
        
    }
}
#endif

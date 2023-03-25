using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ExpressionParameters = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters;
using ExpressionParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter;
using VRCMenu = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu;
using UnityEditor.Animations;

public class helperMethods
{
    // Create Animations
    public static void createAnimation(string filePath)
    {
        var empty = AssetDatabase.LoadAssetAtPath(filePath + "/EMPTY.anim", typeof(AnimationClip)) as Motion;
        if (empty != null) return;
        Keyframe[] keys = new Keyframe[1];
        AnimationCurve curve;
        string objectPath = "EMPTYFORREASONS";

        AnimationClip clip = new AnimationClip();
        clip.legacy = false;

        keys[0] = new Keyframe(0.0f, 0.0f);
        curve = new AnimationCurve(keys);
        clip.SetCurve(objectPath, typeof(GameObject), "m_IsActive", curve);
        AssetDatabase.CreateAsset(clip, filePath + "/" + "EMPTY" + ".anim");
    }

    public static void createAnimation(string avatarName, List<ObjectInformation> objectsList, string parameterName, string type, string filePath, bool dissolve)
    {
        // Create Simple toggle
        if (objectsList.Count == 1)
        {
            GameObject item = objectsList[0].item;
            filePath += "/" + avatarName + "/" + type;

            if (dissolve)
            {
                bool toggle = true;
                string objectPath = getObjectPath(avatarName, item);
                for (int i = 0; i < 2; i++)
                {
                    // Start
                    AnimationClip clip = new AnimationClip { legacy = false };

                    // Toggle
                    Keyframe[] toggleKeys = new Keyframe[2];
                    toggleKeys[0] = new Keyframe(0.0f, 1.0f);
                    toggleKeys[1] = new Keyframe(1.0f, (toggle ? 1.0f : 0.0f));
                    clip.SetCurve(objectPath, typeof(GameObject), "m_IsActive", new AnimationCurve(toggleKeys));

                    // Dissolve
                    Keyframe[] dissolveKeys = new Keyframe[2];
                    dissolveKeys[0] = new Keyframe(0.0f, (toggle ? 1.0f : 0.0f));
                    dissolveKeys[1] = new Keyframe(1.0f, (toggle ? 0.0f : 1.0f));
                    clip.SetCurve(objectPath, typeof(MeshRenderer), "material._DissolveAlpha", new AnimationCurve(dissolveKeys));

                    // End
                    AssetDatabase.CreateAsset(clip, filePath + "/Dissolve_" + item.name + (toggle ? "_IN" : "_OUT") + ".anim");
                    toggle = !toggle;
                }

                // Initial States
                toggle = true;
                for (int i = 0; i < 2;i++)
                {
                    // Start
                    AnimationClip clip = new AnimationClip { legacy = false };

                    // Toggle
                    Keyframe[] toggleKeys = new Keyframe[1];
                    toggleKeys[0] = new Keyframe(0.0f, toggle ? 1.0f : 0.0f);
                    clip.SetCurve(objectPath, typeof(GameObject), "m_IsActive", new AnimationCurve(toggleKeys));

                    // Dissolve
                    // Dissolve
                    Keyframe[] dissolveKeys = new Keyframe[2];
                    dissolveKeys[0] = new Keyframe(0.0f, toggle ? 0.0f : 1.0f);
                    clip.SetCurve(objectPath, typeof(MeshRenderer), "material._DissolveAlpha", new AnimationCurve(dissolveKeys));

                    // End
                    AssetDatabase.CreateAsset(clip, filePath + "/Dissolve_Initial_" + item.name + (toggle ? "_IN" : "_OUT") + ".anim");
                    toggle = !toggle;
                }
            }
            else
            {
                bool toggle = true;
                for (int i = 0; i < 2; i++)
                {
                    Keyframe[] keys = new Keyframe[1];
                    AnimationCurve curve;
                    string objectPath = getObjectPath(avatarName, item);
                    AnimationClip clip = new AnimationClip();
                    clip.legacy = false;
                    keys[0] = new Keyframe(0.0f, (toggle ? 1.0f : 0.0f));
                    curve = new AnimationCurve(keys);
                    clip.SetCurve(objectPath, typeof(GameObject), "m_IsActive", curve);
                    AssetDatabase.CreateAsset(clip, filePath + "/" + item.name + (toggle ? "_ON" : "_OFF") + ".anim");
                    toggle = !toggle;
                }
            }
            return;
        }

        // Travel the entire list
        filePath += "/" + avatarName + "/" + type;
        for (int i = 0; i <= objectsList.Count; i++)
        {
            GameObject currentItem = null;
            if (i < objectsList.Count)
                currentItem = objectsList[i].item;
            AnimationClip clip = new AnimationClip();
            Keyframe[] keys = new Keyframe[1];
            AnimationCurve curve;

            // Travel list turning stuff off
            for(int j = 0; j < objectsList.Count; j++)
            {
                GameObject tempItem = objectsList[j].item;
                keys[0] = new Keyframe(0.0f, (currentItem == tempItem ? 1.0f : 0.0f));
                curve = new AnimationCurve(keys);
                string objectPath = getObjectPath(avatarName, objectsList[j].item);
                clip.SetCurve(objectPath, typeof(GameObject), "m_IsActive", curve);
            }
            if (i < objectsList.Count)
                AssetDatabase.CreateAsset(clip, filePath + "/" + currentItem.name + "_ON" + (type == "bool" ? "_SWAP" : "") + ".anim");
            else
                AssetDatabase.CreateAsset(clip, filePath + "/" + parameterName + "_naked" + ".anim");
        }
    }
    
    public static string getObjectPath(string avatarName, GameObject item)
    {
        string objectPath = item.transform.GetHierarchyPath(null);
        return objectPath.Substring(avatarName.Length + 1, objectPath.Length - avatarName.Length - 1);
    }

    // Create VRC Parameters
    public static void createParameter(ExpressionParameters parameters, string parameterName, float parameterDefaultValue, bool save, string type)
    {
        // Set type of parameter is being made
        var valueType = ExpressionParameters.ValueType.Bool;
        switch (type)
        {
            case "int":
                valueType = ExpressionParameters.ValueType.Int;
                break;
            case "float":
                valueType = ExpressionParameters.ValueType.Float;
                break;
        }

        // Find parameter if it exists and update the values
        for(int i = 0; i < parameters.parameters.Length; i++)
        {
            ExpressionParameter currentParameter = parameters.parameters[i];
            if(currentParameter.name == parameterName)
            {
                currentParameter.valueType = valueType;
                currentParameter.defaultValue = (type == "int" || type == "float" ? 0 : parameterDefaultValue);
                currentParameter.saved = save;
                return;
            }
        }

        // Increase parameter list size
        ExpressionParameter[] tempParameters = new ExpressionParameter[parameters.parameters.Length + 1];
        for(int i = 0; i < parameters.parameters.Length; i++)
            tempParameters[i] = parameters.parameters[i];

        // Create new parameter
        tempParameters[tempParameters.Length - 1] = new ExpressionParameter
        {
            name = parameterName,
            valueType = valueType,
            saved = save,
            defaultValue = (type == "int" || type == "float" ? 0 : parameterDefaultValue)
        };

        parameters.parameters = tempParameters;
        return;
    }

    // Create FX Parameter
    public static void createFXParameter(AnimatorController controllerFX, bool isDefault, string parameterName, string type)
    {
        // Set type
        AnimatorControllerParameter newParameter = new AnimatorControllerParameter();
        newParameter.name = parameterName;
        //var valueType = AnimatorControllerParameterType.Bool;
        switch (type)
        {
            case "int":
                //valueType = AnimatorControllerParameterType.Int;
                newParameter.type = AnimatorControllerParameterType.Int;
                newParameter.defaultInt = 0;
                break;
            case "float":
                //valueType = AnimatorControllerParameterType.Float;
                newParameter.type = AnimatorControllerParameterType.Float;
                newParameter.defaultFloat = 0;
                break;
            case "trigger":
                //valueType = AnimatorControllerParameterType.Trigger;
                newParameter.type = AnimatorControllerParameterType.Trigger;
                break;
            case "bool":
                newParameter.type = AnimatorControllerParameterType.Bool;
                newParameter.defaultBool = isDefault;
                break;
        }

        // Add fx 
        var parameter = controllerFX.parameters;
        for(int i = 0; i < parameter.Length; i++)
        {
            if (parameter[i].name == parameterName)
            {
                controllerFX.RemoveParameter(parameter[i]);
                break;
            }
        }

        //Add in
        controllerFX.AddParameter(newParameter);
    }

    // Create FX Layer
    public static void createFXLayer(AnimatorController controllerFX, string objectToggleName)
    {
        AnimatorControllerLayer[] layers = controllerFX.layers;

        for(int i = 0; i < layers.Length; i++)
        {
            if(layers[i].name == objectToggleName)
            {
                controllerFX.RemoveLayer(i);
                break;
            }
        }

        controllerFX.AddLayer(objectToggleName);
        layers = controllerFX.layers;
        var currentLayer = layers[layers.Length - 1];
        currentLayer.defaultWeight = 1.0f;

        controllerFX.layers = layers;
    }

    // Fill FX Layers
    public static void fillFXLayer(string avatarName, AnimatorController controllerFX, string parameterName, List<ObjectInformation> objectsList, bool defaultState, string type, bool writeDefaults, string filePath, bool dissolve)
    {
        // Get layer index
        int layerIndex = getLayerIndex(controllerFX.layers, parameterName);
        if (layerIndex == -1) return;

        // Get state machine
        AnimatorControllerLayer activeLayer = controllerFX.layers[layerIndex];
        AnimatorStateMachine stateMachine = activeLayer.stateMachine;

        // Reposition Default States
        Vector3 location = new Vector3(0, -50);
        int nodeOffset = 220;
        stateMachine.anyStatePosition = location;
        location[1] += 50;
        stateMachine.entryPosition = location;
        location[0] += 3 * nodeOffset;
        stateMachine.exitPosition = location;

        location = new Vector3(nodeOffset, 0);
        // Create nodes
        Motion motion = null;
        string path = filePath + "/" + avatarName + "/" + type + "/";

        // Simple Toggle Dissolve
        if (type == "bool" && objectsList.Count == 1)
        {
            stateMachine.anyStatePosition = new Vector3(50, 80);
            stateMachine.entryPosition = new Vector3(50, 120);
            stateMachine.exitPosition = new Vector3(50, 160);

            string emptyLocation = filePath + "/" + avatarName + "/";
            string objectName = objectsList[0].item.name;

            // Wait
            motion = AssetDatabase.LoadAssetAtPath(emptyLocation + "EMPTY.anim", typeof(AnimationClip)) as Motion;
            if (motion == null) { Debug.LogError("Failed to fetch EMPTY"); return; }
            AnimatorState waitNode = createNode(motion, stateMachine, new Vector3(250, 120, 0), writeDefaults);

            // Initial State ON
            motion = AssetDatabase.LoadAssetAtPath(path + "Dissolve_Initial_" + objectName + "_IN.anim", typeof(AnimationClip)) as Motion;
            if (motion == null) { Debug.LogError("Failed to fetch Dissolve Initial IN"); return; }
            AnimatorState initialIn = createNode(motion, stateMachine, new Vector3(250, 40, 0), writeDefaults);

            // Initial State OFF
            motion = AssetDatabase.LoadAssetAtPath(path + "Dissolve_Initial_" + objectName + "_OUT.anim", typeof(AnimationClip)) as Motion;
            if (motion == null) { Debug.LogError("Failed to fetch Dissolve initial OUT"); return; }
            AnimatorState initialOut = createNode(motion, stateMachine, new Vector3(250, 200, 0), writeDefaults);

            // Dissolve IN
            motion = AssetDatabase.LoadAssetAtPath(path + "Dissolve_" + objectName + "_IN.anim", typeof(AnimationClip)) as Motion;
            if (motion == null) { Debug.LogError("Failed to fetch Dissolve IN"); return; }
            AnimatorState dissolveIn = createNode(motion, stateMachine, new Vector3(490, 80, 0), writeDefaults);

            // Dissolve OFF_Cube_IN
            motion = AssetDatabase.LoadAssetAtPath(path + "Dissolve_" + objectName + "_OUT.anim", typeof(AnimationClip)) as Motion;
            if (motion == null) { Debug.LogError("Failed to fetch Dissolve OUT"); return; }
            AnimatorState dissolveOut = createNode(motion, stateMachine, new Vector3(490, 160, 0), writeDefaults);

            // Transitions

            // Wait to initials
            createTransition(waitNode, initialIn, defaultState ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, defaultState, parameterName);
            createTransition(waitNode, initialOut, defaultState ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, defaultState, parameterName);

            // Initial IN to Dissolve OUT
            createTransition(initialIn, dissolveOut, defaultState ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, defaultState, parameterName);

            // Initial OUT to Dissolve IN
            createTransition(initialOut, dissolveIn, defaultState ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, defaultState, parameterName);

            // Dissolve IN to Dissolve OUT
            createTransition(dissolveIn, dissolveOut, defaultState ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, defaultState, parameterName);

            // Dissolve OUT to Dissolve IN
            createTransition(dissolveOut, dissolveIn, defaultState ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, defaultState, parameterName);
            return;
        }

        // Simple Toggle
        if (type == "bool" && objectsList.Count == 1)
        {
            path += objectsList[0].item.name;
            // Start
            motion = AssetDatabase.LoadAssetAtPath(path + (defaultState ? "_ON" : "_OFF") + ".anim", typeof(AnimationClip)) as Motion;
            if (motion == null) return;
            AnimatorState startNode = createNode(motion, stateMachine, location, writeDefaults);

            // End
            motion = AssetDatabase.LoadAssetAtPath(path + (!defaultState ? "_ON" : "_OFF") + ".anim", typeof(AnimationClip)) as Motion;
            if (motion == null) return;
            location[0] += nodeOffset;
            AnimatorState endNode = createNode(motion, stateMachine, location, writeDefaults);

            // Transitions
            createTransition(startNode, endNode, defaultState ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, defaultState, parameterName);
            createTransition(endNode, !defaultState ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, defaultState, parameterName);
            return;
        }

        // Swap Toggle
        if (type == "bool" && objectsList.Count == 2)
        {
            // Start
            motion = AssetDatabase.LoadAssetAtPath(path + objectsList[0].item.name + "_ON_SWAP.anim", typeof(AnimationClip)) as Motion;
            if (motion == null) return;
            AnimatorState startNode = createNode(motion, stateMachine, location, writeDefaults);

            // End
            motion = AssetDatabase.LoadAssetAtPath(path + objectsList[1].item.name + "_ON_SWAP.anim", typeof(AnimationClip)) as Motion;
            if (motion == null) return;
            location[0] += nodeOffset;
            AnimatorState endNode = createNode(motion, stateMachine, location, writeDefaults);

            // Transitions
            createTransition(startNode, endNode, defaultState ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, defaultState, parameterName);
            createTransition(endNode, !defaultState ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, defaultState, parameterName);
            return;
        }

        // Any Toggle
        if (type == "int" && objectsList.Count >= 2)
        {
            // Idle Node
            motion = AssetDatabase.LoadAssetAtPath(filePath + "/Empty.anim", typeof(AnimationClip)) as Motion;
            if (motion == null) return;
            AnimatorState idleNode = createNode(motion, stateMachine, location, writeDefaults);

            // Int Nodes
            location[0] += nodeOffset;
            for(int i = 0; i < objectsList.Count; i++)
            {
                AnimatorState currentNode = null;
                if(i == 0)
                {
                    Motion motion2 = null;
                    if(!objectsList[i].isDefault)
                    {
                        motion = AssetDatabase.LoadAssetAtPath(path + "/" + parameterName + "_naked.anim", typeof(AnimationClip)) as Motion;
                        motion2 = AssetDatabase.LoadAssetAtPath(path + "/" + objectsList[i].item.name + "_ON.anim", typeof(AnimationClip)) as Motion;
                    }
                    else
                    {
                        motion2 = AssetDatabase.LoadAssetAtPath(path + "/" + parameterName + "_naked.anim", typeof(AnimationClip)) as Motion;
                        motion = AssetDatabase.LoadAssetAtPath(path + "/" + objectsList[i].item.name + "_ON.anim", typeof(AnimationClip)) as Motion;
                    }

                    if (motion == null) return;
                    currentNode = createNode(motion, stateMachine, location, writeDefaults);
                    location[1] += 50;

                    // Transitions
                    createTransition(idleNode, currentNode, AnimatorConditionMode.Equals, i, parameterName);
                    createTransition(currentNode, AnimatorConditionMode.NotEqual, i, parameterName);

                    // Flipped Above ----------------------------
                    if (motion2 == null) return;
                    currentNode = createNode(motion2, stateMachine, location, writeDefaults);
                    location[1] += 50;

                    // Transitions
                    createTransition(idleNode, currentNode, AnimatorConditionMode.Equals, i + 1, parameterName);
                    createTransition(currentNode, AnimatorConditionMode.NotEqual, i + 1, parameterName);
                    continue;
                }

                motion = AssetDatabase.LoadAssetAtPath(path + "/" + objectsList[i].item.name + "_ON.anim", typeof(AnimationClip)) as Motion;
                if (motion == null) return;

                currentNode = createNode(motion, stateMachine, location, writeDefaults);
                location[1] += 50;

                // Transitions
                createTransition(idleNode, currentNode, AnimatorConditionMode.Equals, i + 1, parameterName);
                createTransition(currentNode, AnimatorConditionMode.NotEqual, i + 1, parameterName);
            }
            return;
        }
    }

    // Set visibility
    public static void setVisibility(List<ObjectInformation> objectsList)
    {
        for(int i = 0; i < objectsList.Count; i++)
            objectsList[i].item.SetActive(objectsList[i].isUnity);
    }

    public static void addParameterMenuEntry(List<ObjectInformation> objectsList, string parameterName, string type)
    {
        for(int i = 0; i < objectsList.Count; i++)
        {
            var obj = objectsList[i];
            if (obj.menu == null)
                continue;

            VRCMenu menu = obj.menu;
            EditorUtility.SetDirty(menu);
            VRCMenu.Control control = new VRCMenu.Control();
            switch(type)
            {
                case "int":
                    control.type = VRCMenu.Control.ControlType.Toggle;
                    control.value = obj.isDefault ? 0 : (i + 1);
                    control.name = obj.item.name;
                    control.parameter = new VRCMenu.Control.Parameter { name = parameterName };
                    break;
                case "bool":
                    control.type = VRCMenu.Control.ControlType.Toggle;
                    control.name = obj.item.name;
                    control.parameter = new VRCMenu.Control.Parameter { name = obj.item.name };
                    break;
            }
            menu.controls.Add(control);
            obj.menu = menu; // Needed for saving apparently
        }
        AssetDatabase.SaveAssets();
    }

    // Validations ----------------------------------------------
    public static bool checkParent(GameObject parent, GameObject child)
    {
        Transform temp = child.transform;

        while (temp != null)
        {
            if (temp.parent == parent.transform)
                return true;

            temp = temp.parent;
        }

        return false;
    }

    public static int getLayerIndex(AnimatorControllerLayer[] layers, string layerName)
    {
        for(int i = 0; i < layers.Length; i++)
        {
            if (layers[i].name == layerName)
                return i;
        }
        return -1;
    }

    public static bool contains(List<MenuCounter> menuList, VRCMenu menuTarget, out int index)
    {
        for(int i = 0; i < menuList.Count; i++)
        {
            if(menuList[i].menu == menuTarget)
            {
                index = i;
                return true;
            }
        }
        index = -1;
        return false;
    }

    public static int count(List<MenuCounter> menuList, VRCMenu menuTarget)
    {
        for(int i = 0; i < menuList.Count; i++)
        {
            if(menuList[i].menu == menuTarget) 
                return menuList[i].count;
        }
        return 0;
    }

    // Nodes ------------------------------------------------------
    private static AnimatorState createNode(Motion motion, AnimatorStateMachine stateMachine, Vector3 location, bool writeDefaults)
    {
        AnimatorState state = stateMachine.AddState(motion.name, location);
        state.motion = motion;
        state.writeDefaultValues = writeDefaults;

        return state;
    }

    // Transitions -------------------------------------------------
    private static void createTransition(AnimatorState start, AnimatorState end, AnimatorConditionMode mode, bool on, string objectToggleName)
    {
        AnimatorStateTransition transition = start.AddTransition(end);
        transition.hasExitTime = false;
        transition.exitTime = 0;
        transition.duration = 0;

        transition.AddCondition(mode, on ? 1f : 0f, objectToggleName);
    }

    private static void createTransition(AnimatorState node1, AnimatorState node2, AnimatorConditionMode mode, int index, string parameterName)
    {
        AnimatorStateTransition transition = node1.AddTransition(node2);
        transition.hasExitTime = false;
        transition.exitTime = 0;
        transition.duration = 0;
        transition.AddCondition(mode, index, parameterName);
    }

    private static void createTransition(AnimatorState toExit, AnimatorConditionMode mode, bool on, string objectToggleName)
    {
        AnimatorStateTransition transition = toExit.AddExitTransition();
        transition.hasExitTime = false;
        transition.exitTime = 0;
        transition.duration = 0;
        transition.AddCondition(mode, on ? 1f : 0f, objectToggleName);
    }

    private static void createTransition(AnimatorState toExit, AnimatorConditionMode mode, int index, string parameterName)
    {
        AnimatorStateTransition transition = toExit.AddExitTransition();
        transition.hasExitTime = false;
        transition.exitTime = 0;
        transition.duration = 0;
        transition.AddCondition(mode, index, parameterName);
    }

    // Other ------------------------------------------------
    public static bool checkList(List<ObjectInformation> objectsList, GameObject gameObject)
    {
        for (int i = 0; i < objectsList.Count; i++)
            if (objectsList[i].item == gameObject)
                return true;
        return false;
    }

    public static bool hasDouble(List<ObjectInformation> objectsList, GameObject gameObject, out int index)
    {
        index = -1;
        int count = 0;
        for (int i = 0; i < objectsList.Count; i++)
        {
            if (objectsList[i].item == gameObject)
            {
                count++;
                if(count > 1)
                {
                    index = i;
                    break;
                }
            }
        }
        return count > 1;
    }

    public static List<ObjectInformation> reverse(List<ObjectInformation> objectsList)
    {
        List<ObjectInformation> reversed = new List<ObjectInformation>();
        for(int i = objectsList.Count - 1; i >= 0; i--)
        {
            objectsList[i].isUnity = objectsList[i].isUnity ? false : true;
            reversed.Add(objectsList[i]);
        }
        return reversed;
    }
}
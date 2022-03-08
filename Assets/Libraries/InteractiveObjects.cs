using Assets.InteractiveObject;
using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InteractiveObjects : ScriptableObject
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/InteractiveObjects")]
    public static void CreateMyAsset()
    {
        InteractiveObjects asset = ScriptableObject.CreateInstance<InteractiveObjects>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/InteractiveObjects.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
#endif
    // todo: first return only state of the object, once the world is created, instantiate gameObject from prefab
    [SerializeField]
    InteractiveObject gravePrefab;

    [SerializeField]
    InteractiveObject ascensionKilnPrefab;

    [SerializeField]
    Transform elevatorPrefab;

    [SerializeField]
    InteractiveObject leverPrefab;

    public InteractiveObject Grave()
    {
        var state = new Grave()
        {
            Name = "Grave",
            MessageOnInteract = "Grave chosen",
            InteractionDescription = "Choose Grave",
            InteractOptions = new InteractOptions()
                .AddOption("Take candle", agent => Debug.Log("Taking candle"))
                .AddOption("Put candle", agent => Debug.Log("Putting candle"))
        };
        var obj = Instantiate(gravePrefab);
        obj.State = state;

        return obj;
    }

    public InteractiveObject AscensionKiln()
    {
        var state = new AscensionKiln()
        {
            Name = "Ascension Kiln",
            MessageOnInteract = "Will increased",
            InteractionDescription = "Increase will"
        };
        var obj = Instantiate(ascensionKilnPrefab);
        obj.State = state;

        return obj;
    }

    public ElevatorState Elevator(float height, bool up)
    {
        var state = new ElevatorState(height, up);
        var obj = Instantiate(elevatorPrefab);
        state.Object = obj;

        var lever = Lever(state.Activate);
        var leverSlot = obj.FindRecursive("LeverSlot");
        lever.transform.SetParent(leverSlot, Vector3.zero);
        return state;
    }

    public InteractiveObject Lever(Action onPulled)
    {
        var state = new Lever(onPulled)
        {
            Name = "Lever",
            MessageOnInteract = "Pulled",
            InteractionDescription = "Pull lever"
        };
        var obj = Instantiate(leverPrefab);
        obj.State = state;

        return obj;
    }
}

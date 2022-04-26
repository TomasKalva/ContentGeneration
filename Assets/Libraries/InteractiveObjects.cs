using Assets.InteractiveObject;
using ContentGeneration.Assets.UI.Model;
using ShapeGrammar;
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

    [SerializeField]
    Transform gableRoof;

    [SerializeField]
    Transform pointyRoof;

    [SerializeField]
    Transform crossRoof;

    [SerializeField]
    Transform oneDirectionRoof;

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

    #region Roofs
    public Transform GableRoof(Vector3 extents)
    {
        var obj = Instantiate(gableRoof);
        obj.transform.localScale = Vector3.Scale(obj.transform.localScale, extents);

        return obj;
    }

    public Transform PointyRoof(Vector3 extents)
    {
        var obj = Instantiate(pointyRoof);
        obj.transform.localScale = Vector3.Scale(obj.transform.localScale, extents);

        return obj;
    }

    public Transform CrossRoof(Vector3 extents)
    {
        var obj = Instantiate(crossRoof);
        obj.transform.localScale = Vector3.Scale(obj.transform.localScale, extents);

        return obj;
    }

    public Transform OneDirectionRoof(Vector3 extents)
    {
        var obj = Instantiate(oneDirectionRoof);
        obj.transform.localScale = Vector3.Scale(obj.transform.localScale, extents);

        return obj;
    }
    #endregion

    public GeometryMaker<InteractiveObjectT> Geometry<InteractiveObjectT>(Transform prefab) where InteractiveObjectT : InteractiveObject
    {
        return new GeometryMaker<InteractiveObjectT>(
            () =>
            {
                var newObj = GameObject.Instantiate(prefab);
                var comp = newObj.GetComponentInChildren<InteractiveObjectT>();
                if (comp == null)
                {
                    Debug.Log($"Adding component {typeof(InteractiveObjectT)}");
                    comp = newObj.gameObject.AddComponent<InteractiveObjectT>();
                }
                return comp;
            }
        );
    }

    public InteractiveObjectState<T> NewInteractiveObject<T>(string name, GeometryMaker<T> geometryMaker) where T : InteractiveObject
    {
        var newInteractiveObject = new InteractiveObjectState<T>()
        {
            Name = name,
            GeometryMaker = geometryMaker
        };
        return newInteractiveObject;
    }
}

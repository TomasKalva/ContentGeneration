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
    public InteractiveObject ascensionKilnPrefab;

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

    [SerializeField]
    Transform physicalItemPrefab;

    public Grave Grave()
    {
        var state = new Grave()
        {
            Name = "Grave",
            MessageOnInteract = "Grave chosen",
            InteractionDescription = "Choose Grave",
            InteractOptions = new InteractOptions<InteractiveObject>()
                .AddOption("Take candle", (grave, player) => Debug.Log("Taking candle"))
                .AddOption("Put candle", (grave, player) => Debug.Log("Putting candle")),
            GeometryMaker = Geometry<InteractiveObject>(gravePrefab.transform)
        };

        return state;
    }

    public AscensionKiln AscensionKiln()
    {
        var state = new AscensionKiln()
        {
            Name = "Ascension Kiln",
            MessageOnInteract = "Will increased",
            InteractionDescription = "Increase will",
            GeometryMaker = Geometry<Kiln>(ascensionKilnPrefab.transform)
        };

        return state;
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

    public InteractiveObjectState<InteractiveObjectT> InteractiveObject<InteractiveObjectT>(string name, GeometryMaker<InteractiveObjectT> geometryMaker) where InteractiveObjectT : InteractiveObject
    {
        var newInteractiveObjectState = new InteractiveObjectState<InteractiveObjectT>()
        {
            Name = name,
            GeometryMaker = geometryMaker
        };
        return newInteractiveObjectState;
    }

    public InteractiveObjectState<InteractiveObject> Item(ItemState itemState)
    {
        var physicalItemState = new PhysicalItemState()
        {
            Name = itemState.Name,
            MessageOnInteract = $"Picked up {itemState.Name}",
            InteractionDescription = "Pick up item",
            Item = itemState,
            GeometryMaker = Geometry<InteractiveObject>(physicalItemPrefab)
        };
        return physicalItemState;
    }

    public InteractiveObjectState<InteractiveObject> Transporter()
    {
        return InteractiveObject("Transporter", Geometry<InteractiveObject>(physicalItemPrefab))
                    .Interact(
                            (transporter, pl) =>
                            {
                                transporter.Interact(
                                    (transporter, pl) => { }
                                    );

                                //var reality = GameObject.Find("Reality").GetComponent<Reality>();
                                //reality.CreateWorld();

                                var game = GameObject.Find("Game").GetComponent<Game>();
                                game.GoToNextLevel();
                            }
                        );
    }
}

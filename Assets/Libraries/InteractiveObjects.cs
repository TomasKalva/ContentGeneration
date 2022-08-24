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
    [SerializeField]
    InteractiveObject gravePrefab;

    [SerializeField]
    public InteractiveObject ascensionKilnPrefab;

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
                .AddOption("Put candle", (grave, player) => Debug.Log("Putting candle"))
                .AddOption("Rest", (grave, player) => player.Rest()),
            GeometryMaker = Geometry<InteractiveObject>(gravePrefab.transform)
        };

        return state;
    }

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

                                var game = GameObject.Find("Game").GetComponent<Game>();
                                game.GoToNextLevel();
                            }
                        );
    }
}

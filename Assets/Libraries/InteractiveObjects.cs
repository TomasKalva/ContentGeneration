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

    [SerializeField]
    Transform farmer;

    public Grave Grave()
    {
        var grave = new Grave()
        {
            Name = "Grave",
            GeometryMaker = Geometry<InteractiveObject>(gravePrefab.transform)
        };
        grave.SetInteraction(
            ins => ins
                .Act("Rest",
                    (_, player) =>
                    {
                        player.World.Grave = grave;
                        player.Rest();
                    })
            ).SetBlocking(true);

        return grave;
    }

    public InteractiveObjectState<InteractiveObject> Farmer(string name = "Farmer")
    {
        return InteractiveObject(name, Geometry<InteractiveObject>(farmer.transform)).SetBlocking(true);
    }

    public InteractiveObjectState<Kiln> Kiln(string name = "Kiln")
    {
        return InteractiveObject(name, Geometry<Kiln>(ascensionKilnPrefab.transform)).SetBlocking(true);
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
        bool beingPickedUp = false;

        var physicalItemState = new PhysicalItemState()
        {
            Name = itemState.Name,
            Item = itemState,
            GeometryMaker = Geometry<InteractiveObject>(physicalItemPrefab)
        };
        physicalItemState.SetInteraction(
            ins => ins.Act("Pick up item", 
                (io, pl) => 
                {
                    if (!beingPickedUp)
                    {
                        pl.Agent.PickUpItem(physicalItemState);
                        beingPickedUp = true;
                    }
                })
            ).SetCreatingStrategy(new CreateIfCondition(() => physicalItemState.Item != null));
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
                        )
                    .SetBlocking(true);
    }
}

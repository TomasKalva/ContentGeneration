using OurFramework.LevelDesignLanguage;
using ContentGeneration.Assets.UI;
using System;
using UnityEditor;
using UnityEngine;
using static OurFramework.LevelDesignLanguage.Game;
using OurFramework.Gameplay.RealWorld;
using OurFramework.Gameplay.Data;

namespace OurFramework.Gameplay.Libraries
{
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
        Transform gravePrefab;

        [SerializeField]
        Transform ascensionKilnPrefab;

        [SerializeField]
        Transform physicalItemPrefab;

        [SerializeField]
        Transform bloodstainPrefab;

        [SerializeField]
        Transform farmer;

        [SerializeField]
        Transform endEye;

        [SerializeField]
        Transform spikyGoblet;

        public GraveState Grave(GameControl gameControl)
        {
            var grave = new GraveState()
            {
                Name = "Grave",
                GeometryMaker = Geometry<InteractiveObject>(gravePrefab)
            };
            grave.SetInteraction(
                ins => ins
                    .Interact("Rest",
                        (_, player) =>
                        {
                            player.World.Grave = grave;
                            gameControl.ResetLevel(0.3f);
                            player.Rest();
                        })
                ).SetBlocking(true);

            return grave;
        }

        public InteractiveObjectState<InteractiveObject> Farmer(string name = "Farmer")
        {
            return InteractiveObject(name, Geometry<InteractiveObject>(farmer)).SetBlocking(true);
        }

        public InteractiveObjectState<Kiln> Kiln(string name = "Kiln")
        {
            return InteractiveObject(name, Geometry<Kiln>(ascensionKilnPrefab)).SetBlocking(true);
        }

        public InteractiveObjectState<InteractiveObject> SpikyGoblet(string name = "Spiky Gobblet")
        {
            return InteractiveObject(name, Geometry<InteractiveObject>(spikyGoblet)).SetBlocking(true);
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
                ins => ins.Interact("Pick up item",
                    (io, pl) =>
                    {
                        if (!beingPickedUp)
                        {
                            pl.Agent.PickUpItem(physicalItemState);
                            Msg.Show($"{itemState.Name} picked up");
                            beingPickedUp = true;
                        }
                    })
                ).SetCreatingStrategy(new CreateIfCondition(() => physicalItemState.Item != null));
            return physicalItemState;
        }

        public InteractiveObjectState<InteractiveObject> Bloodstain(Action onUse)
        {
            var bloodstain = new InteractiveObjectState<InteractiveObject>()
            {
                Name = "Bloodstain",
                GeometryMaker = Geometry<InteractiveObject>(bloodstainPrefab)
            };
            bloodstain.SetInteraction(
                ins => ins.Interact("Retrieve",
                    (io, pl) =>
                    {
                        onUse();
                        pl.World.RemoveItem(bloodstain);
                    })
                );
            return bloodstain;
        }

        public InteractiveObjectState<InteractiveObject> Transporter(GameControl gameControl, Action onUse = null)
        {
            return InteractiveObject("Transporter", Geometry<InteractiveObject>(endEye))
                .SetInteraction(
                    ins => ins.Interact("Proceed to the next level",
                        (transporter, pl) =>
                        {
                            transporter.Interact(
                                (transporter, pl) => { }
                                );

                            if (onUse != null) onUse();

                            gameControl.GoToNextLevel();
                        }
                    )
                )
                .SetBlocking(true);
        }
    }
}

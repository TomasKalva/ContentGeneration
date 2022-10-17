using ContentGeneration.Assets.UI;
using ContentGeneration.Assets.UI.Model;
using OurFramework.Environment.GridMembers;
using OurFramework.Environment.ShapeGrammar;
using System.Collections.Generic;
using System.Linq;

namespace OurFramework.LevelDesignLanguage.CustomModules
{
    class LockingModule : LDLanguage
    {
        public LockingModule(LanguageParams parameters) : base(parameters) { }

        /// <summary>
        /// Returns true if unlocking was successful.
        /// </summary>
        public delegate bool UnlockAction(PlayerCharacterState player);

        /// <summary>
        /// Locks the first edge leading to this area that contains door. If there is no such area, no locking will happen.
        /// </summary>
        public void LockArea(Area areaToLock, UnlockAction unlock)
        {
            var connection = State.TraversabilityGraph.EdgesTo(areaToLock).First();
            // the door face exists because of the chosen grammar
            var doorFace = connection.Path.LE.CG().ConsecutiveInsideFacesH().Where(faceH => faceH.FaceType == FACE_HOR.Door).Facets.FirstOrDefault();
            if(doorFace == null)
            {
                return;
            }

            doorFace.OnObjectCreated += tr =>
            {
                var door = tr.GetComponentInChildren<Door>();
                var doorState = (DoorState)door.State;

                bool unlocked = false;
                doorState.SetInteraction(
                    ins => ins.Act("Open/Close", 
                        (ios, player) =>
                        {
                            if (unlocked)
                            {
                                ios.IntObj.SwitchPosition();
                            }
                            else
                            {
                                unlocked = unlock(player);

                                if (unlocked)
                                {
                                    Msg.Show("Door unlocked");
                                    ios.IntObj.SwitchPosition();
                                }
                                else
                                {
                                    Msg.Show("Door is locked");
                                }
                            }
                        })
                    );
            };
        }

        public void LockedArea(NodesQuery startNodes, UnlockAction unlock, out SingleArea lockedArea)
        {
            Env.One(Gr.PrL.BlockedByDoor(), startNodes, out lockedArea);
            // the locked area has to be connected to some previous area
            LockArea(lockedArea.Get, unlock);
        }

        public IEnumerable<ItemState> CreateLockItems(string name, int count, string description, out UnlockAction unlockAction)
        {
            var items = Enumerable.Range(0, count).Select(_ =>
                Lib.Items.NewItem(name, description).SetStackable(1, false)
                );
            unlockAction = player =>
            {
                if (player.Inventory.HasItems(name, count, out var keys))
                {
                    player.Inventory.RemoveStacksOfItems(keys, count);
                    return true;
                }
                else
                {
                    return false;
                }
            };
            return items;
        }

        public void LineWithKey(NodesQuery startNodesQuery, int keyLineLength, ProductionList keyLinePr, out SingleArea locked, out LinearPath keyLine)
        {
            var branchNodes = startNodesQuery(State.GrammarState);
            Env.Line(keyLinePr, startNodesQuery, keyLineLength, out keyLine);

            var keys = CreateLockItems(State.UniqueNameGenerator.UniqueName("Key"), 1, "Used to unlock door", out var unlock);
            keyLine.LastArea().AddInteractiveObject(Lib.InteractiveObjects.Item(keys.First()));

            LockedArea(_ => branchNodes, unlock, out locked);
            locked.Get.AddInteractiveObject(Lib.InteractiveObjects.Item(Lib.Items.NewItem("Unlocked", "The door are unlocked now")));
        }

        public void RandomBranchingWithKeys(int areasCount, ProductionList keyBranchPr, out SingleArea locked, out Branching branches)
        {
            Env.BranchRandomly(keyBranchPr, areasCount, out branches);

            var keys = CreateLockItems(State.UniqueNameGenerator.UniqueName("Gemstone"), 3, "Shiny", out var unlock);
            var keyPlacer = PlO.DeadEndPlacer(keys.Select(item => Lib.InteractiveObjects.Item(item)));
            keyPlacer.Place(branches);

            LockedArea(NodesQueries.All, unlock, out locked);
            locked.Get.AddInteractiveObject(Lib.InteractiveObjects.Item(Lib.Items.NewItem("Unlocked", "The door are unlocked now")));
        }
    }
}

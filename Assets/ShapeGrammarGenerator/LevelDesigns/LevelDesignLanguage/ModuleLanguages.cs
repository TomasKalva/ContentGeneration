using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using Assets.Util;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions.SelectorLibrary;

namespace ShapeGrammar
{
    class MyLanguage : LDLanguage
    {
        public MyLanguage(LanguageParams tools) : base(tools) { }

        public void MyWorldStart()
        {
            State.LC.AddEvent(
                new LevelConstructionEvent(100, () =>
                {
                    L.LevelLanguage.LevelStart(out var startArea);
                    return false;
                }) 
            );
            
            /*State.LC.AddEvent(5, () =>
            {
                L.FarmersLanguage.FarmerBranch(0);
                return false;
            });*/
            

            /*
            State.LC.AddEvent(5, () =>
            {
                //L.PatternLanguage.BranchWithKey(NodesQueries.LastCreated, 4, Gr.PrL.TestingProductions());
                L.PatternLanguage.RandomBranchingWithKeys(6, Gr.PrL.TestingProductions(), out var locked, out var branches);
                return false;
            });
            */
            

            State.LC.AddEvent(
                new LevelConstructionEvent(0, () =>
                {
                    L.LevelLanguage.LevelEnd();
                    return false;
                })
            );

            L.FactionsLanguage.InitializeFactions(1);
            /*State.LC.AddEvent(
                new LevelConstructionEvent(5, () =>
            {
                L.TestingLanguage.LargeLevel();
                return false;
            })
            );*/
        }
    }

    class LevelLanguage : LDLanguage
    {
        public LevelLanguage(LanguageParams tools) : base(tools) { }

        public void LevelStart(out Area area)
        {
            Env.One(Gr.PrL.CreateNewHouse(), NodesQueries.All, out area);
            area.Node.AddSymbol(Gr.Sym.LevelStartMarker);
        }

        public void LevelPathSegment()
        {

        }

        public void LevelEnd()
        {
            Env.One(Gr.PrL.LevelEnd(), NodesQueries.All, out var area);
            area.AddInteractiveObject(
                Lib.InteractiveObjects.Transporter()
                );
        }


    }

    class PatternLanguage : LDLanguage
    {
        public PatternLanguage(LanguageParams tools) : base(tools) { }

        /// <summary>
        /// Returns true if unlocking was successful.
        /// </summary>
        public delegate bool UnlockAction(PlayerCharacterState player);

        public void LockedArea(NodesQuery startNodes, UnlockAction unlock, out Area lockedArea)
        {
            Env.One(Gr.PrL.BlockedByDoor(), startNodes, out lockedArea);
            // the locked area has to be connected to some previous area
            var connection = State.TraversabilityGraph.EdgesTo(lockedArea).First();
            // the door face exists because of the chosen grammar
            var doorFace = connection.Path.LE.CG().InsideFacesH().Where(faceH => faceH.FaceType == FACE_HOR.Door).Facets.First();
            doorFace.OnObjectCreated += tr =>
            {
                var door = tr.GetComponentInChildren<Door>();
                var doorState = (DoorState)door.State;

                bool unlocked = false;
                doorState.ActionOnInteract = (ios, player) =>
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
                };
            };
        }

        public IEnumerable<ItemState> CreateLockItems(string name, int count, string description, out UnlockAction unlockAction)
        {
            var items = Enumerable.Range(0, count).Select(_ =>
                Lib.Items.NewItem(name, description)
                );
            unlockAction = player =>
            {
                if (player.Inventory.HasItems(name, count, out var keys))
                {
                    player.Inventory.RemoveItems(keys);
                    return true;
                }
                else
                {
                    return false;
                }
            };
            return items;
        }

        public void BranchWithKey(NodesQuery startNodesQuery, int keyBranchLength, ProductionList keyBranchPr, out Area locked, out LinearPath keyBranch)
        {
            var branchNodes = startNodesQuery(State.GrammarState);
            Env.Line(keyBranchPr, startNodesQuery, keyBranchLength, out keyBranch);

            var keys = CreateLockItems("Key", 1, "Used to unlock door", out var unlock);
            keyBranch.LastArea().AddInteractiveObject(Lib.InteractiveObjects.Item(keys.First()));

            LockedArea(_ => branchNodes, unlock, out locked);
            locked.AddInteractiveObject(Lib.InteractiveObjects.Item(Lib.Items.NewItem("Unlocked", "The door are unlocked now")));
        }

        public void RandomBranchingWithKeys(int areasCount, ProductionList keyBranchPr, out Area locked, out Branching branches)
        {
            Env.BranchRandomly(keyBranchPr, areasCount, out branches);

            var keys = CreateLockItems("Gemstone", 3, "Shiny", out var unlock);
            var keyPlacer = PlO.DeadEndPlacer(keys.Select(item => Lib.InteractiveObjects.Item(item)));
            keyPlacer.Place(branches);

            LockedArea(NodesQueries.All, unlock, out locked);
            locked.AddInteractiveObject(Lib.InteractiveObjects.Item(Lib.Items.NewItem("Unlocked", "The door are unlocked now")));
        }
    }

    class TestingLanguage : LDLanguage
    {
        public TestingLanguage(LanguageParams tools) : base(tools) { }

        public void LargeLevel()
        {
            var grammarState = State.GrammarState;

            var shapeGrammar = new RandomGrammar(Gr.PrL.TestingProductions(), 20);
            var randGardenGrammar = new RandomGrammar(Gr.PrL.Garden(), 1);
            var graveyardGrammar = new RandomGrammar(Gr.PrL.Graveyard(), 10);
            var graveyardPostprocessGrammar = new AllGrammar(Gr.PrL.GraveyardPostprocess());
            var roofGrammar = new AllGrammar(Gr.PrL.Roofs());

            Env.Execute(shapeGrammar);
            Env.Execute(randGardenGrammar);
            Env.Execute(graveyardGrammar);
            Env.Execute(graveyardPostprocessGrammar);
            Env.Execute(roofGrammar);

            var allAreas = State.TraversableAreas;
            //var objects = Enumerable.Range(0, 100)
            //.Select(_ => Lib.InteractiveObjects.Item(Lib.Items.FreeWill()));
            //.Select(_ => Lib.InteractiveObjects.AscensionKiln());
            //.Select(_ => Lib.InteractiveObjects.InteractiveObject<InteractiveObject>("bush", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer)));
            //objects.ForEach(obj => allAreas.GetRandom().AddInteractiveObject(obj));

            State.TraversableAreas
               .ForEach(
                   area => Enumerable.Range(0, 1)
                       .ForEach(_ => area.AddEnemy(Lib.Enemies.AllAgents().GetRandom()()))
               );
        }
    }

    class BrothersLanguage : LDLanguage
    {
        public BrothersLanguage(LanguageParams tools) : base(tools) { }

        public void ThymeTea()
        {

        }

        public void GiftOfHope()
        {

        }
    }

    class FarmersLanguage : LDLanguage
    {
        public FarmersLanguage(LanguageParams tools) : base(tools) { }

        public void FarmerBranch(int progress)
        {
            Env.Line(Gr.PrL.Garden(), NodesQueries.LastCreated, 2, out var path_to_farmer);

            Env.One(Gr.PrL.Garden(), NodesQueries.LastCreated, out var farmer_area);
            farmer_area.AddInteractiveObject(
                Lib.InteractiveObjects.InteractiveObject("Farmer", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer))
                    .SetInteraction(
                        new InteractionSequence<InteractiveObject>()
                            .Say("My name is Ted")
                            .Say("I a farmer")
                            .Say("I desire nourishment")
                            .Act("Confirm your understanding", (ios, _) => ios.Interaction.TryMoveNext(ios))
                            .Decision("Would you mind sharing some apples?",
                            new InteractOption<InteractiveObject>("Give apples",
                                (farmer, player) =>
                                {
                                    if (player.Inventory.HasItems("Earthen apple", 3, out var desiredApples))
                                    {
                                        player.Inventory.RemoveItems(desiredApples);
                                        Msg.Show("Apples given");

                                        // moves farmer to another state
                                        farmer.SetInteraction(
                                            new InteractionSequence<InteractiveObject>()
                                                .Say("Thanks for the apples, mate")
                                        );

                                        player.Prop.Spirit += 10 * (1 + progress);
                                        //Levels().Next().AddPossibleBranch(FarmerBranch(progress + 1);
                                    }
                                    else
                                    {
                                        Msg.Show("Not enough apples");
                                    }
                                }
                            , 0)
                        )
                    )
                );

            Env.ExtendRandomly(Gr.PrL.Garden(), NodesQueries.LastCreated, 5, out var garden);
            var apples = Enumerable.Range(0, 5).Select(_ =>
                Lib.Items.NewItem("Earthen apple", "An apple produced by the earth itself.")
                    .OnUse(ch => ch.Prop.Spirit += 10)
                    .SetConsumable()
                )
                .Select(itemState => Lib.InteractiveObjects.Item(itemState));
            var applePlacer = PlO.EvenPlacer(apples);
            applePlacer.Place(garden);

            var enemyPlacer = PlC.RandomPlacer(
                        new UniformIntDistr(1, 1),
                        (3, Lib.Enemies.Dog),
                        (3, Lib.Enemies.Human),
                        (1, Lib.Enemies.Sculpture));
            enemyPlacer.Place(path_to_farmer.Concat(garden));
        }
    }


    class FactionsLanguage : LDLanguage
    {
        public FactionsLanguage(LanguageParams tools) : base(tools) { }

        public void InitializeFactions(int factionsCount)
        {
            var branches = Branches();

            var selectorLibrary = new SelectorLibrary(Lib);
            var effectsLibrary = new EffectLibrary(selectorLibrary);
            var factionScalingEffectLibrary = new FactionScalingEffectLibrary(effectsLibrary);

            var concepts = new FactionConcepts(
                    new List<Func<ProductionList>>()
                    {
                            Gr.PrL.Garden
                    },
                    /*new List<Func<CharacterState>>()
                    {
                        Lib.Enemies.SkinnyWoman,
                        Lib.Enemies.MayanSwordsman,

                    },*/
                    Lib.Enemies.AllAgents().Shuffle().Take(2).ToList()
                    ,
                    factionScalingEffectLibrary.EffectsByUser.Take(5).ToList(),
                    new List<Annotated<SelectorByUserByArgs>>()
                    {
                            //new Annotated<SelectorByUser>("Self", "self", selectorLibrary.SelfSelector()),
                            new Annotated<SelectorByUserByArgs>("Fire", "all those that stand in fire", selectorLibrary.GeometricSelector(Lib.VFXs.Fire, 8f, selectorLibrary.RightHandOfCharacter(0.5f) + selectorLibrary.Move(ch => ch.Agent.movement.AgentForward, 1f))),
                            new Annotated<SelectorByUserByArgs>("Cloud", "all those that stand in cloud", selectorLibrary.GeometricSelector(Lib.VFXs.MovingCloud, 4f, selectorLibrary.FrontOfCharacter(1.2f) + selectorLibrary.Move(ch => ch.Agent.movement.AgentForward, 1f))),
                            new Annotated<SelectorByUserByArgs>("Lightning", "all those that stand in lightning", selectorLibrary.GeometricSelector(Lib.VFXs.Lightning, 6f, selectorLibrary.FrontOfCharacter(0.5f) + selectorLibrary.Move(ch => ch.Agent.movement.AgentForward, 1f))),
                            new Annotated<SelectorByUserByArgs>("Fireball", "all those that are hit by fireball", selectorLibrary.GeometricSelector(Lib.VFXs.Fireball, 4f, selectorLibrary.FrontOfCharacter(0.8f) + selectorLibrary.Move(ch => ch.Agent.movement.AgentForward, 1f))),
                    },
                    new List<FlipbookTexture>()
                    {
                            Lib.VFXs.WindTexture,
                            Lib.VFXs.FireTexture,
                            Lib.VFXs.SmokeTexture,
                        //Lib.VFXs.LightningTexture,
                    }
                );

            Enumerable.Range(0, factionsCount).ForEach(_ =>
            {
                var factionConcepts = concepts.TakeSubset(3, 4, 2, 2);
                var faction = new Faction(concepts);

                State.LC.AddEvent(
                    new LevelConstructionEvent(5, () =>
                    {
                        var factionManifestation = faction.GetFactionManifestation();
                        var factionEnvironment = factionManifestation.GetFactionEnvironment();
                        branches.GetRandom()(factionEnvironment, faction.StartingBranchProgress);
                        return false;
                    })
                );
            });
        }

        public delegate void FactionEnvironmentConstructor(FactionEnvironment fe, int progress);

        public IEnumerable<FactionEnvironmentConstructor> Branches()
        {
            //yield return LockedDoorBranch;
            //yield return RandomBranches;
            yield return LinearBranch;
        }

        public void LockedDoorBranch(FactionEnvironment fe, int progress)
        {

        }

        public void RandomBranches(FactionEnvironment fe, int progress)
        {

        }



        public void LinearBranch(FactionEnvironment fe, int progress)
        {
            Env.Line(fe.ProductionList() , NodesQueries.All, 1, out var path);

            //PlO.ProgressFunctionPlacer(fe.CreateInteractiveObjectFactory(), new UniformIntDistr(1, 4)).Place(path);
            PlO.ProgressFunctionPlacer(progress => Lib.InteractiveObjects.Item(fe.CreateItemFactory()(progress)), new UniformIntDistr(1, 4)).Place(path);
            //PlC.ProgressFunctionPlacer(fe.CreateEnemyFactory(), new UniformIntDistr(1, 4)).Place(path);

            /*
            var manif = fe.FactionManifestation;
            manif.Progress++;
            State.LC.AddEvent(
                new LevelConstructionEvent(
                    10 + manif.Progress, 
                    () =>
                    {
                        LinearBranch(manif.GetFactionEnvironment(), manif.Progress);
                        return true;
                    })
                );*/
            var progressManifestation = Lib.InteractiveObjects.InteractiveObject("Progress of Manifestation", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer))
                    .SetInteraction(
                        new InteractionSequence<InteractiveObject>()
                            .Decision($"Progress this manifestation ({progress + 1})",
                            new InteractOption<InteractiveObject>("Yes",
                                (ios, player) =>
                                {
                                    fe.FactionManifestation.ContinueManifestation(State.LC, Branches());
                                    Msg.Show("Progress achieved");
                                    ios.SetInteraction(
                                        new InteractionSequence<InteractiveObject>()
                                            .Say("Thank you for your attention.")
                                    );
                                }
                            , 0)
                        )
                    );
            path.LastArea().AddInteractiveObject(progressManifestation);
        }
    }
}

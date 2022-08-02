﻿using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
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

            /*
            State.LC.AddEvent(
                new LevelConstructionEvent(0, () =>
                {
                    L.LevelLanguage.LevelEnd();
                    return false;
                })
            );

            

            State.LC.AddEvent(
                new LevelConstructionEvent(90,
                () =>
                {
                    L.AscendingLanguage.AscendingBranch(() => 100);
                    return true;
                }));

            L.FactionsLanguage.InitializeFactions(3);*/

            /*
            State.LC.AddEvent(
                new LevelConstructionEvent(10, () =>
                {
                    L.TestingLanguage.LevellingUpItems();
                    return false;
                })
            );
            */

            /*
            State.LC.AddEvent(
                new LevelConstructionEvent(5, () =>
                {
                    L.TestingLanguage.StatsScalingOfEnemies();
                    return false;
                })
            );
            

            State.LC.AddEvent(
                new LevelConstructionEvent(90,
                () =>
                {
                    L.TestingLanguage.Spells();
                    return true;
                }));
            
             */
            State.LC.AddEvent(
                new LevelConstructionEvent(90,
                () =>
                {
                    L.TestingLanguage.GrammarTesting();
                    return true;
                }));
        }
    }

    class LevelLanguage : LDLanguage
    {
        public LevelLanguage(LanguageParams tools) : base(tools) { }

        public void LevelStart(out SingleArea area)
        {
            Env.One(Gr.PrL.CreateNewHouse(), NodesQueries.All, out area);
            area.Get.Node.AddSymbol(Gr.Sym.LevelStartMarker);
        }

        public void LevelPathSegment()
        {

        }

        public void LevelEnd()
        {
            Env.One(Gr.PrL.LevelEnd(), NodesQueries.All, out var area);
            area.Get.AddInteractiveObject(
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

        public void LockedArea(NodesQuery startNodes, UnlockAction unlock, out SingleArea lockedArea)
        {
            Env.One(Gr.PrL.BlockedByDoor(), startNodes, out lockedArea);
            // the locked area has to be connected to some previous area
            var connection = State.TraversabilityGraph.EdgesTo(lockedArea.Get).First();
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

        public void BranchWithKey(NodesQuery startNodesQuery, int keyBranchLength, ProductionList keyBranchPr, out SingleArea locked, out LinearPath keyBranch)
        {
            var branchNodes = startNodesQuery(State.GrammarState);
            Env.Line(keyBranchPr, startNodesQuery, keyBranchLength, out keyBranch);

            var keys = CreateLockItems(State.UniqueNameGenerator.UniqueName("Key"), 1, "Used to unlock door", out var unlock);
            keyBranch.LastArea().AddInteractiveObject(Lib.InteractiveObjects.Item(keys.First()));

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

        public void StatsScalingOfEnemies()
        {
            Env.Line(Gr.PrL.Garden(), NodesQueries.All, Lib.Enemies.AllAgents().Count(), out var areas);

            var allEnemiesPlacer = PlC.EvenPlacer(
                Lib.Enemies.AllAgents().Select(chF =>
                {
                    var enemy = chF();
                    var stats = new CharacterStats()
                    {
                        Will = 50,
                        Strength = 5,
                        Endurance = 5,
                        Agility = 10,
                        Posture = 5,
                        Resistances = 5,
                        Versatility = 5
                    };
                    enemy.Stats = stats;

                    return enemy;
                })
            );

            allEnemiesPlacer.Place(areas);
        }

        public void LevellingUpItems()
        {
            Env.One(Gr.PrL.Garden(), NodesQueries.LastCreated, out var level_up_area);

            var statIncreaseItems = CharacterStats.StatIncreases().Select(statIncrease =>
                new ItemState()
                {
                    Name = statIncrease.Stat.ToString(),
                    Description = $"Increases {statIncrease.Stat}"
                }
                .OnUse(player => statIncrease.Manipulate(player.Stats))
            ).ToArray();

            level_up_area.Get.AddInteractiveObject(
                Lib.InteractiveObjects.InteractiveObject("Levelling up object", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer))
                    .SetInteraction(
                        new InteractionSequence<InteractiveObject>()
                            .Act("Take levelling up items", (ios, player) => statIncreaseItems.ForEach(item => player.AddItem(item))
                            )
                        )
                    );
        }

        public void Spells()
        {
            Env.One(Gr.PrL.Garden(), NodesQueries.All, out var area);

            var spells = new Spells(Lib.Effects, Lib.Selectors, Lib.VFXs);
            var spellItems = new SpellItems(spells, Lib.VFXs);
            Func<ItemState>[] s = spellItems.AllSpellItems()
                .Select<Func<ItemState>, Func<ItemState>>(itemF => () => itemF().SetReplenishable(1)).ToArray();

            area.Get.AddInteractiveObject(
                Lib.InteractiveObjects.InteractiveObject("Spells object", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer))
                    .SetInteraction(
                        new InteractionSequence<InteractiveObject>()
                            .Act("Take all spells", (ios, player) => s.ForEach(itemF => player.AddItem(itemF()))
                            )
                        )
                    );
        }

        public void GrammarTesting()
        {
            var grammarState = State.GrammarState;

            //var shapeGrammar = new RandomGrammar(Gr.PrL.TestingProductions(), 20);
            /*var randGardenGrammar = new RandomGrammar(Gr.PrL.Garden(), 1);
            var graveyardGrammar = new RandomGrammar(Gr.PrL.Graveyard(), 10);
            var graveyardPostprocessGrammar = new AllGrammar(Gr.PrL.GraveyardPostprocess());
            var roofGrammar = new AllGrammar(Gr.PrL.Roofs());
            */

            //Env.Execute(shapeGrammar);
            /*Env.Execute(randGardenGrammar);
            Env.Execute(graveyardGrammar);
            Env.Execute(graveyardPostprocessGrammar);
            Env.Execute(roofGrammar);
            */

            Env.Line(Gr.PrL.Castle(), NodesQueries.All, 40, out var _);
            /*Env.Line(Gr.PrL.TestingProductions(), NodesQueries.All, 10, out var _);
            Env.Line(Gr.PrL.TestingProductions(), NodesQueries.All, 10, out var _);
            Env.Line(Gr.PrL.TestingProductions(), NodesQueries.All, 10, out var _);
            Env.Line(Gr.PrL.TestingProductions(), NodesQueries.All, 10, out var _);*/
            Env.Execute(new AllGrammar(Gr.PrL.GraveyardPostprocess()));

            var allAreas = State.TraversableAreas;
            //var objects = Enumerable.Range(0, 100)
            //.Select(_ => Lib.InteractiveObjects.Item(Lib.Items.FreeWill()));
            //.Select(_ => Lib.InteractiveObjects.AscensionKiln());
            //.Select(_ => Lib.InteractiveObjects.InteractiveObject<InteractiveObject>("bush", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer)));
            //objects.ForEach(obj => allAreas.GetRandom().AddInteractiveObject(obj));
            
            /*
            State.TraversableAreas
               .ForEach(
                   area => Enumerable.Range(0, 1)
                       .ForEach(_ => area.AddEnemy(Lib.Enemies.AllAgents().GetRandom()()))
               );*/
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
            farmer_area.Get.AddInteractiveObject(
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
                                        player.Inventory.RemoveStacksOfItems(desiredApples, 3);
                                        Msg.Show("Apples given");

                                        // moves farmer to another state
                                        farmer.SetInteraction(
                                            new InteractionSequence<InteractiveObject>()
                                                .Say("Thanks for the apples, mate")
                                        );

                                        player.Spirit += 10 * (1 + progress);
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
                    .OnUse(ch => ch.Spirit += 10)
                    .SetStackable(1)
                )
                .Select(itemState => Lib.InteractiveObjects.Item(itemState));
            var applePlacer = PlO.EvenPlacer(apples);
            applePlacer.Place(garden);

            var enemyPlacer = PlC.RandomAreaPlacer(
                        new UniformIntDistr(1, 1),
                        (3, Lib.Enemies.Dog),
                        (3, Lib.Enemies.Human),
                        (1, Lib.Enemies.Sculpture));
            enemyPlacer.Place(path_to_farmer.Concat(garden));
        }
    }

    class AscendingLanguage : LDLanguage
    {
        public AscendingLanguage(LanguageParams tools) : base(tools) { }


        public void AscendingBranch(Func<int> startingAscensionPrice)
        {
            Env.One(Gr.PrL.Garden(), NodesQueries.All, out var ascending_area);

            var statsIncreases = CharacterStats.StatIncreases();

            int ascensionPrice = startingAscensionPrice();

            Func<StatManipulation<Action<CharacterStats>>, InteractOption<Kiln>> increaseOption = null; // Declare function before calling it recursively
            increaseOption = 
                statIncrease => new InteractOption<Kiln>($"{statIncrease.Stat}",
                    (kiln, player) =>
                    {
                        if (player.Pay(ascensionPrice))
                        {
                            statIncrease.Manipulate(player.Stats);
                            ascensionPrice += 50;
                            kiln.IntObj.BurstFire();
                            kiln.Interaction = 
                                new InteractionSequence<Kiln>()
                                .Say("Do you desire further ascending?")
                                .Decision($"What ascension are you longing for? ({ascensionPrice} Spirit)",
                                    statsIncreases.Shuffle().Take(3).Select(si => increaseOption(si)).ToArray());
                        }
                    });

            Env.One(Gr.PrL.Garden(), NodesQueries.LastCreated, out var farmer_area);
            farmer_area.Get.AddInteractiveObject(
                Lib.InteractiveObjects.InteractiveObject("Farmer", Lib.InteractiveObjects.Geometry<Kiln>(Lib.InteractiveObjects.ascensionKilnPrefab.transform))
                    .SetInteraction(
                        new InteractionSequence<Kiln>()
                            .Say("Ascension kiln is glad to feel you")
                            .Decision($"What ascension are you longing for? ({ascensionPrice} Spirit)",
                                statsIncreases.Shuffle().Take(3).Select(si => increaseOption(si)).ToArray())
                    )
                );

            // Add the same branch to the next level
            State.LC.AddEvent(
                new LevelConstructionEvent(90,
                () =>
                {
                    L.AscendingLanguage.AscendingBranch(() => ascensionPrice);
                    return true;
                }));
        }
    }

    class FactionsLanguage : LDLanguage
    {
        public FactionsLanguage(LanguageParams tools) : base(tools) { }

        public void InitializeFactions(int factionsCount)
        {
            var branches = Branches();

            var selectorLibrary = Lib.Selectors;
            var effectsLibrary = Lib.Effects;
            var factionScalingEffectLibrary = new FactionScalingEffectLibrary(effectsLibrary);

            // Make sure that item names are generated uniquely across all factions
            //var uniqueNameGenerator = new UniqueNameGenerator();


            var spells = new Spells(Lib.Effects, Lib.Selectors, Lib.VFXs);
            var spellItems = new SpellItems(spells, Lib.VFXs);
            Func<ItemState>[] s = spellItems.AllSpellItems()
                .Select<Func<ItemState>, Func<ItemState>>(itemF => () => itemF().SetReplenishable(1)).ToArray();

            var concepts = new FactionConcepts(
                    new List<Func<ProductionList>>()
                    {
                            Gr.PrL.Garden
                    },
                    Lib.Enemies.AllAgents().Shuffle().ToList(),
                    Lib.Items.AllWeapons().ToList(),
                    new List<List<Func<ItemState>>>()
                    {
                        /*new List<Func<ItemState>>()
                        {

                        },*/
                        new List<Func<ItemState>>()
                        {
                            spellItems.FireBolt,
                            spellItems.Replenishment,
                        },
                        new List<Func<ItemState>>()
                        {
                            spellItems.FlameBolt,
                            spellItems.Fireball,
                            spellItems.Cloud,
                            spellItems.PillarsOfHeaven,
                            spellItems.ConsecratedGround,
                            spellItems.Refreshment,
                        },
                        new List<Func<ItemState>>()
                        {
                            spellItems.ChaosBolt,
                            spellItems.Firefall,
                            spellItems.SquareOfChaos,
                            spellItems.FlameOfHeaven,
                            spellItems.HeavenlyFlameCloud,
                            spellItems.Triangle,
                        },
                        new List<Func<ItemState>>()
                        {
                            spellItems.CircleOfChaos,
                            spellItems.FlamesOfHeaven,
                            spellItems.Inferno,
                            spellItems.WaveOfChaos,
                        },
                    }
                );

            Enumerable.Range(0, factionsCount).ForEach(_ =>
            {
                var factionConcepts = concepts.TakeSubset(3, 4);
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

        /// <summary>
        /// Returns interactive object that allows player to continue the manifestation.
        /// </summary>
        InteractiveObjectState ProgressOfManifestation(FactionManifestation manifestation)
        {
            return Lib.InteractiveObjects.InteractiveObject("Progress of Manifestation", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer))
                    .SetInteraction(
                        new InteractionSequence<InteractiveObject>()
                            .Decision($"Progress this manifestation ({manifestation.Progress + 1})",
                            new InteractOption<InteractiveObject>("Yes",
                                (ios, player) =>
                                {
                                    manifestation.ContinueManifestation(State.LC, Branches());
                                    Msg.Show("Progress achieved");
                                    ios.SetInteraction(
                                        new InteractionSequence<InteractiveObject>()
                                            .Say("Thank you for your attention.")
                                    );
                                }
                            , 0)
                        )
                    );
        }


        public Func<InteractiveObjectState>[] ItemsToPlace(FactionEnvironment fe, int count)
        {
            return Enumerable.Range(0, 3).Select<int, Func<InteractiveObjectState>>(_ => () => Lib.InteractiveObjects.Item(fe.CreateItemFactory()(_))).ToArray();
        }

        public IEnumerable<FactionEnvironmentConstructor> Branches()
        {
            yield return LinearWithKey;
            yield return BranchesWithKey;
            yield return RandomBranches;
            yield return LinearBranch;
        }

        public void LinearWithKey(FactionEnvironment fe, int progress)
        {
            L.PatternLanguage.BranchWithKey(NodesQueries.LastCreated, 4, Gr.PrL.TestingProductions(), out var lockedArea, out var linearPath);

            var itemPlacer = PlO.RandomAreasPlacer(new UniformDistr(3, 4), ItemsToPlace(fe, 3));
            itemPlacer.Place(linearPath);
            itemPlacer.Place(lockedArea);
            PlC.ProgressFunctionPlacer(fe.CreateEnemyFactory(), new UniformIntDistr(1, 4)).Place(linearPath);

            lockedArea.Get.AddInteractiveObject(ProgressOfManifestation(fe.FactionManifestation));
        }

        public void BranchesWithKey(FactionEnvironment fe, int progress)
        {
            L.PatternLanguage.RandomBranchingWithKeys(4, Gr.PrL.TestingProductions(), out var lockedArea, out var branches);

            var itemPlacer = PlO.RandomAreasPlacer(new UniformDistr(3, 4), ItemsToPlace(fe, 3));
            itemPlacer.Place(branches);
            itemPlacer.Place(lockedArea);
            PlC.ProgressFunctionPlacer(fe.CreateEnemyFactory(), new UniformIntDistr(1, 4)).Place(branches);

            lockedArea.Get.AddInteractiveObject(ProgressOfManifestation(fe.FactionManifestation));
        }

        public void RandomBranches(FactionEnvironment fe, int progress)
        {
            Env.BranchRandomly(fe.ProductionList(), 5, out var path);

            //PlO.ProgressFunctionPlacer(fe.CreateInteractiveObjectFactory(), new UniformIntDistr(1, 4)).Place(path);
            PlO.RandomAreasPlacer(new UniformDistr(3, 6), ItemsToPlace(fe, 3)).Place(path);
            PlC.ProgressFunctionPlacer(fe.CreateEnemyFactory(), new UniformIntDistr(1, 4)).Place(path);

            path.AreasList.GetRandom().AddInteractiveObject(ProgressOfManifestation(fe.FactionManifestation));
        }

        public void LinearBranch(FactionEnvironment fe, int progress)
        {
            Env.Line(fe.ProductionList() , NodesQueries.All, 5, out var path);

            //PlO.ProgressFunctionPlacer(fe.CreateInteractiveObjectFactory(), new UniformIntDistr(1, 4)).Place(path);
            PlO.RandomAreasPlacer(new UniformDistr(3, 6), ItemsToPlace(fe, 3)).Place(path);
            PlC.ProgressFunctionPlacer(fe.CreateEnemyFactory(), new UniformIntDistr(1, 4)).Place(path);

            path.LastArea().AddInteractiveObject(ProgressOfManifestation(fe.FactionManifestation));
        }
    }
}

using Assets.ShapeGrammarGenerator;
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
                new LevelConstructionEvent(
                    $"Level Start", 
                    100, 
                    () =>
                    {
                        L.LevelLanguage.LevelStart(out var startArea);
                        return false;
                    }
                ) 
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
                new LevelConstructionEvent(
                    $"Main path", 
                    90, 
                    () =>
                    {
                        L.LevelLanguage.MainPath(0);
                        return false;
                    }
                )
            );

            /*

            State.LC.AddEvent(
                new LevelConstructionEvent(90,
                () =>
                {
                    L.AscendingLanguage.AscendingBranch(() => 100);
                    return true;
                }));
            */

            
            L.FactionsLanguage.InitializeFactions(2);
            
            State.LC.AddEvent(
                new LevelConstructionEvent(
                    $"Add Details",
                    0, 
                    () =>
                    {
                        L.DetailsLanguage.AddDetails(0);
                        return false;
                    }
                )
            );

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
            */
            /*
            State.LC.AddEvent(
                new LevelConstructionEvent(90,
                () =>
                {
                    L.TestingLanguage.Spells();
                    return true;
                }));
            
             */

            /*
            State.LC.AddEvent(
                new LevelConstructionEvent(90,
                () =>
                {
                    L.TestingLanguage.GrammarTesting();
                    return true;
                })
            );
            */

            
            State.LC.AddEvent(
                new LevelConstructionEvent(
                    $"Out of depth encounter",
                    80,
                    () =>
                    {
                        L.OutOfDepthEncountersLanguage.DifficultEncounter(0);
                        return false;
                    }
                )
            );
            
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

        public List<Func<ProductionList>> MainPathProductionLists() =>
            new List<Func<ProductionList>>()
                    {
                            () => Gr.PrL.Town(),
                            () => Gr.PrL.Castle(),
                            () => Gr.PrL.Chapels(),
                    };

        EnemyMaker BasicEnemyMaker()
        {
            return new EnemyMaker(
                level => new CharacterStats()
                {
                    Will = 0 + 1 * level,
                    Strength = 4 + 3 * level,
                    Versatility = 3 + 5 * level,
                    Endurance = 3 + 5 * level,
                    Agility = 5 + 5 * level,
                    Posture = 1 + 2 * level,
                    Resistances = 0 + level
                },
                new List<Func<WeaponItem>>()
                {
                    Lib.Items.Katana,
                    Lib.Items.MayanSword,
                    Lib.Items.Mace,
                },
                new List<Func<CharacterState>>()
                {
                    Lib.Enemies.MayanSwordsman,
                    Lib.Enemies.MayanThrower,
                    Lib.Enemies.SkinnyWoman,
                }
            );
        }

        public void MainPath(int level)
        {
            var enemyMaker = BasicEnemyMaker();

            // Place first part of the main path
            Env.Line(MainPathProductionLists().GetRandom()(), NodesQueries.All, 6, out var pathToShortcut);
            PlC.RandomAreaPlacer(new UniformDistr(1, 3), enemyMaker.GetRandomEnemy(level))
                .Place(pathToShortcut);

            // Create a shortcut
            var shortcutArea = pathToShortcut.LastArea();
            var first = pathToShortcut.AreasList.First();
            Env.MoveFromTo(pathGuide => Gr.PrL.GuidedGarden(pathGuide), Gr.PrL.ConnectBack(), 2, shortcutArea.Node.ToEnumerable(), first.Node.ToEnumerable(), out var shortcut);

            // Lock the shortcut
            var shortcutKey = L.PatternLanguage.CreateLockItems(State.UniqueNameGenerator.UniqueName("Shortcut key"), 1, "Unlocks a shortcut", out var unlock).First();
            shortcut.AreasList[0].AddInteractiveObject(Lib.InteractiveObjects.Item(shortcutKey));
            L.PatternLanguage.LockArea(shortcut.AreasList[1], unlock);

            // Create second part of the main path
            Env.Line(Gr.PrL.Town(), _ => shortcutArea.Node.ToEnumerable(), 5, out var pathToEnd);
            PlC.RandomAreaPlacer(new UniformDistr(1, 4), enemyMaker.GetRandomEnemy(level))
                .Place(pathToEnd);

            // Place transporter to the next level
            var end = pathToEnd.LastArea();
            end.AddInteractiveObject(
                Lib.InteractiveObjects.Transporter()
                );
        }

        /*
        public void LevelEnd()
        {
            Env.One(Gr.PrL.LevelEnd(), NodesQueries.All, out var area);
            area.Get.AddInteractiveObject(
                Lib.InteractiveObjects.Transporter()
                );
        }*/


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

    class EnemyMaker
    {
        public Func<int, CharacterStats> CharacterStats { get; }
        public IEnumerable<Func<WeaponItem>> Weapons { get; }
        public IEnumerable<Func<CharacterState>> Enemies { get; }

        public EnemyMaker(Func<int, CharacterStats> characterStats, IEnumerable<Func<WeaponItem>> weapons, IEnumerable<Func<CharacterState>> enemies)
        {
            CharacterStats = characterStats;
            Weapons = weapons;
            Enemies = enemies;
        }

        public Func<CharacterState> GetRandomEnemy(int level)
        {
            var weaponF = Weapons.GetRandom();
            var enemyF = Enemies.GetRandom();
            return () =>
            {
                var enemy = enemyF();
                return enemy
                    .SetStats(CharacterStats(level))
                    .SetLeftWeapon(weaponF())
                    .SetRightWeapon(weaponF())
                    .AddOnDeath(() => GameViewModel.ViewModel.PlayerState.Spirit += enemy.Health.Maximum);
            };
        }
    }
}

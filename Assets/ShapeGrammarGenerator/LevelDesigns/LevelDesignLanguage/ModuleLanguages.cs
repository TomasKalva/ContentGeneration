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

            
            State.LC.AddEvent(
                new LevelConstructionEvent(0, () =>
                {
                    L.LevelLanguage.LevelEnd();
                    return false;
                })
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
            L.FactionsLanguage.InitializeFactions(3);

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

}

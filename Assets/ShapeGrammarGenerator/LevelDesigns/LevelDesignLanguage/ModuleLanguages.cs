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
                    $"Level End", 
                    0, 
                    () =>
                    {
                        L.LevelLanguage.LevelPathSegment();
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

            /*
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
            );*/

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

            /*
            State.LC.AddEvent(
                new LevelConstructionEvent(
                    $"Out of depth encounter",
                    90,
                    () =>
                    {
                        L.OutOfDepthEncountersLanguage.DifficultEncounter(0);
                        return false;
                    }
                )
            );*/
            
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

        public void ThisShouldGuideBack(Node backToThis)
        {
            var guideRandomly = new RandomPathGuide();
            var guideToPoint = new PointPathGuide(State.GrammarState, state => new Vector3Int(0, 0, 50));

            // define path guide
            var guideBack = new PointPathGuide(State.GrammarState,
                state =>
                {
                    // todo: fix null reference exception
                    var returnToNodes = state.WithSymbols(Gr.Sym.ReturnToMarker);
                    var currentNodesCenter = state.LastCreated.Select(node => node.LE).ToLevelGroupElement(State.Ldk.grid).CG().Center();
                    var targetPoint = returnToNodes.SelectMany(n => n.LE.Cubes()).ArgMin(cube => (cube.Position - currentNodesCenter).AbsSum()).Position;
                    return Vector3Int.RoundToInt(targetPoint);
                });

            // create targeted grammar
            var targetedLowGarden = Gr.PrL.GuidedGarden(guideBack);

            //var shapeGrammar = new CustomGrammarEvaluator(productionList, 20, null, state => state.LastCreated);

            // mark the target location with a symbol
            backToThis.AddSymbol(Gr.Sym.ReturnToMarker);

            // define a grammar that moves to the target
            var targetedGardenGrammar =
                new GrammarSequence()
                    /*.SetStartHandler(
                        state => state
                            .LastCreated.SelectMany(node => node.AllDerivedFrom()).Distinct()
                            .ForEach(parent => parent.AddSymbol(Gr.Sym.ReturnToMarker))
                    )*/
                    .AppendLinear(
                        new ProductionList(Gr.Pr.ExtendBridgeToRoom(Gr.Sym.FullFloorMarker, Gr.Sym.Room, () => State.Ldk.qc.GetFlatBox(3, 3, 3), guideBack)),
                        1, NodesQueries.LastCreated
                    )
                    
                    .AppendLinear(
                        targetedLowGarden,
                        2, NodesQueries.LastCreated
                    )
                    
                    /*
                    .AppendLinear(
                        1, NodesQueries.All
                    )*/

                    .AppendStartEnd(
                        Gr.Sym,
                        new ProductionList(Gr.Pr.TowerFallDown(Gr.Sym.StartMarker, Gr.Sym.EndMarker, () => State.Ldk.qc.GetFlatBox(3, 3, 3).SetAreaType(AreaStyles.Room()))),
                        state => state.LastCreated,
                        state => state.WithSymbols(Gr.Sym.ReturnToMarker)
                    )
                    /*.SetEndHandler(
                        state => state.Root.AllDerived().ForEach(parent => parent.RemoveSymbolByName(Gr.Sym.ReturnToMarker))
                    )*/;

            // execute the grammar
            Env.Execute(targetedGardenGrammar);

            // remove the marking symbols
            backToThis.RemoveSymbolByName(Gr.Sym.ReturnToMarker);
        }

        public void LevelPathSegment()
        {

            Env.Line(Gr.PrL.Town(), NodesQueries.All, 6, out var pathToShortcut);
            var first = pathToShortcut.AreasList.First();
            var shortcutArea = pathToShortcut.LastArea();

            //ThisShouldGuideBack(first.Node);

            Env.MoveFromTo(pathGuide => Gr.PrL.GuidedGarden(pathGuide), 2, shortcutArea.Node.ToEnumerable(), first.Node.ToEnumerable(), out var shortcut);

            /*Env.Line(Gr.PrL.Town(), _ => shortcutArea.Node.ToEnumerable(), 5, out var pathToEnd);
            var end = pathToEnd.LastArea();

            end.AddInteractiveObject(
                Lib.InteractiveObjects.Transporter()
                );*/
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

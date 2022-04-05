using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{

    public class ShapeGrammarLevelDesign : LevelDesign
    {
        public ShapeGrammarLevelDesign(LevelDevelopmentKit ldk) : base(ldk)
        {
        }

        public override LevelElement CreateLevel()
        {
            var pr = new Productions(ldk);
            var grammarState = new ShapeGrammarState(ldk);

            var productionList = new List<Production>()
            {
                pr.CourtyardFromRoom(),
                pr.CourtyardFromCourtyardCorner(),
                pr.BridgeFromCourtyard(),
                pr.ExtendBridge(),
                pr.CourtyardFromBridge(),
                pr.ExtendHouse(ldk.tr.GetFloorConnector(lge => ldk.tr.SplittingFloorPlan(lge, 2))),
                pr.AddNextFloor(),
                //pr.GardenFromCourtyard(),
                //pr.RoomNextTo(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                //pr.RoomNextTo(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 4, 2, 5))),
                //pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))

                // these productions make the world untraversable
                pr.RoomFallDown(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                pr.TowerFallDown(pr.sym.Courtyard, pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),


                pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
            };
            var lowGarden = new List<Production>()
            {
                //pr.GardenFromCourtyard(),
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaType.Garden)),
                pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
            };

            var guideBack = new PointPathGuide(grammarState, state => new Vector3Int(0, 0, 50));

            var targetedLowGarden = new List<Production>()
            {
                //pr.GardenFromCourtyard(),
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), guideBack),
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaType.Garden), guideBack),
                pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
            };

            var connectBack = new List<Production>()
            {
                pr.TowerFallDown(pr.sym.StartMarker, pr.sym.EndMarker, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
            };

            var roofs = new List<Production>()
            {
                pr.AddRoof(),
            };

            var newNodes = grammarState.ApplyProduction(pr.CreateNewHouse());
            var shapeGrammar = new RandomGrammarEvaluator(productionList, 10);


            var gardenGrammar =
                new GrammarEvaluatorSequence()
                    /*.SetStartHandler(
                        state =>
                        {
                            var allParents = state.LastCreated.SelectMany(node => node.AllDerivedFrom()).Distinct();
                            allParents.ForEach(parent => parent.AddSymbol(pr.sym.ReturnToMarker));
                            //allParents.ForEach(n => Debug.Log("parent"));
                        }
                    )*/
                    .AppendLinear(
                        pr.GardenFromCourtyard().ToEnumerable().ToList(),
                        1, pr.sym.Courtyard
                    )
                    /*.AppendLinear(
                        lowGarden,
                        1, pr.sym.Courtyard,
                        state => state.LastCreated
                    )*/
                    .AppendLinear(
                        targetedLowGarden,
                        10, pr.sym.Courtyard,
                        state => state.LastCreated
                    )
                    /*.AppendStartEnd(
                        pr.sym,
                        connectBack,
                        state => state.LastCreated,
                        state => state.WithSymbols(pr.sym.ReturnToMarker)
                    )*/
                    /*.AppendLinear(
                        pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))).ToEnumerable().ToList(),
                        1, pr.sym.Courtyard,
                        state => state.LastCreated
                    )*/
                    .SetEndHandler(
                        state => state.Root.AllDerived().ForEach(parent => parent.RemoveSymbolByName(pr.sym.ReturnToMarker))
                    )
                    ;

            var roofGrammar = new AllGrammarEvaluator(roofs);

            shapeGrammar.Evaluate(grammarState);
            gardenGrammar.Evaluate(grammarState);
            roofGrammar.Evaluate(grammarState);
            grammarState.Print(new PrintingState()).Show();
            
            grammarState.Stats.Print();
            //shapeGrammar.ShapeGrammarState.VerticallyTaken.SetAreaType(AreaType.Garden).ApplyGrammarStyleRules(ldk.houseStyleRules);

            var level = grammarState.WorldState.Added;


            return level;
        }
    }
}

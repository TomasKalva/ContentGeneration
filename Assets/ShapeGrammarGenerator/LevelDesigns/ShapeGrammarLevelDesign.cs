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
                pr.TowerFallDown(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
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
            var grammarState = new ShapeGrammarState(ldk);
            var newNodes = grammarState.ApplyProduction(pr.CreateNewHouse());
            var shapeGrammar = new ShapeGrammar(productionList, 10);

            //var gardenBranch = new 
            var gardenGrammar =
                new GrammarEvaluatorSequence()
                    .AppendBranch(
                        pr.GardenFromCourtyard().ToEnumerable().ToList(),
                        1, pr.sym.Courtyard
                    )
                    .AppendBranch(
                        lowGarden,
                        10, pr.sym.Courtyard,
                        state => state.LastCreated
                    )
                    .AppendBranch(
                        pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))).ToEnumerable().ToList(),
                        1, pr.sym.Courtyard,
                        state => state.LastCreated
                    );
                /*new BranchGrammarEvaluator(
                pr.GardenFromCourtyard(), 
                lowGarden, 
                pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                state => state.Root.AllNodes(),
                10, pr.sym.Courtyard);*/
            shapeGrammar.Evaluate(grammarState);
            gardenGrammar.Evaluate(grammarState);
            grammarState.Print(new PrintingState()).Show();
            
            grammarState.Stats.Print();
            //shapeGrammar.ShapeGrammarState.VerticallyTaken.SetAreaType(AreaType.Garden).ApplyGrammarStyleRules(ldk.houseStyleRules);

            var level = grammarState.WorldState.Added;


            return level;
        }
    }
}

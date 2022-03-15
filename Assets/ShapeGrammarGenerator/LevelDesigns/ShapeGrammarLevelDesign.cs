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
                //pr.CourtyardFromCourtyardCorner(),
                /*pr.BridgeFromCourtyard(),
                pr.ExtendBridge(),
                pr.CourtyardFromBridge(),
                pr.HouseFromCourtyard(),
                pr.ExtendHouse(ldk.tr.GetFloorConnector(lge => ldk.tr.SplittingFloorPlan(lge, 2))),
                pr.AddNextFloor(),
                pr.GardenFromCourtyard(),*/
                //pr.RoomNextToCourtyard(() => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
                //pr.RoomNextToCourtyard(() => ldk.sgShapes.Tower(ldk.qc.GetFlatBox(new Box2Int(0, 0, 3, 3)).CG(), 3, 4))
            };
            var shapeGrammar = new ShapeGrammar(productionList, ldk);
            shapeGrammar.ShapeGrammarState.ApplyProduction(pr.CreateNewHouse());
            shapeGrammar.DoProductions(3);
            shapeGrammar.ShapeGrammarState.Print(new PrintingState()).Show();

            var level = shapeGrammar.ShapeGrammarState.WorldState.Added;


            return level;
        }
    }
}

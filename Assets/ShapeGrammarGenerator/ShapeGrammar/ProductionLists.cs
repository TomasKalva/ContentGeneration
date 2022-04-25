using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeGrammar
{
    public class ProductionLists
    {
        LevelDevelopmentKit ldk { get; }

        public ProductionLists(LevelDevelopmentKit ldk)
        {
            this.ldk = ldk;
        }

        public ProductionList TestingProductions(Productions pr)
        {
            return new ProductionList
            (
                pr.CourtyardFromRoom(),
                pr.CourtyardFromCourtyardCorner(),
                pr.BridgeFromCourtyard(),
                pr.ExtendBridge(),
                pr.CourtyardFromBridge(),
                //pr.ExtendHouse(ldk.tr.GetFloorConnector(lge => ldk.tr.SplittingFloorPlan(lge, 2))),
                pr.AddNextFloor(),
                //pr.GardenFromCourtyard(),
                pr.RoomNextTo(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                //pr.RoomNextTo(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 4, 2, 5))),
                //pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))

                // these productions make the world untraversable
                //pr.RoomFallDown(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                //pr.TowerFallDown(pr.sym.Courtyard, pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))

                //pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
            );
        }

        public ProductionList Garden(Productions pr)
        {
            return new ProductionList
            (
                //pr.GardenFromCourtyard(),
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaType.Garden), addFloorAbove: false),
                pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
            );
        }

        public ProductionList GuidedGarden(Productions pr, PathGuide guide)
        {
            return new ProductionList
            (
                //pr.GardenFromCourtyard(),
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), guide),
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaType.Garden), guide, addFloorAbove: false),
                pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
            );
        }

        public ProductionList Graveyard(Productions pr)
        {
            var guideRandomly = new RandomPathGuide();
            return new ProductionList 
            (
                //pr.GardenFromCourtyard(),
                //pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                //pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaType.Garden), addFloorAbove: false),
                pr.ParkNextTo(pr.sym.Courtyard, () => ldk.qc.GetFlatBox(3, 3)),
                pr.DownwardPark(pr.sym.Park, 1, () => ldk.qc.GetFlatBox(3, 3)),
                pr.DownwardPark(pr.sym.Park, 1, () => ldk.qc.GetFlatBox(5, 4)),
                pr.ChapelNextTo(pr.sym.Park, () => ldk.qc.GetFlatBox(3, 3, 2)),
                pr.ChapelHall(pr.sym.ChapelEntrance, 6, guideRandomly),
                pr.ChapelHall(pr.sym.ChapelRoom, 6, guideRandomly),
                pr.ChapelRoom(3),
                pr.ChapelNextFloor(3)
            );
        }

        public ProductionList GraveyardPostprocess(Productions pr)
        {
            var guideRandomly = new RandomPathGuide();
            return new ProductionList
            (
                pr.ChapelTowerRoof(3)
            );
        }

        public ProductionList ConnectBack(Productions pr)
        {
            return new ProductionList
            (
                pr.TowerFallDown(pr.sym.StartMarker, pr.sym.EndMarker, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
            );
        }

        public ProductionList Roofs(Productions pr)
        {
            return new ProductionList
            (
                pr.AddRoof()
            );
        }
    }
}

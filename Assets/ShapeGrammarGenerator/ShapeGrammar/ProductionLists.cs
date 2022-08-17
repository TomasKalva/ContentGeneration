using Assets.ShapeGrammarGenerator;
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
        Productions pr { get; }

        public ProductionLists(LevelDevelopmentKit ldk, Productions pr)
        {
            this.ldk = ldk;
            this.pr = pr;
        }

        public ProductionList Town(PathGuide pathGuide = null)
        {
            pathGuide ??= new RandomPathGuide();
            Func<LevelElement>[] boxFs = new Func<LevelElement>[]
            {
                () => ldk.qc.GetFlatBox(3, 5, 2),
                () => ldk.qc.GetFlatBox(6, 4, 3),
            };
            return new ProductionList
            (
                // Connection from other grammars
                pr.ExtendBridgeToRoom(pr.sym.FullFloorMarker, pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), pathGuide),

                pr.CourtyardFromRoom(pathGuide),
                pr.CourtyardFromCourtyardCorner(),
                
                pr.BridgeFrom(pr.sym.Courtyard, pathGuide),
                pr.ExtendBridge(),
                pr.CourtyardFromBridge(),
                

                pr.RoomNextFloor(pr.sym.Room, pr.sym.Room, 2, 13, ldk.con.ConnectByWallStairsOut),
                pr.RoomDown(pr.sym.Room, pr.sym.Room, 2, 3),

                pr.GardenFrom(pr.sym.Courtyard, boxFs.GetRandom()),
                pr.RoomNextTo(pr.sym.Courtyard, boxFs.GetRandom()),

                // these productions make the world untraversable
                //pr.RoomFallDown(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                //pr.TowerFallDown(pr.sym.Room, pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),

                pr.ExtendBridgeToRoom(pr.sym.Room, pr.sym.Room, boxFs.GetRandom(), pathGuide),
                pr.ExtendBridgeToGarden(pr.sym.Room, pr.sym.Garden, boxFs.GetRandom(), pathGuide),
                pr.RoomNextTo(pr.sym.Garden, boxFs.GetRandom())
            );
        }

        public ProductionList TownDetails()
        {
            var guideRandomly = new RandomPathGuide();

            return new ProductionList
            (
                pr.TerraceFrom(pr.sym.Room),
                pr.BalconyFrom(pr.sym.Room)
            );
        }

        public ProductionList CreateNewHouse()
        {
            return new ProductionList
            (
                pr.CreateNewHouse(2)
            );
        }

        public ProductionList LevelEnd()
        {
            var pathGuide = new RandomPathGuide();
            return new ProductionList
            (
                pr.ExtendBridgeToRoom(pr.sym.Room, pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), pathGuide),
                pr.ExtendBridgeToGarden(pr.sym.Garden, pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), pathGuide),
                pr.RoomNextTo(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                pr.RoomNextTo(pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
            );
        }

        public ProductionList BlockedByDoor()
        {
            var pathGuide = new RandomPathGuide();
            return new ProductionList
            (
                pr.ExtendBridgeToRoom(pr.sym.Room, pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), pathGuide),
                pr.ExtendBridgeToRoom(pr.sym.Garden, pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), pathGuide)
            );
        }

        public ProductionList Garden()
        {
            var pathGuide = new RandomPathGuide();
            return new ProductionList
            (
                //pr.GardenFromCourtyard(),
                pr.ExtendBridgeToRoom(pr.sym.Room, pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 5, 3, 4)), pathGuide),
                pr.ExtendBridgeToGarden(pr.sym.Room, pr.sym.Garden, () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaStyles.Garden()), pathGuide),
                pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 4, 3, 5)))
            );
        }

        public ProductionList GuidedGarden(PathGuide pathGuide)
        {
            return new ProductionList
            (
                //pr.GardenFromCourtyard(),
                pr.ExtendBridgeToRoom(pr.sym.Room, pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), pathGuide),
                pr.ExtendBridgeToGarden(pr.sym.Room, pr.sym.Garden, () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaStyles.Garden()), pathGuide),
                pr.ExtendBridgeToRoom(pr.sym.Garden, pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), pathGuide)
                //pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
            );
        }

        public ProductionList Chapels(PathGuide pathGuide = null)
        {
            pathGuide ??= new RandomPathGuide();

            return new ProductionList
            (
                // Connection from other grammars
                pr.ExtendBridgeTo(
                    pr.sym.FullFloorMarker,
                    pr.sym.ChapelRoom,
                    4,
                    () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)).SetAreaType(AreaStyles.Room(AreaStyles.ChapelStyle)),
                    pathGuide,
                    pr.Reserve(2, pr.sym.UpwardReservation)),
                //.ExtendBridgeToRoom(pr.sym.FullFloorMarker, pr.sym.ChapelRoom, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)).SetAreaType(AreaStyles.Room(AreaStyles.ChapelStyle)), pathGuide),

                pr.ChapelEntranceNextTo(pr.sym.Room, 3, () => ldk.qc.GetFlatBox(3, 3, 2)),
                pr.ChapelEntranceNextTo(pr.sym.Park, 3, () => ldk.qc.GetFlatBox(3, 3, 2)),

                pr.ChapelHall(pr.sym.ChapelEntrance, 4, pathGuide),

                pr.ChapelHall(pr.sym.ChapelRoom, 7, pathGuide),
                pr.ChapelHall(pr.sym.ChapelRoom, 5, pathGuide),

                pr.ChapelRoom(3),

                pr.RoomDown(pr.sym.ChapelRoom, pr.sym.ChapelRoom, 2, 3),

                pr.ChapelNextFloor(3, 16),
                pr.ChapelTowerTop(2, 3),

                pr.ParkNear(pr.sym.ChapelTowerTop, -5, 3, () => ldk.qc.GetFlatBox(5, 6, 3)),
                pr.ParkNear(pr.sym.Park, -1, 3, () => ldk.qc.GetFlatBox(5, 6, 3)),
                pr.ParkNear(pr.sym.Park, 2, 3, () => ldk.qc.GetFlatBox(5, 6, 3))
            );
        }

        public ProductionList SuperMegaTower()
        {
            var guideRandomly = new RandomPathGuide();

            return new ProductionList
            (
                //pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                //pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaType.Garden), addFloorAbove: false),

                pr.ChapelEntranceNextTo(pr.sym.Room, 3, () => ldk.qc.GetFlatBox(3, 3, 2)),

                pr.ChapelHall(pr.sym.ChapelEntrance, 4, guideRandomly),

                pr.ChapelRoom(3),
                pr.ChapelNextFloor(3, 10000)
            );
        }

        public ProductionList ChapelsDetails()
        {
            var guideRandomly = new RandomPathGuide();

            return new ProductionList
            (
                pr.ChapelSide(2),
                pr.BalconyFrom(pr.sym.ChapelRoom)
            );
        }

        public ProductionList GraveyardPostprocess()
        {
            var guideRandomly = new RandomPathGuide();
            return new ProductionList
            (
                //pr.ChapelTowerTop(3),
                pr.Roof()
            );
        }

        public ProductionList Castle(PathGuide pathGuide = null)
        {
            pathGuide ??= new RandomPathGuide();
            return new ProductionList
            (
                // Connection from other grammars
                pr.ExtendBridgeToGarden(pr.sym.FullFloorMarker, pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), pathGuide),

                pr.TowerBottomNear(pr.sym.Room, () => ldk.qc.GetFlatBox(3, 3, 5)),
                pr.TowerBottomNear(pr.sym.Garden, () => ldk.qc.GetFlatBox(3, 3, 7)),

                pr.UpwardTowerTop(2),
                pr.WallTop(pr.sym.TowerTop, 5, 2, pathGuide),
                pr.WallTop(pr.sym.TowerTop, 8, 2, pathGuide),
                pr.TowerTopFromWallTop(4, 4),
                pr.RoomDown(pr.sym.TowerTop, pr.sym.TowerBottom, 5, 3),

                //pr.GardenFrom(pr.sym.TowerBottom, () => ldk.qc.GetFlatBox(4, 4, 1)),
                pr.TowerTopNear(pr.sym.TowerTop, 4, 0, 3, () => ldk.qc.GetFlatBox(3, 3, 2)),
                pr.GardenFrom(pr.sym.TowerBottom)
            );
        }

        public ProductionList CastleDetails()
        {
            var guideRandomly = new RandomPathGuide();

            return new ProductionList
            (
                pr.SideWall(2),
                pr.WatchPostNear(pr.sym.WallTop(default), 1, -3, 4, () => ldk.qc.GetFlatBox(3, 3, 1))
            );
        }

        public ProductionList ConnectBack()
        {
            return new ProductionList
            (
                pr.TowerFallDown(pr.sym.StartMarker, pr.sym.EndMarker, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
            );
        }

        public ProductionList Roofs()
        {
            return new ProductionList
            (
                pr.Roof()
            );
        }
        /*
        public ProductionList TestinRoomFromRoom()
        {
            return new ProductionList
            (
                pr.ExtendBridgeTo(pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 10, 3, 10)))
            );
        }*/
    }
}

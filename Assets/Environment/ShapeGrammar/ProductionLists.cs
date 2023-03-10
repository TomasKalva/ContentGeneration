using OurFramework.Environment.GridMembers;
using OurFramework.Environment.ShapeCreation;
using OurFramework.Environment.StylingAreas;
using OurFramework.Util;
using System;
using System.Linq;

namespace OurFramework.Environment.ShapeGrammar
{
    /// <summary>
    /// Lists of productions.
    /// </summary>
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
                () => ldk.les.Box(3, 5, 2),
                () => ldk.les.Box(6, 4, 3),
            };
            return new ProductionList
            (
                // Connection from other grammars
                pr.ExtendBridgeToRoom(pr.sym.FullFloorMarker, pr.sym.Room, () => ldk.les.Room(4, 4, 3), pathGuide),

                pr.CourtyardFromRoom(pathGuide),
                pr.CourtyardFromCourtyardCorner(),
                
                pr.BridgeFrom(pr.sym.Courtyard, pathGuide),
                pr.ExtendBridge(),
                pr.CourtyardFromBridge(),
                

                pr.RoomNextFloor(pr.sym.Room, pr.sym.Room, AreaStyles.Room(AreaStyles.TownStyle), 2, 13, ldk.con.ConnectByWallStairsOut),
                pr.RoomDown(pr.sym.Room, pr.sym.Room, AreaStyles.Room(AreaStyles.TownStyle), 2, 3),

                pr.GardenFrom(pr.sym.Courtyard, boxFs.GetRandom()),
                pr.RoomNextTo(pr.sym.Courtyard, boxFs.GetRandom()),

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
                pr.ExtendBridgeToRoom(pr.sym.Room, pr.sym.Room, () => ldk.les.Room(3, 3, 3), pathGuide),
                pr.ExtendBridgeToGarden(pr.sym.Garden, pr.sym.Garden, () => ldk.les.Room(3, 3, 3), pathGuide),
                pr.RoomNextTo(pr.sym.Courtyard, () => ldk.les.Room(3, 3, 3)),
                pr.RoomNextTo(pr.sym.Room, () => ldk.les.Room(3, 3, 3))
            );
        }

        public ProductionList BlockedByDoor()
        {
            var pathGuide = new RandomPathGuide();
            return new ProductionList
            (
                pr.ExtendBridgeToRoom(pr.sym.FullFloorMarker, pr.sym.Room, () => ldk.les.Room(4, 4, 3), pathGuide),
                pr.ExtendBridgeToRoom(pr.sym.FullFloorMarker, pr.sym.Room, () => ldk.les.Room(4, 4, 3), pathGuide)
            );
        }

        public ProductionList Garden()
        {
            var pathGuide = new RandomPathGuide();
            return new ProductionList
            (
                //pr.GardenFromCourtyard(),
                pr.ExtendBridgeToRoom(pr.sym.Room, pr.sym.Room, () => ldk.les.Room(5, 4, 3), pathGuide),
                pr.ExtendBridgeToGarden(pr.sym.Room, pr.sym.Garden, () => ldk.cgs.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaStyles.Garden()), pathGuide),
                pr.RoomNextTo(pr.sym.Garden, () => ldk.les.Room(4, 5, 3))
            );
        }

        public ProductionList GuidedGarden(PathGuide pathGuide)
        {
            return new ProductionList
            (
                pr.ExtendBridgeToRoom(pr.sym.FullFloorMarker, pr.sym.Room, () => ldk.les.Room(4, 4, 3), pathGuide),
                pr.ExtendBridgeToGarden(pr.sym.FullFloorMarker, pr.sym.Garden, () => ldk.cgs.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaStyles.Garden()), pathGuide)
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
                    () => ldk.les.Room(5, 3, 3).SetAreaStyle(AreaStyles.Room(AreaStyles.ChapelStyle)),
                    pathGuide,
                    pr.Reserve(2, pr.sym.UpwardReservation)),
                //.ExtendBridgeToRoom(pr.sym.FullFloorMarker, pr.sym.ChapelRoom, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)).SetAreaType(AreaStyles.Room(AreaStyles.ChapelStyle)), pathGuide),

                pr.ChapelEntranceNextTo(pr.sym.Room, 3, () => ldk.les.Box(4, 4, 2)),
                pr.ChapelEntranceNextTo(pr.sym.Park, 3, () => ldk.les.Box(4, 4, 2)),

                pr.ChapelHall(pr.sym.ChapelEntrance, 5, pathGuide),

                pr.ChapelHall(pr.sym.ChapelRoom, 7, pathGuide),
                pr.ChapelHall(pr.sym.ChapelRoom, 5, pathGuide),

                pr.ChapelRoom(3),

                pr.RoomDown(pr.sym.ChapelRoom, pr.sym.ChapelRoom, AreaStyles.Room(AreaStyles.ChapelStyle), 2, 3),

                pr.ChapelNextFloor(3, 16),
                pr.ChapelTowerTop(3, 6),

                pr.ParkNear(pr.sym.ChapelTowerTop, -5, 3, () => ldk.les.Box(5, 6, 3)),
                pr.ParkNear(pr.sym.Park, -1, 3, () => ldk.les.Box(5, 6, 3)),
                pr.ParkNear(pr.sym.Park, 2, 3, () => ldk.les.Box(5, 6, 3))
            );
        }

        public ProductionList TallTowerNextToRoom()
        {
            var guideRandomly = new RandomPathGuide();

            return new ProductionList
            (
                pr.ChapelEntranceNextTo(pr.sym.Room, 3, () => ldk.les.Box(3, 3, 2)),

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

        public ProductionList ChapelsPostprocess()
        {
            var guideRandomly = new RandomPathGuide();
            return new ProductionList
            (
                pr.Roof(pr.sym.ChapelHall(default), 3, AreaStyles.GableRoof(AreaStyles.ChapelStyle)),
                pr.Roof(pr.sym.ChapelRoom, 3, AreaStyles.CrossRoof(AreaStyles.ChapelStyle))
            );
        }

        public ProductionList Castle(PathGuide pathGuide = null)
        {
            pathGuide ??= new RandomPathGuide();
            return new ProductionList
            (
                // Connection from other grammars
                pr.ExtendBridgeToGarden(pr.sym.FullFloorMarker, pr.sym.Garden, () => ldk.les.Room(new Box3Int(0, 0, 0, 4, 3, 3)), pathGuide),

                pr.TowerBottomNear(pr.sym.Room, () => ldk.les.Box(4, 4, 4)),
                pr.TowerBottomNear(pr.sym.Garden, () => ldk.les.Box(4, 4, 4)),

                pr.UpwardTowerTop(2),
                pr.WallTop(pr.sym.TowerTop, 5, 2, pathGuide),
                pr.WallTop(pr.sym.TowerTop, 8, 2, pathGuide),
                pr.TowerTopFromWallTop(4, 4),
                pr.RoomDown(pr.sym.TowerTop, pr.sym.TowerBottom, AreaStyles.Room(AreaStyles.CastleStyle), 5, 3),

                pr.TowerTopNear(pr.sym.TowerTop, 4, 0, 3, () => ldk.les.Box(4, 4, 2)),
                pr.GardenFrom(pr.sym.TowerBottom)
            );
        }

        public ProductionList CastleDetails()
        {
            var guideRandomly = new RandomPathGuide();

            return new ProductionList
            (
                pr.SideWall(2),
                pr.WatchPostNear(pr.sym.WallTop(default), 1, -3, 4, () => ldk.les.Box(3, 3, 1))
            );
        }

        public ProductionList ConnectBack()
        {
            return new ProductionList
            (
                pr.ConnectByRoom(
                    pr.sym.StartMarker, 
                    pr.sym.EndMarker, 
                    () => ldk.les.Room(4, 4, 3),
                    pr.EmptyOp(),
                    ldk.con.ConnectByStairsOutside,
                    ldk.con.ConnectByStairsOutside,
                    3,
                    (_1, _2) => true)
            );
        }

        public ProductionList OneWayConnectBack()
        {
            return new ProductionList
            (
                pr.ConnectByRoom(
                    pr.sym.StartMarker,
                    pr.sym.EndMarker,
                    () => ldk.les.Room(4, 4, 3),
                    pr.MoveVertically(1, 2),
                    ldk.con.ConnectByStairsOutside,
                    ldk.con.ConnectByFall,
                    1,
                    (_, to) => to.LE.CG().Extents().y >= 2)
            );
        }

        public ProductionList Roofs()
        {
            Production[] roofs(int height) => new Production[]
            {
                pr.Roof(pr.sym.ChapelHall(default), height, AreaStyles.GableRoof(AreaStyles.ChapelStyle)),
                pr.Roof(pr.sym.ChapelRoom, height, AreaStyles.CrossRoof(AreaStyles.ChapelStyle)),
                pr.Roof(pr.sym.TowerTop, height, AreaStyles.PointyRoof()),
                pr.Roof(pr.sym.TowerBottom, height, AreaStyles.PointyRoof()),
                pr.Roof(pr.sym.Room, height, AreaStyles.GableRoof())
            };
            return new ProductionList
            (
                roofs(3).Concat(roofs(2)).ToArray()
            );
        }

        public ProductionList WalledAround()
        {
            return new ProductionList
            (
                pr.RoomNextTo(pr.sym.FullFloorMarker, () => ldk.les.Box(4, 4, 3))
            );
        }

        public ProductionList NewStart()
        {
            return new ProductionList
            (
                pr.NewStart()
            );
        }
        
        public ProductionList NewGrammar()
        {
            return new ProductionList
            (
                pr.NewRoomNear(),
                pr.ExtrudeNewCorridor(),
                pr.ExtendNewCorridor(),
                pr.NewRoomFromNewCorridor(),
                pr.NewRoomNextFloor()
            );
        }

        public ProductionList NewRoofs()
        {
            return new ProductionList
            (
                pr.Roof(pr.sym.NewRoom, 3, AreaStyles.FlatRoof(AreaStyles.CastleStyle)),
                pr.Roof(pr.sym.NewCorridor(), 3, AreaStyles.GableRoof(AreaStyles.CastleStyle)),

                pr.Roof(pr.sym.NewRoom, 2, AreaStyles.FlatRoof(AreaStyles.CastleStyle)),
                pr.Roof(pr.sym.NewCorridor(), 2, AreaStyles.GableRoof(AreaStyles.CastleStyle))
            );
        }
    }
}

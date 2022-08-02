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

        public ProductionList TestingProductions()
        {
            return new ProductionList
            (
                pr.CourtyardFromRoom(),
                pr.CourtyardFromCourtyardCorner(),
                pr.BridgeFromCourtyard(),
                pr.ExtendBridge(),
                pr.CourtyardFromBridge(),
                pr.AddNextFloor(),
                pr.RoomNextTo(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                
                //pr.ExtendHouse(ldk.tr.GetFloorConnector(lge => ldk.tr.SplittingFloorPlan(lge, 2))), // Creates area for the node doesn't exist error
                //pr.GardenFromCourtyard(),
                pr.RoomNextTo(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 4, 2, 5))),
                pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),

                // these productions make the world untraversable
                //pr.RoomFallDown(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                //pr.TowerFallDown(pr.sym.Courtyard, pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),

                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),

                pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
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
            return new ProductionList
            (
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                pr.ExtendBridgeTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                pr.RoomNextTo(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                pr.RoomNextTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
            );
        }

        public ProductionList BlockedByDoor()
        {
            return new ProductionList
            (
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                pr.ExtendBridgeTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
            );
        }

        public ProductionList Garden()
        {
            return new ProductionList
            (
                //pr.GardenFromCourtyard(),
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 5, 3, 4))),
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaType.Garden), addFloorAbove: false),
                pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 4, 3, 5)))
            );
        }

        public ProductionList GuidedGarden(PathGuide guide)
        {
            return new ProductionList
            (
                //pr.GardenFromCourtyard(),
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), guide),
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaType.Garden), guide, addFloorAbove: false),
                pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
            );
        }

        public ProductionList Graveyard()
        {
            var guideRandomly = new RandomPathGuide();
            return new ProductionList 
            (
                //pr.GardenFromCourtyard(),
                //pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                //pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaType.Garden), addFloorAbove: false),
                pr.ParkNextTo(pr.sym.Room(), () => ldk.qc.GetFlatBox(4, 4, 3)),
                pr.Park(pr.sym.Park, -1, 3, () => ldk.qc.GetFlatBox(3, 5, 3)),
                pr.Park(pr.sym.Park, -1, 3, () => ldk.qc.GetFlatBox(5, 4, 3)),
                pr.ChapelNextTo(pr.sym.Park, () => ldk.qc.GetFlatBox(3, 3, 2)),
                pr.ChapelHall(pr.sym.ChapelEntrance, 6, guideRandomly),
                pr.ChapelHall(pr.sym.ChapelRoom(), 6, guideRandomly),
                pr.ChapelRoom(3)
                //pr.ChapelNextFloor(3, 2)
            );
        }

        public ProductionList Chappels()
        {
            var guideRandomly = new RandomPathGuide();

            Func<Symbol, int, int, Func<LevelElement>, Production> parkNear = (nearWhatSym, heighChange, minHeight, leF) =>
                pr.FullFloorPlaceNear(
                    nearWhatSym,
                    pr.sym.Park,
                    () => leF().SetAreaType(AreaType.Garden),
                    pr.MoveVertically(heighChange, minHeight),
                    pr.Empty(),
                    ldk.con.ConnectByBalconyStairsOutside,
                    1);

            Func<Symbol, int, Func<LevelElement>, Production> chapelEntranceNear = (nearWhatSym, roofHeight, leF) =>
                pr.FullFloorPlaceNear(
                    nearWhatSym,
                    pr.sym.ChapelEntrance,
                    () => leF().SetAreaType(AreaType.Room),
                    pr.Empty(),
                    pr.Roof(AreaType.CrossRoof, roofHeight),
                    ldk.con.ConnectByBalconyStairsOutside,
                    1);

            Func<int, Production> chapelNextFloor = height =>
                pr.TakeUpwardReservation(
                    pr.sym.UpwardReservation(default),
                    nextFloor => nextFloor.LE(AreaType.Room).GN(pr.sym.ChapelRoom(), pr.sym.FullFloorMarker),
                    height,
                    16,
                    pr.Reserve(2, pr.sym.UpwardReservation),
                    _ => ldk.con.ConnectByWallStairsIn);

            Func<int, Production> chapelTowerTop = height =>
                pr.TakeUpwardReservation(
                    pr.sym.UpwardReservation(default),
                    nextFloor => nextFloor.LE(AreaType.Colonnade).GN(pr.sym.ChapelTowerTop, pr.sym.FullFloorMarker),
                    2,
                    100,
                    pr.Roof(AreaType.PointyRoof, height),
                    _ => ldk.con.ConnectByWallStairsIn);

            return new ProductionList
            (
                //pr.GardenFromCourtyard(),
                //pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                //pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaType.Garden), addFloorAbove: false),
                //pr.ParkNextTo(pr.sym.Room(), () => ldk.qc.GetFlatBox(4, 4)),
                //pr.DownwardPark(pr.sym.Park, 1, () => ldk.qc.GetFlatBox(3, 5)),
                //pr.DownwardPark(pr.sym.Park, 1, () => ldk.qc.GetFlatBox(5, 4)),
                /*
                var crossRoofF = () => pr.Roof(AreaType.CrossRoof, 2);

                Symbol townRoom

                Symbol chapelEntrance
                Symbol chapelRoom
                Symbol park
                DirectedSymbol hall
                 
                pr.NextTo(townRoom, chapelEntrance, crossRoofF(), () => ldk.qc.GetFlatBox(3, 3, 2), _ => ldk.con.ConnectByDoor),
                pr.NextTo(park, chapelEntrance, crossRoofF(), () => ldk.qc.GetFlatBox(3, 3, 2), _ => ldk.con.ConnectByDoor),

                pr.NextTo(park, park, pr.Empty(), () => ldk.qc.GetFlatBox(4, 4, 3)),



                pr.ChapelHall(pr.sym.ChapelEntrance, 4, guideRandomly),
                pr.ChapelHall(pr.sym.ChapelRoom(), 7, guideRandomly),
                pr.ChapelHall(pr.sym.ChapelRoom(), 5, guideRandomly),

                pr.ChapelRoom(3),
                pr.ChapelNextFloor(3, 2),
                pr.ChapelTowerTop(3),


                
                pr.NextTo(park, pr.sym.ChapelTowerTop, pr.MoveVertically(-5, 3), pr.Empty(), () => ldk.qc.GetFlatBox(5, 6, 3), ldk.con.ConnectByStairsOutside, heightChange: -5, minHeight: 3),
                pr.NextTo(park, park, pr.Empty(), () => ldk.qc.GetFlatBox(5, 4, 3), ldk.con.ConnectByStairsOutside, heightChange: -1, minHeight: 3),
                pr.NextTo(park, park, pr.Empty(), () => ldk.qc.GetFlatBox(3, 5, 3), ldk.con.ConnectByStairsOutside, heightChange: -1, minHeight: 3),
                pr.NextTo(park, park, pr.Empty(), () => ldk.qc.GetFlatBox(4, 5, 3), ldk.con.ConnectByStairsOutside, heightChange: 2, minHeight: 3),
                 
                 */

                //pr.BridgeFrom(pr.sym.Park, guideRandomly),

                chapelEntranceNear(pr.sym.Room(), 3, () => ldk.qc.GetFlatBox(3, 3, 2)),
                chapelEntranceNear(pr.sym.Park, 3, () => ldk.qc.GetFlatBox(3, 3, 2)),

                //parkNear(pr.sym.Park, 0, 0, () => ldk.qc.GetFlatBox(4, 4, 3)),

                pr.ChapelHall(pr.sym.ChapelEntrance, 4, guideRandomly),
                /*
                pr.ChapelHall(pr.sym.ChapelRoom(), 7, guideRandomly),
                pr.ChapelHall(pr.sym.ChapelRoom(), 5, guideRandomly),

                pr.ChapelRoom(3),

                pr.RoomDown(pr.sym.ChapelRoom()),*/

                //pr.ChapelNextFloor(3, 2),
                chapelNextFloor(3),// now it takes ANY upward reservation - even from hall
                chapelTowerTop(6),
                //pr.ChapelTowerTop(3),

                parkNear(pr.sym.ChapelTowerTop, -5, 3, () => ldk.qc.GetFlatBox(5, 6, 3))
                /*parkNear(pr.sym.Park, -1, 3, () => ldk.qc.GetFlatBox(5, 4, 3)),
                parkNear(pr.sym.Park, -1, 3, () => ldk.qc.GetFlatBox(3, 5, 3)),
                parkNear(pr.sym.Park, 2, 3, () => ldk.qc.GetFlatBox(4, 5, 3))*/
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

        public ProductionList TestinRoomFromRoom()
        {
            return new ProductionList
            (
                pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 10, 3, 10)))
            );
        }
    }
}

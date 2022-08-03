﻿using System;
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
            var pathGuide = new RandomPathGuide();
            Func<LevelElement>[] boxFs = new Func<LevelElement>[]
            {
                () => ldk.qc.GetFlatBox(3, 5, 2),
                () => ldk.qc.GetFlatBox(6, 4, 3),
            };
            return new ProductionList
            (
                pr.CourtyardFromRoom(),
                pr.CourtyardFromCourtyardCorner(),
                
                pr.BridgeFrom(pr.sym.Courtyard, pathGuide),
                pr.ExtendBridge(),
                pr.CourtyardFromBridge(),
                

                pr.RoomNextFloor(pr.sym.Room, pr.sym.Room, 2, 13),
                pr.RoomDown(pr.sym.Room, pr.sym.Room, 2, 3),

                pr.GardenFrom(pr.sym.Courtyard, boxFs.GetRandom()),
                pr.RoomNextTo(pr.sym.Courtyard, boxFs.GetRandom()),

                // these productions make the world untraversable
                //pr.RoomFallDown(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                //pr.TowerFallDown(pr.sym.Courtyard, pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),

                pr.ExtendBridgeToRoom(pr.sym.Room, boxFs.GetRandom(), pathGuide),
                pr.ExtendBridgeToGarden(pr.sym.Room, boxFs.GetRandom(), pathGuide),
                pr.RoomNextTo(pr.sym.Garden, boxFs.GetRandom())
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
                pr.ExtendBridgeToRoom(pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), pathGuide),
                pr.ExtendBridgeToGarden(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), pathGuide),
                pr.RoomNextTo(pr.sym.Courtyard, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                pr.RoomNextTo(pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)))
            );
        }

        public ProductionList BlockedByDoor()
        {
            var pathGuide = new RandomPathGuide();
            return new ProductionList
            (
                pr.ExtendBridgeToRoom(pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), pathGuide),
                pr.ExtendBridgeToRoom(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), pathGuide)
            );
        }

        public ProductionList Garden()
        {
            var pathGuide = new RandomPathGuide();
            return new ProductionList
            (
                //pr.GardenFromCourtyard(),
                pr.ExtendBridgeToRoom(pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 5, 3, 4)), pathGuide),
                pr.ExtendBridgeToGarden(pr.sym.Room, () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaType.Garden), pathGuide),
                pr.RoomNextTo(pr.sym.Garden, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 4, 3, 5)))
            );
        }

        public ProductionList GuidedGarden(PathGuide pathGuide)
        {
            return new ProductionList
            (
                //pr.GardenFromCourtyard(),
                pr.ExtendBridgeToRoom(pr.sym.Room, () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3)), pathGuide),
                pr.ExtendBridgeToGarden(pr.sym.Room, () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaType.Garden), pathGuide),
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
                /*pr.ParkNextTo(pr.sym.Room(), () => ldk.qc.GetFlatBox(4, 4, 3)),
                pr.Park(pr.sym.Park, -1, 3, () => ldk.qc.GetFlatBox(3, 5, 3)),
                pr.Park(pr.sym.Park, -1, 3, () => ldk.qc.GetFlatBox(5, 4, 3)),
                pr.ChapelNextTo(pr.sym.Park, () => ldk.qc.GetFlatBox(3, 3, 2)),
                pr.ChapelHall(pr.sym.ChapelEntrance, 6, guideRandomly),
                pr.ChapelHall(pr.sym.ChapelRoom(), 6, guideRandomly),
                pr.ChapelRoom(3)
                */
                //pr.ChapelNextFloor(3, 2)
            );
        }

        public ProductionList Chapels()
        {
            var guideRandomly = new RandomPathGuide();

            return new ProductionList
            (
                //pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.Room(new Box3Int(0, 0, 0, 3, 3, 3))),
                //pr.ExtendBridgeTo(pr.sym.Room(), () => ldk.sgShapes.IslandExtrudeIter(CubeGroup.Zero(ldk.grid), 4, 0.7f).LE(AreaType.Garden), addFloorAbove: false),

                pr.ChapelEntranceNextTo(pr.sym.Room, 3, () => ldk.qc.GetFlatBox(3, 3, 2)),
                pr.ChapelEntranceNextTo(pr.sym.Park, 3, () => ldk.qc.GetFlatBox(3, 3, 2)),

                pr.ChapelHall(pr.sym.ChapelEntrance, 4, guideRandomly),

                pr.ChapelHall(pr.sym.ChapelRoom, 7, guideRandomly),
                pr.ChapelHall(pr.sym.ChapelRoom, 5, guideRandomly),

                pr.ChapelRoom(3),

                pr.RoomDown(pr.sym.ChapelRoom, pr.sym.ChapelRoom, 2, 3),

                pr.ChapelNextFloor(3, 16),
                pr.ChapelTowerTop(2, 3),

                pr.ParkNear(pr.sym.ChapelTowerTop, -5, 3, () => ldk.qc.GetFlatBox(5, 6, 3)),
                pr.ParkNear(pr.sym.Park, -1, 3, () => ldk.qc.GetFlatBox(5, 6, 3)),
                pr.ParkNear(pr.sym.Park, 2, 3, () => ldk.qc.GetFlatBox(5, 6, 3))
            );
        }

        public ProductionList ChapelsDetails()
        {
            var guideRandomly = new RandomPathGuide();

            return new ProductionList
            (
                pr.ChapelSides(2)
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

        public ProductionList Castle()
        {
            var pathGuide = new RandomPathGuide();
            return new ProductionList
            (
                pr.TowerBottomNear(pr.sym.Room, () => ldk.qc.GetFlatBox(3, 3, 5)),
                pr.TowerBottomNear(pr.sym.Garden, () => ldk.qc.GetFlatBox(3, 3, 7)),

                pr.UpwardTowerTop(2),
                pr.WallTop(pr.sym.TowerTop, 5, 2, pathGuide),
                pr.WallTop(pr.sym.TowerTop, 8, 2, pathGuide),
                pr.TowerTopFromWallTop(4, 4),
                pr.RoomDown(pr.sym.TowerTop, pr.sym.TowerBottom, 5, 3),

                //pr.GardenFrom(pr.sym.TowerBottom, () => ldk.qc.GetFlatBox(4, 4, 1)),
                pr.TowerTopNear(pr.sym.TowerTop, 4, 0, 3, () => ldk.qc.GetFlatBox(3, 3, 2)),
                pr.GardenAround(pr.sym.TowerBottom)
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

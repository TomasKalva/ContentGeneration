using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ShapeGrammar.Grid;

namespace ShapeGrammar
{

    public class ShapeGrammarShapes
    {
        Grid Grid { get; }
        QueryContext qc { get; }
        Placement pl { get; }

        public ShapeGrammarShapes(Grid grid)
        {
            Grid = grid;
            qc = new QueryContext(grid);
            pl = new Placement(grid);
        }

        public CubeGroup Room(Box3Int roomArea) => qc.GetBox(roomArea);

        public CubeGroup FlatRoof(Box2Int areaXZ, int posY) => qc.GetPlatform(areaXZ, posY);

        public LevelGroupElement SimpleHouseWithFoundation(Box2Int areaXZ, int posY)
        {
            var room = Room(areaXZ.InflateY(posY, posY + 2)).LevelElement(AreaType.Room);
            var roof = FlatRoof(areaXZ, posY + 2).LevelElement(AreaType.Roof);
            var foundation = Foundation(room).LevelElement(AreaType.Foundation);
            var house = new LevelGroupElement(Grid, AreaType.House, foundation, room, roof);
            return house;
        }

        public LevelGroupElement SimpleHouseWithFoundation(CubeGroup belowFirstFloor, int floorHeight)
        {
            var room = belowFirstFloor.ExtrudeVer(Vector3Int.up, floorHeight, false)
                .LevelElement(AreaType.Room);
                //.SplitRel(Vector3Int.up, AreaType.Room, 0.3f, 0.7f);
            var roof = room.CubeGroup().ExtrudeVer(Vector3Int.up, 1, false).LevelElement(AreaType.Roof);
            var foundation = Foundation(room).LevelElement(AreaType.Foundation);
            var house = new LevelGroupElement(Grid, AreaType.House, foundation, room, roof);
            return house;
        }

        public LevelGroupElement CompositeHouse(/*Box2Int areaXZ,*/ int height)
        {
            var flatBoxes = qc.FlatBoxes(2, 5, 4);
            var layout = qc.RemoveOverlap(pl.MoveToIntersectAll(flatBoxes));

            var boxes =  qc.RaiseRandomly(layout, () => UnityEngine.Random.Range(height / 2, height))
                    .Select(part => TurnIntoHouse(part.CubeGroup()))
                    .SetChildrenAreaType(AreaType.Room);
            var symBoxes = boxes.SymmetrizeGrp(boxes.CubeGroup().CubeGroupMaxLayer(Vector3Int.left).Cubes.GetRandom().FacesHor(Vector3Int.left));

            return boxes.Merge(symBoxes);
        }

        public LevelGroupElement Tower(CubeGroup belowFirstFloor, int floorHeight, int floorsCount)
        {
            var belowEl = belowFirstFloor.LevelElement(AreaType.Foundation); 
            var towerEl = new LevelGroupElement(belowFirstFloor.Grid, AreaType.None, belowEl);
            // create floors of the tower
            var floors = Enumerable.Range(0, floorsCount).Aggregate(towerEl,
                            (fls, _) =>
                            {
                                var newFloor = fls.CubeGroup().ExtrudeVer(Vector3Int.up, floorHeight).LevelElement(AreaType.Room);
                                return fls.Add(newFloor);
                            });
            // add roof
            var tower = floors.Add(floors.CubeGroup().ExtrudeVer(Vector3Int.up, 1).LevelElement(AreaType.Roof));

            return tower;
        }

        public LevelGroupElement AddBalcony(LevelGroupElement house)
        {
            // add outside part of each floor
            var withBalcony = house.ReplaceLeafsGrp(
                le => le.AreaType == AreaType.Room,
                le => new LevelGroupElement(le.Grid, AreaType.None, le, le.CubeGroup().CubeGroupMaxLayer(Vector3Int.down).ExtrudeHor(true, false).LevelElement(AreaType.Roof))
            );
            return withBalcony;
        }

        public LevelGroupElement TurnIntoHouse(CubeGroup house)
        {
            var roof = house.CubeGroupMaxLayer(Vector3Int.up).LevelElement(AreaType.Roof);
            var room = house.Minus(roof.CubeGroup()).LevelElement(AreaType.Room);
            return new LevelGroupElement(Grid, AreaType.House, room, roof);
        }

        public CubeGroup Foundation(LevelElement toBeFounded) => qc.MoveDownUntilGround(toBeFounded.CubeGroup().MoveBy(Vector3Int.down));

        public CubeGroup Platform(Box2Int areaXZ, int posY) => qc.GetPlatform(areaXZ, posY);

        public CubeGroup BalconyOne(CubeGroup house)
        {
            // Find a cube for the balcony
            var balcony = house.WithFloor()
               .AllBoundaryFacesH()
               .Where(face => !face.OtherCube.Changed && !face.OtherCube.In(house))
               .Facets.GetRandom()
               .OtherCube.Group();
           
            return balcony;
        }

        public CubeGroup BalconyWide(CubeGroup house)
        {
            // Find a cube for the balcony
            var balcony = house.WithFloor()
               .BoundaryFacesH(Vector3Int.left)
               .Where(face => !face.OtherCube.Changed && !face.OtherCube.In(house))
               .Extrude(1);
;
            return balcony;
        }

        public CubeGroup IslandIrregular(Box2Int boundingArea)
        {
            int cubesCount = boundingArea.Volume() / 2;
            var boundingBox = boundingArea.InflateY(0, 1);
            var cubeGroup = new CubeGroup(Grid, boundingBox.Select(coords => Grid[coords]).ToList());
            return qc.GetRandomHorConnected(boundingBox.Center(), cubeGroup, cubesCount);
        }

        public CubeGroup IslandExtrudeIter(CubeGroup cubeGroup, int iterCount, float extrudeKeptRatio)
        {
            return ExtensionMethods.ApplyNTimes(cg => qc.ExtrudeRandomly(cg, extrudeKeptRatio), cubeGroup, iterCount);
        }

        public LevelGroupElement WallAround(LevelElement inside, int height)
        {
            var insideCg = inside.CubeGroup();
            var wallTop = insideCg.ExtrudeHor(true, false).Minus(insideCg).MoveBy(height * Vector3Int.up).LevelElement(AreaType.WallTop);
            var wall = Foundation(wallTop).LevelElement(AreaType.Wall);
            return new LevelGroupElement(inside.Grid, AreaType.None, wall, wallTop);
        }


        /*
        public LevelGroupElement CompositeHouse(Box2Int areaXZ, int height)
        {
            var flatBoxes = qc.FlatBoxes(2, 5, 4);
            var layout = qc.RemoveOverlap(pl.MoveToIntersectAll(flatBoxes));

            var boxes = qc.RaiseRandomly(layout, () => UnityEngine.Random.Range(height / 2, height))
                    .Select(part => TurnIntoHouse(part.CubeGroup()))
                    .SetChildrenAreaType(AreaType.Room);

            return boxes;
        }*/

        public LevelGroupElement SubdivideRoom(LevelGeometryElement box, Vector3Int horDir, float width)
        {

            var splitBox = box.SplitRel(horDir, AreaType.None, width).SplitRel(Vector3Int.up, AreaType.Room, 0.5f);
            return splitBox
                .ReplaceLeafsGrp(0, le => le.SetAreaType(AreaType.Wall))
                .ReplaceLeafsGrp(1, le => le.SetAreaType(AreaType.OpenRoom))
                .ReplaceLeafsGrp(3, le => le.SetAreaType(AreaType.Empty));
        }

        public LevelGroupElement FloorPlan(LevelGeometryElement box, int maxRoomSize)
        {
            return qc.RecursivelySplitXZ(box, maxRoomSize).Leafs().ToLevelGroupElement(box.Grid);
        }

        public LevelGroupElement PickAndConnect(LevelGroupElement floorPlan, float keepProb)
        {
            var picked = floorPlan.LevelElements.Select(le => UnityEngine.Random.Range(0f, 1f) < keepProb ? le : le.SetAreaType(AreaType.Empty)).ToLevelGroupElement(floorPlan.Grid);
            var nonEmpty = picked.NonEmpty().CubeGroup().SplitToConnected().ToLevelGroupElement(floorPlan.Grid);
            var empty = picked.Empty().CubeGroup().SplitToConnected().ToLevelGroupElement(floorPlan.Grid).ReplaceLeafs(_ => true, le => le.SetAreaType(AreaType.Empty));
            return nonEmpty.Merge(empty);
        }
    }
}

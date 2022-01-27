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
        QueryContext QC { get; }

        public ShapeGrammarShapes(Grid grid)
        {
            Grid = grid;
            QC = new QueryContext(grid);
        }

        public CubeGroup Room(Box3Int roomArea) => QC.GetBox(roomArea);

        public CubeGroup FlatRoof(Box2Int areaXZ, int posY) => QC.GetPlatform(areaXZ, posY);

        public LevelElement House(Box2Int areaXZ, int posY)
        {
            var room = Room(areaXZ.InflateY(posY, posY + 2)).LevelElement(AreaType.Room);
            var roof = FlatRoof(areaXZ, posY + 2).LevelElement(AreaType.Roof);
            var foundation = Foundation(room).LevelElement(AreaType.Foundation);
            var house = new LevelGroupElement(Grid, AreaType.House, foundation, room, roof);
            return house;
        }

        public LevelGroupElement House(CubeGroup belowFirstFloor, int floorHeight)
        {
            var room = belowFirstFloor.ExtrudeVer(Vector3Int.up, floorHeight, false).LevelElement(AreaType.Room);
            var roof = room.CubeGroup().ExtrudeVer(Vector3Int.up, 1, false).LevelElement(AreaType.Roof);
            var foundation = Foundation(room).LevelElement(AreaType.Foundation);
            var house = new LevelGroupElement(Grid, AreaType.House, foundation, room, roof);
            return house;
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

        public LevelGroupElement WallPathH(LevelElement area1, LevelElement area2, int thickness)
        {
            // Create path between the towers
            var starting = area1.CubeGroup().WithFloor();
            var ending = area2.CubeGroup().WithFloor();
            var forbidden = area1;

            Neighbors<PathNode> neighbors = PathNode.NotIn(PathNode.HorizontalNeighbors(), forbidden.CubeGroup());
            var pathCubes = ConnectByPath(starting, ending, neighbors);
            if(pathCubes == null)
            {
                return null;
            }

            if (thickness > 1)
            {
                var halfThickness = (thickness - 1) / 2;
                // extruding to both sides
                pathCubes = pathCubes.ExtrudeHorOut(halfThickness, false).Merge(pathCubes);
                if (thickness % 2 == 0)
                {
                    // extruding to only 1 side
                    pathCubes = pathCubes.ExtrudeHorOut(thickness - 1, false).Minus(pathCubes).SplitToConnected().FirstOrDefault().Merge(pathCubes);
                }
            }

            var path = pathCubes.LevelElement(AreaType.Path);
            var foundation = Foundation(path).LevelElement(AreaType.Foundation);

            return path.Merge(foundation);
        }

        public LevelGroupElement AddBalcony(LevelGroupElement house)
        {
            // add outside part of each floor
            var withBalcony = house.ReplaceLeafsGrp(
                le => le.AreaType == AreaType.Room,
                le => new LevelGroupElement(le.Grid, AreaType.None, le, le.CubeGroup().CubesMaxLayer(Vector3Int.down).ExtrudeHor(true, false).LevelElement(AreaType.Roof))
            );
            return withBalcony;
        }

        public LevelGroupElement TurnIntoHouse(CubeGroup house)
        {
            var roof = house.CubesMaxLayer(Vector3Int.up).LevelElement(AreaType.Roof);
            var room = house.Minus(roof.CubeGroup()).LevelElement(AreaType.Room);
            return new LevelGroupElement(Grid, AreaType.House, room, roof);
        }

        public CubeGroup Foundation(LevelElement toBeFounded) => QC.MoveDownUntilGround(toBeFounded.CubeGroup().MoveBy(Vector3Int.down));

        public CubeGroup Platform(Box2Int areaXZ, int posY) => QC.GetPlatform(areaXZ, posY);

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

        public CubeGroup ConnectByPath(CubeGroup startGroup, CubeGroup endGroup, Neighbors<PathNode> neighbors)
        {
            // create graph for searching for the path
            var graph = new ImplicitGraph<PathNode>(neighbors);
            var graphAlgs = new GraphAlgorithms<PathNode, Edge<PathNode>, ImplicitGraph<PathNode>>(graph);

            var goal = new HashSet<Cube>(endGroup.Cubes);
            var starting = startGroup.Cubes.Select(c => new PathNode(null, c));

            var heuristicsV = endGroup.Cubes.GetRandom();
            var path = graphAlgs.FindPath(
                starting,
                pn => goal.Contains(pn.cube) && pn.prevVerMove == 0,
                (pn0, pn1) => (pn0.cube.Position - pn1.cube.Position).sqrMagnitude,
                pn => (pn.cube.Position - heuristicsV.Position).Sum(x => Mathf.Abs(x)),
                PathNode.Comparer);

            var pathCubes = path == null ? starting.GetRandom().cube.Group().Cubes : path.Select(pn => pn.cube).ToList();
            // drop first and last element
            pathCubes = pathCubes.Skip(1).Reverse().Skip(1).Reverse().ToList();
            return new CubeGroup(Grid, pathCubes);
        }

        public CubeGroup IslandIrregular(Box2Int boundingArea)
        {
            int cubesCount = boundingArea.Volume() / 2;
            var boundingBox = boundingArea.InflateY(0, 1);
            var cubeGroup = new CubeGroup(Grid, boundingBox.Select(coords => Grid[coords]).ToList());
            return QC.GetRandomHorConnected(boundingBox.Center(), cubeGroup, cubesCount);
        }

        public CubeGroup IslandExtrudeIter(CubeGroup cubeGroup, int iterCount, float extrudeKeptRatio)
        {
            return ExtensionMethods.ApplyNTimes(cg => QC.ExtrudeRandomly(cg, extrudeKeptRatio), cubeGroup, iterCount);
        }

        public LevelGroupElement WallAround(LevelElement inside, int height)
        {
            var insideCg = inside.CubeGroup();
            var wallTop = insideCg.ExtrudeHor(true, false).Minus(insideCg).MoveBy(height * Vector3Int.up).LevelElement(AreaType.WallTop);
            var wall = Foundation(wallTop).LevelElement(AreaType.Wall);
            return new LevelGroupElement(inside.Grid, AreaType.None, wall, wallTop);
        }
    }
}

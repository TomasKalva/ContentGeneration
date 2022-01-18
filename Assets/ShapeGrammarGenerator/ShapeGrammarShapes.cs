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

        public CubeGroup Room(Box3Int roomArea) => QC.GetBox(roomArea).SetAreaType(AreaType.Room);

        public CubeGroup FlatRoof(Box2Int areaXZ, int posY) => QC.GetPlatform(areaXZ, posY).SetAreaType(AreaType.Roof);

        public CubeGroupGroup House(Box2Int areaXZ, int posY)
        {
            var room = Room(areaXZ.InflateY(posY, posY + 2));
            var roof = FlatRoof(areaXZ, posY + 2);
            var cubesBelowRoom = room.BoundaryFacesV(Vector3Int.down).Cubes().MoveBy(Vector3Int.down);
            var foundation = Foundation(cubesBelowRoom);
            var house = new CubeGroupGroup(Grid, AreaType.House, foundation, room, roof);
            return house;
        }

        public CubeGroupGroup House(CubeGroup belowFirstFloor, int floorHeight)
        {
            var room = belowFirstFloor.ExtrudeVer(Vector3Int.up, floorHeight).SetAreaType(AreaType.Room);
            var roof = room.ExtrudeVer(Vector3Int.up, 1).SetAreaType(AreaType.Roof);
            var foundation = Foundation(belowFirstFloor);
            var house = new CubeGroupGroup(Grid, AreaType.House, foundation, room, roof);
            return house;
        }

        public CubeGroup Foundation(CubeGroup foundationArea) => QC.MoveDownUntilGround(foundationArea).SetAreaType(AreaType.Foundation);

        public CubeGroup Platform(Box2Int areaXZ, int posY) => QC.GetPlatform(areaXZ, posY);

        public CubeGroup BalconyOne(CubeGroup house)
        {
            // Find a cube for the balcony
            var balcony = house.WithFloor()
               .AllBoundaryFacesH()
               .Where(face => !face.OtherCube.Changed && !face.OtherCube.In(house))
               .Facets.GetRandom()
               .OtherCube.Group(AreaType.Balcony);
           
            return balcony;
        }

        public CubeGroup BalconyWide(CubeGroup house)
        {
            // Find a cube for the balcony
            var balcony = house.WithFloor()
               .BoundaryFacesH(Vector3Int.left)
               .Where(face => !face.OtherCube.Changed && !face.OtherCube.In(house))
               .Extrude(1)
               .SetAreaType(AreaType.Balcony);
;
            return balcony;
        }

        public CubeGroup ConnectByPath(CubeGroup cubeGroup1, CubeGroup cubeGroup2, CubeGroup pathBoundingBox)
        {
            // create graph for searching for the path
            var graph = new ImplicitGraph<Cube>(cube => cube.NeighborsHor().Intersect(pathBoundingBox.Cubes));
            var graphAlgs = new GraphAlgorithms<Cube, Edge<Cube>, ImplicitGraph<Cube>>(graph);

            //var path = graphAlgs.FindPath(cubeGroup1.Cubes.GetRandom(), cubeGroup2.Cubes);
            var test = graphAlgs.EdgeAStar(cubeGroup1.Cubes.GetRandom(), (c0, c1) => (c0.Position - c1.Position).sqrMagnitude, _ => 0).Select(e => e.To);
            return new CubeGroup(Grid, AreaType.Path, test.ToList());
        }

        public CubeGroup IslandIrregular(Box2Int boundingArea)
        {
            int cubesCount = boundingArea.Volume() / 2;
            var boundingBox = boundingArea.InflateY(0, 1);
            var cubeGroup = new CubeGroup(Grid, AreaType.None, boundingBox.Select(coords => Grid[coords]).ToList());
            return QC.GetRandomHorConnected(boundingBox.Center(), cubeGroup, cubesCount);
        }

        public CubeGroup IslandExtrudeIter(CubeGroup cubeGroup, int iterCount, float extrudeKeptRatio)
        {
            return ExtensionMethods.ApplyNTimes(cg => QC.ExtrudeRandomly(cg, extrudeKeptRatio), cubeGroup, iterCount);
        }
    }
}

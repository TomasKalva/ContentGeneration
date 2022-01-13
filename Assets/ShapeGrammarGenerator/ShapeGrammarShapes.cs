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

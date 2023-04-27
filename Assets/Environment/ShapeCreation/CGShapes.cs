using OurFramework.Environment.StylingAreas;
using OurFramework.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OurFramework.Environment.GridMembers
{
    /// <summary>
    /// Used for creating cube group shapes.
    /// </summary>
    public class CGShapes
    {
        Grid<Cube> QueriedGrid { get; }

        public CGShapes(Grid<Cube> queriedGrid)
        {
            QueriedGrid = queriedGrid;
        }

        public CubeGroup Box(Box3Int box)
        {
            var cubes = box.Select(i => QueriedGrid[i]).ToList();
            return new CubeGroup(QueriedGrid, cubes);
        }

        public CubeGroup FlatBox(Box2Int box, int y = 0)
        {
            return Box(box.InflateY(0, 1) + y * Vector3Int.up);
        }

        public CubeGroup MoveDownUntilGround(CubeGroup topLayer)
        {
            var foundationCubes = topLayer.MoveInDirUntil(Vector3Int.down, cube => cube.Position.y < 0);
            return foundationCubes;
        }

        public CubeGroup GetRandomHorConnected(CubeGroup start, CubeGroup boundingGroup)
        {
            var possibleCubes = start.AllBoundaryFacesH().Extrude(1).Cubes.Intersect(boundingGroup.Cubes);
            if (!possibleCubes.Any())
                return start;
            var newCube = possibleCubes.GetRandom();
            return new CubeGroup(QueriedGrid, start.Cubes.Append(newCube).ToList());
        }

        public CubeGroup ExtrudeRandomly(CubeGroup start, float keptRatio)
        {
            var possibleCubes = start.AllBoundaryFacesH().Extrude(1).Cubes;
            if (!possibleCubes.Any())
                return start;
            var newCubes = possibleCubes.Shuffle().Take((int)(keptRatio * possibleCubes.Count()));
            return new CubeGroup(QueriedGrid, start.Cubes.Concat(newCubes).ToList());
        }

        public CubeGroup IslandExtrudeIter(CubeGroup cubeGroup, int iterCount, float extrudeKeptRatio)
        {
            return ExtensionMethods.ApplyNTimes(cg => ExtrudeRandomly(cg, extrudeKeptRatio), cubeGroup, iterCount);
        }
    }
}

﻿using Assets.ShapeGrammarGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class Paths
    {
        /*
        public LevelGroupElement WalkableWallPathH(LevelElement area1, LevelElement area2, int thickness)
        {
            var starting = area1.CG().BottomLayer();
            var ending = area2.CG().BottomLayer();

            Neighbors<PathNode> neighbors = PathNode.HorizontalNeighbors();
            var pathCubes = ConnectByPathFS(starting, ending, neighbors);
            
            if (pathCubes == null)
            {
                return null;
            }

            pathCubes = Thicken(pathCubes, thickness, area1.Merge(area2).CG());

            var path = pathCubes.LE(AreaStyles.Path());
            var foundation = sgShapes.Foundation(path);

            return path.Merge(foundation);
        }

        public LevelGeometryElement PathH(LevelElement area1, LevelElement area2, int thickness, LevelElement notIntersecting)
        {
            var starting = area1.CG();
            var ending = area2.CG();

            Neighbors<PathNode> neighbors = PathNode.NotIn(PathNode.HorizontalNeighbors(), notIntersecting.CG());
            var pathCubes = ConnectByPathFS(starting, ending, neighbors);

            if (pathCubes == null)
            {
                return null;
            }

            var surrounding = notIntersecting.Merge(area2).CG();
            pathCubes = Thicken(pathCubes, thickness, surrounding).Minus(surrounding);

            return pathCubes.LE(AreaStyles.Path());
        }
        */
        /*
        public LevelGeometryElement WalkableElevator(LevelElement area1, LevelElement area2)
        {
            var starting = area1.CG().BottomLayer();
            var ending = area2.CG().BottomLayer();

            Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.ElevatorNeighbors(area2.CG()), area1.Merge(area2).CG());
            var pathCubes = ConnectByPath(starting, ending, neighbors);

            if (pathCubes == null)
            {
                return null;
            }

            return pathCubes.LE(AreaStyle.Elevator);
        }*/

        /// <summary>
        /// Path doesn't go through the starting group.
        /// </summary>
        /*
        public CubeGroup ConnectByPathFS(CubeGroup starting, CubeGroup ending, Neighbors<PathNode> neighbors)
        {
            neighbors = PathNode.NotIn(neighbors, starting);
            return ConnectByPath(starting, ending, neighbors);
        }
        */

        public CubeGroup Thicken(CubeGroup pathCubes, int thickness, CubeGroup notIntersecting)
        {
            if (thickness > 1)
            {
                var halfThickness = (thickness - 1) / 2;
                // extruding to both sides
                pathCubes = pathCubes.ExtrudeHorOut(halfThickness, false).Merge(pathCubes).Minus(notIntersecting);

                if (thickness % 2 == 0)
                {
                    // extruding to only 1 side
                    pathCubes = pathCubes.ExtrudeHorOut((thickness - 1) % 2, false).Minus(pathCubes.Merge(notIntersecting)).SplitToConnected().FirstOrDefault().Merge(pathCubes);
                }
            }
            return pathCubes;
        }

        public CubeGroup ConnectByPath(CubeGroup startGroup, CubeGroup endGroup, Neighbors<PathNode> neighbors)
        {
            Debug.Assert(startGroup.Cubes.Count > 0);
            Debug.Assert(endGroup.Cubes.Count > 0);

            // create graph for searching for the path
            var graph = new ImplicitGraph<PathNode>(neighbors);
            var graphAlgs = new GraphAlgorithms<PathNode, Edge<PathNode>, ImplicitGraph<PathNode>>(graph);

            var goal = new HashSet<Cube>(endGroup.Cubes);
            var starting = startGroup.Cubes.Select(c => new PathNode(null, c));

            var heuristicsV = endGroup.Cubes.GetRandom();
            var path = graphAlgs.FindPath(
                starting,
                // the path always ends with horizontal move
                pn => goal.Contains(pn.cube) && pn.prevVerMove == 0, 
                (pn0, pn1) => (pn0.cube.Position - pn1.cube.Position).sqrMagnitude,
                pn => (pn.cube.Position - heuristicsV.Position).Sum(x => Mathf.Abs(x)),
                PathNode.Comparer,
                // cap the number of iterations
                10_000);

            var pathCubes = path == null ? starting.GetRandom().cube.Group().Cubes : path.Select(pn => pn.cube).ToList();
            // drop first and last element
            return new CubeGroup(startGroup.Grid, pathCubes);
        }

        public CubeGroup ConnectByConnectivityPreservingPath(CubeGroup startGroup, CubeGroup endGroup, CubeGroup startAreaFloor, CubeGroup endAreaFloor, Neighbors<PathNode> neighbors, LevelGroupElement existingPaths)
        {
            var existingPathsCgs = existingPaths.LevelElements.Select(le => le.CG());
            return ConnectByPath(
                // Preserve connectivity for the starting nodes
                PathNode.PreserveConnectivity(
                    _ => startGroup.Cubes.Select(startCube => new PathNode(null, startCube)), startAreaFloor, endAreaFloor, existingPathsCgs)(null)
                    .Select(pathNode => pathNode.cube).ToCubeGroup(startGroup.Grid),
                endGroup,
                // Preserve connectivity for all the other nodes
                PathNode.PreserveConnectivity(neighbors, startAreaFloor, endAreaFloor, existingPathsCgs));
        }
    }
}

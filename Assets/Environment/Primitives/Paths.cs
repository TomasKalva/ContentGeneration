using OurFramework.Environment.GridMembers;
using OurFramework.Util;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace OurFramework.Environment.ShapeCreation
{
    /// <summary>
    /// Used for connecting by paths.
    /// </summary>
    public class Paths
    {
        /// <summary>
        /// Returns condition for ending pathfinding.
        /// </summary>
        StopCondition StopAfterIterationsOrTime(int maxIterations, float maxTimeS)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            return iterationsCount =>
                    iterationsCount >= maxIterations ||
                    stopwatch.ElapsedMilliseconds / 1000f > maxTimeS;
        }

        /// <summary>
        /// Connects the two groups by a path constructed from neighbors function.
        /// </summary>
        public CubeGroup ConnectByPath(CubeGroup startGroup, CubeGroup endGroup, Neighbors<PathNode> neighbors)
        {
            startGroup.AssertNonEmpty();
            endGroup.AssertNonEmpty();

            // create graph for searching for the path
            var graph = new ImplicitGraph<PathNode>(neighbors);
            var graphAlgs = new GraphAlgorithms<PathNode, Edge<PathNode>, ImplicitGraph<PathNode>>(graph);

            var goal = new HashSet<Cube>(endGroup.Cubes);
            var starting = startGroup.Cubes.Select(c => new PathNode(null, c));

            var heuristicsV = endGroup.Cubes.GetRandom();

            var stopCondition = StopAfterIterationsOrTime(10_000, 1f);
            var path = graphAlgs.FindPath(
                starting,
                // the path always ends with horizontal move
                pn => goal.Contains(pn.cube) && pn.prevVerMove == 0, 
                (pn0, pn1) => (pn0.cube.Position - pn1.cube.Position).sqrMagnitude,
                pn => (pn.cube.Position - heuristicsV.Position).Sum(x => Mathf.Abs(x)),
                PathNode.Comparer,
                // cap the number of iterations
                stopCondition);

            var pathCubes = path.Select(pn => pn.cube).ToList();
            return new CubeGroup(startGroup.Grid, pathCubes);
        }

        /// <summary>
        /// Connects the two groups by a path constructed from neighbors function. It preserves connectedness of other areas.
        /// </summary>
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

        /// <summary>
        /// Connects the two groups by a path constructed from a valid neighbors function. The path doesn't go below y=2.
        /// </summary>
        public CubeGroup ConnectByValidPath(CubeGroup startGroup, CubeGroup endGroup, CubeGroup startAreaFloor, CubeGroup endAreaFloor, Neighbors<PathNode> neighbors, LevelGroupElement existingPaths)
        {
            return ConnectByConnectivityPreservingPath(
                startGroup,
                endGroup,
                startAreaFloor,
                endAreaFloor,
                pn => neighbors(pn).Where(nb => nb.cube.Position.y >= 2),
                existingPaths);
        }
    }
}

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ShapeGrammar
{
    public class Paths
    {
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

        StopCondition StopAfterIterationsOrTime(int maxIterations, float maxTimeS)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            return iterationsCount =>
                    iterationsCount >= maxIterations ||
                    stopwatch.ElapsedMilliseconds / 1000f > maxTimeS;
        }

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

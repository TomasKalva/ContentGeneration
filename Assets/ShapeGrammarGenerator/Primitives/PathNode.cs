using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class PathNode : IEqualityComparer<PathNode>
    {
        public static IEqualityComparer<PathNode> Comparer { get; } = new PathNode(null, null);

        #region Neighborhoods
        public static Neighbors<PathNode> HorizontalNeighbors()
        {
            return pathNode => pathNode.cube.NeighborsHor().Select(c => new PathNode(pathNode, c));
        }

        public static Neighbors<PathNode> VerticalNeighbors()
        {
            return pathNode => pathNode.cube.NeighborsVer().Select(c => new PathNode(pathNode, c));
        }

        public static Neighbors<PathNode> StraightHorizontalNeighbors()
        {
            return pathNode => pathNode.prev == null ?
                pathNode.cube.NeighborsHor().Select(c => new PathNode(pathNode, c)) :
                new PathNode(pathNode, pathNode.cube.Grid[2 * pathNode.cube.Position - pathNode.prev.cube.Position]).ToEnumerable();
        }

        public static Neighbors<PathNode> DirectionNeighbors(params Vector3Int[] directions)
        {
            return pathNode => pathNode.cube.NeighborsDirections(directions).Select(c => new PathNode(pathNode, c));
        }

        public static Neighbors<PathNode> NotRepeatingCubes(Neighbors<PathNode> neighbors)
        {
            return pathNode =>
            {
                var path = new HashSet<Cube>(pathNode.ReversePath());
                return neighbors(pathNode).Where(neighbor => !path.Contains(neighbor.cube));
            };
        }

        public static Neighbors<PathNode> BoundedBy(Neighbors<PathNode> neighbors, CubeGroup pathBounds)
        {
            var boundingSet = new HashSet<Cube>(pathBounds.Cubes);
            return pathNode =>
            {
                return neighbors(pathNode).Where(neighbor => boundingSet.Contains(neighbor.cube));
            };
        }

        public static Neighbors<PathNode> NotIn(Neighbors<PathNode> neighbors, CubeGroup forbiddenCubes)
        {
            var forbiddenSet = new HashSet<Cube>(forbiddenCubes.Cubes);
            return pathNode =>
            {
                return neighbors(pathNode).Where(neighbor => !forbiddenSet.Contains(neighbor.cube));
            };
        }

        public static Neighbors<PathNode> NotAbove(Neighbors<PathNode> neighbors, CubeGroup forbiddenCubes)
        {
            var forbiddenSet = new Dictionary<Vector2Int, int>();
            forbiddenCubes.Cubes.ForEach(cube =>
            {
                var yExists = forbiddenSet.TryGetValue(cube.Position.XZ(), out var max);
                var newY = cube.Position.y;
                forbiddenSet.TryAdd(cube.Position.XZ(), yExists ? Mathf.Max(newY, max) : newY);
            });
            return pathNode =>
            {
                return neighbors(pathNode).Where(neighbor =>
                {
                    var pos = neighbor.cube.Position;
                    var yExists = forbiddenSet.TryGetValue(pos.XZ(), out var y);
                    return !yExists || pos.y < y;
                });
            };
        }

        #region Preserving area connectivity
        
        /// <summary>
        /// Assumes that 
        ///     - each path has ordered cubes from start to end
        ///     - each path has at least 2 cubes
        /// </summary>
        public static IEnumerable<Cube> FindPathEndsInFloor(CubeGroup floor, IEnumerable<CubeGroup> paths) 
        {
            var pathEnds =
                paths.Select(path => path.Cubes.First()).Concat( // todo: sequence contains no elements error happens in here
                paths.Select(path => path.Cubes.Last())
                );
            //return pathEnds;
            return pathEnds.SetIntersect(floor.Cubes).ToArray();
        }

        /// <summary>
        /// All path ends must belong to the area of floor group.
        /// </summary>
        public static bool IsConnected(CubeGroup unoccupiedFloor, IEnumerable<Cube> pathEnds)
        {
            return
                // floor doesn't get disconnected
                unoccupiedFloor.SplitToConnected().Count() <= 1 &&
                // all path ends in the floor are still connected to the new floor
                pathEnds.All(pathEnd => pathEnd.NeighborsHor().SetIntersect(unoccupiedFloor.Cubes).Any());
        }

        /// <summary>
        /// All path ends must belong to the area of floor group.
        /// </summary>
        public static bool StaysConnected(CubeGroup unoccupiedFloor, IEnumerable<Cube> pathEnds, PathNode newNode)
        {
            var newFloor = unoccupiedFloor.Cubes.Except(newNode.ReversePath()).ToCubeGroup(unoccupiedFloor.Grid);
            var endOfNewPath = newNode.ReversePath().Last();
            var newPathEnds = unoccupiedFloor.Cubes.Contains(endOfNewPath) ? pathEnds.Append(endOfNewPath) : pathEnds;
            return IsConnected(newFloor, newPathEnds);
        }

        public static Neighbors<PathNode> PreserveConnectivity(Neighbors<PathNode> neighbors, CubeGroup cg1, CubeGroup cg2, IEnumerable<CubeGroup> otherPaths)
        {
            var allPathsGroup = otherPaths.SelectMany(cg => cg.Cubes).ToCubeGroup(cg1.Grid);

            var floor1 = cg1.BottomLayer();
            var floor2 = cg2.BottomLayer();

            var unoccupiedFloor1 = floor1.Minus(allPathsGroup);
            var unoccupiedFloor2 = floor2.Minus(allPathsGroup);

            var floorSet1 = new HashSet<Cube>(floor1.Cubes);
            var floorSet2 = new HashSet<Cube>(floor2.Cubes);

            var pathEndsInFloor1 = FindPathEndsInFloor(floor1, otherPaths);
            var pathEndsInFloor2 = FindPathEndsInFloor(floor2, otherPaths);

            return NotIn(
                pathNode =>
                    neighbors(pathNode)
                    .Where(pn =>
                        floorSet1.Contains(pn.cube) ?
                            StaysConnected(unoccupiedFloor1, pathEndsInFloor1, pn) :
                            floorSet2.Contains(pn.cube) ?
                                StaysConnected(unoccupiedFloor2, pathEndsInFloor2, pn) :
                                true
                    ),
                allPathsGroup
                );
        }

        #endregion

        public static Neighbors<PathNode> StairsNeighbors()
        {
            var horizontal = HorizontalNeighbors();
            var vertical = VerticalNeighbors();
            Neighbors<PathNode> repeatingCubesNeighbors = pathNode =>
            {
                // return only horizontal directions for 1st node
                return pathNode.prev != null ? 
                            pathNode.prevVerMove == 0 ? 
                                horizontal(pathNode).Concat(vertical(pathNode)) : 
                                DirectionNeighbors(pathNode.lastHorMove.X0Z())(pathNode) :
                            horizontal(pathNode);
            };
            return NotRepeatingCubes(repeatingCubesNeighbors);
        }

        public static Neighbors<PathNode> FallNeighbors(CubeGroup end)
        {
            var horizontal = HorizontalNeighbors();
            var vertical = VerticalNeighbors();
            var endSet = new HashSet<Cube>(end.Cubes);
            Neighbors<PathNode> repeatingCubesNeighbors = pathNode =>
            {
                return pathNode.prev == null ?
                            horizontal(pathNode) : // horizontal node at start
                            vertical(pathNode).Concat(horizontal(pathNode).Where(node => endSet.Contains(node.cube))); // one last horizontal move when reaching the end
            };
            return NotRepeatingCubes(repeatingCubesNeighbors);
        }
        #endregion

        // Helper variable
        public PathNode prev;

        // State variables
        public Cube cube;
        public Vector2Int lastHorMove;
        public float prevVerMove;

        public PathNode(PathNode prev, Cube cube)
        {
            this.prev = prev;
            this.cube = cube;
            if (prev == null)
            {
                lastHorMove = Vector2Int.zero;
                prevVerMove = 0;             
            }
            else
            {
                lastHorMove = LastHorMove();
                prevVerMove = (cube.Position - prev.cube.Position).y;
            }
        }

        Vector2Int LastHorMove()
        {
            if (prev == null)
            {
                return Vector2Int.zero;
            }
            else
            {
                var prevHorMove = (cube.Position - prev.cube.Position).XZ();
                return prevHorMove == Vector2Int.zero ? prev.LastHorMove() : prevHorMove;
            }
        }

        public IEnumerable<Cube> ReversePath()
        {
            var pn = this;
            while(pn != null)
            {
                yield return pn.cube;
                pn = pn.prev;
            }
        }

        public bool Equals(PathNode x, PathNode y)
        {
            return x.cube == y.cube && x.lastHorMove == y.lastHorMove && x.prevVerMove == y.prevVerMove;
        }

        /// <summary>
        /// Adapted from:
        /// https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-overriding-gethashcode
        /// </summary>
        public int GetHashCode(PathNode obj)
        {
            int hash = 17;

            hash = hash * 23 + (cube == null ? 0 : cube.GetHashCode());
            hash = hash * 23 + lastHorMove.GetHashCode();
            hash = hash * 23 + prevVerMove.GetHashCode();
            return hash;
        }
    }
}

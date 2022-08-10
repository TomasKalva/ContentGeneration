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

        /// <summary>
        /// Assumes that 
        ///     - each path has ordered cubes from start to end
        ///     - each path has at least 2 cubes
        /// </summary>
        static Cube[] FindPathEndsInFloor(CubeGroup floor, CubeGroup[] paths) 
        {
            var pathEnds =
                paths.Select(path => path.Cubes.First()).Concat(
                paths.Select(path => path.Cubes.Last())
                );
            //return pathEnds;
            return pathEnds.SetIntersect(floor.Cubes).ToArray();
        }

        static bool StaysConnected(CubeGroup floor, PathNode newNode)
        {
            var newFloor = floor.Cubes.Except(newNode.ReversePath()).ToCubeGroup(floor.Grid);
            return 
                // floor doesn't get disconnected
                newFloor.SplitToConnected().Count() <= 1 && 
                // new node is next to a cube of new floor
                newNode.cube.NeighborsHor().SetIntersect(newFloor.Cubes).Any();
        }
        /*
        static Neighbors<PathNode> DontSplitToMultiple(HashSet<Cube> floorSet1, HashSet<Cube> floorSet2, )
            => pathNode =>
                    neighbors(pathNode)
                    .Where(pn =>
                        floorSet1.Contains(pn.cube) ?
                            StaysConnected(floor1, pn.ReversePath()) :
                            floorSet2.Contains(pn.cube) ?
                                StaysConnected(floor2,  pn.ReversePath()) :
                                true);*/

        public static Neighbors<PathNode> PreserveConnectivity(Neighbors<PathNode> neighbors, CubeGroup cg1, CubeGroup cg2, CubeGroup otherPaths)
        {
            var floor1 = cg1.BottomLayer().Minus(otherPaths);
            var floor2 = cg2.BottomLayer().Minus(otherPaths);
            var floorSet1 = new HashSet<Cube>(floor1.Cubes);
            var floorSet2 = new HashSet<Cube>(floor2.Cubes);

            //var pathEndsInFloor1 = FindPathEndsInFloor(floor1, otherPaths);
            //var pathEndsInFloor2 = FindPathEndsInFloor(floor2, otherPaths);

            //var floorWithoutPathEnds1 = floor1.Minus(otherPaths);
            //var allPathsGroup = otherPaths.SelectMany(cg => cg.Cubes).ToCubeGroup(cg1.Grid);
            return NotIn(
                pathNode =>
                    neighbors(pathNode)
                    /*.Where(pn =>
                        floorSet1.Contains(pn.cube) ?
                            StaysConnected(floor1, pn) :
                            floorSet2.Contains(pn.cube) ?
                                StaysConnected(floor2, pn) :
                                true
                    )*/,
                otherPaths
                );
        }

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

        public static Neighbors<PathNode> BalconyStairsBalconyNeighbors(CubeGroup start, CubeGroup end, CubeGroup balcony1, CubeGroup balcony2)
        {
            //todo: make sure that the path is always valid - problems with returning to the balcony space
            var horizontal = HorizontalNeighbors();
            var vertical = VerticalNeighbors();
            var balconySet1 = new HashSet<Cube>(balcony1.Cubes);
            var balconySet2 = new HashSet<Cube>(balcony2.Cubes);
            var stairsNeighbors = StairsNeighbors();
            Neighbors<PathNode> repeatingCubesNeighbors = pathNode =>
            {
                // return only horizontal directions for 1st node
                return pathNode.prev == null || balconySet2.Contains(pathNode.cube) ?
                            horizontal(pathNode) :
                            balconySet1.Contains(pathNode.cube) ?
                                horizontal(pathNode).Concat(stairsNeighbors(pathNode)) :
                                stairsNeighbors(pathNode)
                                    .Where(nextNode => !balconySet2.Contains(nextNode.cube) || nextNode.prevVerMove == 0);// move to the second balcony horizontaly
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

        public int GetHashCode(PathNode obj)
        {
            int hash = 17;
            // Suitable nullity checks etc, of course :)

            hash = hash * 23 + (cube == null ? 0 : cube.GetHashCode());
            hash = hash * 23 + lastHorMove.GetHashCode();
            hash = hash * 23 + prevVerMove.GetHashCode();
            return hash;
        }
    }
}

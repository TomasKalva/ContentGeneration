using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    /*class Path
    {
        List<Cube> 
    }*/


    public class PathNode : IEqualityComparer<PathNode>
    {
        public static IEqualityComparer<PathNode> Comparer { get; } = new PathNode(null, null);

        #region Neighborhoods
        public static Neighbors<PathNode> HorizontalNeighbors(CubeGroup pathBoundingBox)
        {
            return pathNode => pathNode.cube.NeighborsHor().Intersect(pathBoundingBox.Cubes)
                                .Select(c => new PathNode(pathNode, c));
        }

        public static Neighbors<PathNode> VerticalNeighbors(CubeGroup pathBoundingBox)
        {
            return pathNode => pathNode.cube.NeighborsVer().Intersect(pathBoundingBox.Cubes)
                                .Select(c => new PathNode(pathNode, c));
        }

        public static Neighbors<PathNode> DirectionNeighbors(CubeGroup pathBoundingBox, params Vector3Int[] directions)
        {
            return pathNode => pathNode.cube.NeighborsDirections(directions).Intersect(pathBoundingBox.Cubes)
                                .Select(c => new PathNode(pathNode, c));
        }

        public static Neighbors<PathNode> StairsNeighbors(CubeGroup pathBoundingBox)
        {
            var horizontal = HorizontalNeighbors(pathBoundingBox);
            var vertical = VerticalNeighbors(pathBoundingBox);
            return pathNode => pathNode.prevVerMove == 0 ? horizontal(pathNode).Concat(vertical(pathNode)) : DirectionNeighbors(pathBoundingBox, pathNode.lastHorMove.X0Z())(pathNode);
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
                prevVerMove = 1; // trick the stairs algorithm to start with vertical move
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

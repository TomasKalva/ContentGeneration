using Assets.ShapeGrammarGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class Connections
    {
        Grid<Cube> Grid { get; }
        ShapeGrammarShapes sgShapes { get; }
        Paths paths { get; }

        public Connections(Grid<Cube> grid)
        {
            Grid = grid;
            sgShapes = new ShapeGrammarShapes(grid);
            paths = new Paths(grid);
        }

        #region Helper methods
        /// <summary>
        /// Cubes on inside part of the wall.
        /// </summary>
        CubeGroup WallSpaceInside(LevelElement le)
        {
            var boundaryWall = le.CG().AllBoundaryFacesH().CG();
            return boundaryWall.ExtrudeHor(false);
        }

        /// <summary>
        /// Cubes on outside part of the wall.
        /// </summary>
        CubeGroup WallSpaceOutside(LevelElement le)
        {
            var inside = le.CG();
            var boundaryWall = inside.AllBoundaryFacesH().CG();
            return boundaryWall.ExtrudeHor(true).Cubes.SetMinus(inside.Cubes).ToCubeGroup(le.Grid);
        }

        /// <summary>
        /// Horizontal edges of the area which contain floor.
        /// </summary>
        CubeGroup AreaEdgesWithFloor(LevelElement le)
        {
            return le.CG().ExtrudeHor(outside: false).BottomLayer();
        }
        #endregion

        // todo: add other paths level element to the api
        public delegate Connection ConnectionFromAddedAndPaths(LevelElement alreadyAdded, LevelElement existingPaths);
        public delegate LevelGeometryElement Connection(LevelElement le1, LevelElement le2);

        /// <summary>
        /// Creates door between two cubes of bottom layer of the elements.
        /// </summary>
        public Connection ConnectByDoor(LevelElement _, LevelElement existingPaths)
        {
            return (le1, le2) =>
            {
                var area1Floor = le1.CG().BottomLayer();
                var area2FloorSet = new HashSet<Cube>(le2.CG().BottomLayer().Cubes);
                var neighbors = from fl1 in area1Floor.Cubes
                                from fl2 in fl1.NeighborsHor().Where(neighbor => area2FloorSet.Contains(neighbor))
                                select new[] { fl1, fl2 };
                return neighbors.Any() ?
                    neighbors.GetRandom().ToCubeGroup(le1.Grid).LE(AreaStyles.Connection()) :
                    null;
            };
        }

        /// <summary>
        /// Creates stairs between cubes of the elements. The stairs don't leave the elements.
        /// </summary>
        public Connection ConnectByWallStairsIn(LevelElement _, LevelElement existingPaths)
        {
            return (le1, le2) =>
            {
                var space1 = WallSpaceInside(le1);
                var space2 = WallSpaceInside(le2);

                Neighbors<PathNode> neighbors = PathNode.PreserveConnectivity(
                    PathNode.BoundedBy(PathNode.StairsNeighbors(), space1.Merge(space2)),
                    space1, space2, existingPaths.CG()
                );
                var path = paths.ConnectByPath(space1.BottomLayer(), space2.BottomLayer(), neighbors);
                return path != null ? path.LE(AreaStyles.Path()) : null;
            };
        }

        /// <summary>
        /// Connects level elements by stairs outside of the area faces.
        /// </summary>
        public Connection ConnectByWallStairsOut(LevelElement alreadyAdded, LevelElement existingPaths)
        {
            return (le1, le2) =>
            {
                var start = AreaEdgesWithFloor(le1);
                var end = AreaEdgesWithFloor(le2);
                var searchSpace = WallSpaceOutside(le1.Merge(le2)).Merge(start).Merge(end);
                Neighbors<PathNode> neighbors = 
                    PathNode.NotIn(
                        PathNode.BoundedBy(
                            PathNode.StairsNeighbors(), 
                            searchSpace
                        ), 
                        alreadyAdded.CG().Minus(end)
                    );
                var path = paths.ConnectByPath(start, end, neighbors);
                return path != null ? path.LE(AreaStyles.Path()) : null;
            };
        }
        /*
        public LevelGeometryElement ConnectByElevator(LevelElement le1, LevelElement le2)
        {
            var space1 = le1.CG();
            var space2 = le2.CG();
            var start = space1.BottomLayer();
            var end = space2.BottomLayer();
            Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.ElevatorNeighbors(end), space1.Merge(space2));
            var path = paths.ConnectByPath(start, end, neighbors);
            return path != null ? path.LE(AreaStyles.Elevator) : null;
        }
        */

        /// <summary>
        /// Connects the level elements by straight vertical fall.
        /// </summary>
        public Connection ConnectByFall(LevelElement _, LevelElement existingPaths)
        {
            return (from, to) =>
            {
                var space1 = from.CG();
                var space2 = to.CG();
                var start = space1.BottomLayer();
                var end = space2.BottomLayer();
                Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.FallNeighbors(end), space1.Merge(space2));
                var path = paths.ConnectByPath(start, end, neighbors);
                return path != null ? path.LE(AreaStyles.Fall()) : null;
            };
        }


        public Connection ConnectByBalconyStairsOutside(LevelElement alreadyAdded, LevelElement existingPaths)
        {
            return (le1, le2) =>
            {
                // todo: the path can moved through both start and end so it isn't connected just by balconies, maybe fix this
                var start = AreaEdgesWithFloor(le1);
                var end = AreaEdgesWithFloor(le2);

                var balconySpaceStart = WallSpaceOutside(le1.CG().BottomLayer().LE());
                var balconySpaceEnd = WallSpaceOutside(le2.CG().BottomLayer().LE());

                var notIntersectingCG = alreadyAdded.CG().Minus(end);
                Neighbors<PathNode> neighbors =
                    PathNode.NotIn(
                            PathNode.BalconyStairsBalconyNeighbors(start, end, balconySpaceStart, balconySpaceEnd),
                            notIntersectingCG
                    );
                var path = paths.ConnectByPath(start, end, neighbors);
                return path != null ? path.LE(AreaStyles.Path()) : null;
            };
        }

        public Connection ConnectByBridge(LevelElement alreadyAdded, LevelElement existingPaths)
        {
            return (le1, le2) =>
            {
                var space1 = le1.CG();
                var space2 = le2.CG();
                var notIntersectingCG = alreadyAdded.CG().Minus(space2);
                Neighbors<PathNode> neighbors = PathNode.NotIn(PathNode.StraightHorizontalNeighbors(), notIntersectingCG);
                var path = paths.ConnectByPath(space1.BottomLayer(), space2.BottomLayer(), neighbors);
                return path != null ? path.LE(AreaStyles.Path()) : null;
            };
        }

        /*
        public LevelGeometryElement ConnectByStairsInside(LevelElement le1, LevelElement le2)
            => ConnectByStairsInside(le1, le2, null);
        */

        public Connection ConnectByStairsInside(LevelElement _, LevelElement existingPaths)
        {
            return (le1, le2) =>
            {
                var space1 = le1.CG();
                var space2 = le2.CG();
                var start = space1.BottomLayer();
                var end = space2.BottomLayer();
                Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.StairsNeighbors(), space1.Merge(space2));
                var path = paths.ConnectByPath(start, end, neighbors);
                return path != null ? path.LE(AreaStyles.Path()) : null;
            };
        }
        /*
        public Connection ConnectByStairs(LevelElement notIntersecting)
        {
            return (le1, le2) =>
            {
                // todo: the path can moved through both start and end so it isn't connected just by balconies, maybe fix this
                var start = le1.CG();
                var end = le2.CG();

                Neighbors<PathNode> neighbors =
                    PathNode.NotIn(
                            PathNode.StairsNeighbors(),
                            notIntersecting.CG()
                    );
                var path = paths.ConnectByPath(start, end, neighbors);
                return path != null ? path.LE(AreaStyles.Path()) : null;
            };
        }*/
    }
}

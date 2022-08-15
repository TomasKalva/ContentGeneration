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
        Paths paths { get; }

        public Connections()
        {
            paths = new Paths();
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
        public delegate Connection ConnectionFromAddedAndPaths(LevelElement alreadyAdded, LevelGroupElement existingPaths);
        public delegate LevelGeometryElement Connection(LevelElement le1, LevelElement le2);

        /// <summary>
        /// Creates door between two cubes of bottom layer of the elements.
        /// </summary>
        public Connection ConnectByDoor(LevelElement _, LevelGroupElement existingPaths)
        {
            return (le1, le2) =>
            {
                var space1 = WallSpaceInside(le1);
                var space2 = WallSpaceInside(le2);
                Neighbors<PathNode> neighbors = PathNode.BoundedBy(
                        PathNode.HorizontalNeighbors(),
                        space1.Merge(space2)
                );
                var path = paths.ConnectByConnectivityPreservingPath(space1.BottomLayer(), space2.BottomLayer(), le1.CG().BottomLayer(), le2.CG().BottomLayer(), neighbors, existingPaths);
                return path != null ? path.LE(AreaStyles.Connection()) : null;
            };
        }

        /// <summary>
        /// Creates stairs between cubes of the elements. The stairs don't leave the elements.
        /// </summary>
        public Connection ConnectByWallStairsIn(LevelElement _, LevelGroupElement existingPaths)
        {
            return (le1, le2) =>
            {
                var space1 = WallSpaceInside(le1);
                var space2 = WallSpaceInside(le2);

                Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.StairsNeighbors(), space1.Merge(space2));
                var path = paths.ConnectByConnectivityPreservingPath(space1.BottomLayer(), space2.BottomLayer(), le1.CG().BottomLayer(), le2.CG().BottomLayer(), neighbors, existingPaths);
                return path != null ? path.LE(AreaStyles.Path()) : null;
            };
        }

        /// <summary>
        /// Connects level elements by stairs outside of the area faces.
        /// </summary>
        public Connection ConnectByWallStairsOut(LevelElement alreadyAdded, LevelGroupElement existingPaths)
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
                        alreadyAdded.CG().Minus(end));
                var path = paths.ConnectByConnectivityPreservingPath(start, end, le1.CG().BottomLayer(), le2.CG().BottomLayer(), neighbors, existingPaths);
                return path != null ? path.LE(AreaStyles.Path()) : null;
            };
        }

        /// <summary>
        /// Connects the level elements by straight vertical fall.
        /// </summary>
        public Connection ConnectByFall(LevelElement _, LevelGroupElement existingPaths)
        {
            return (from, to) =>
            {
                var space1 = from.CG();
                var space2 = to.CG();
                var start = space1.BottomLayer();
                var end = space2.BottomLayer();
                Neighbors<PathNode> neighbors = 
                    PathNode.BoundedBy(PathNode.FallNeighbors(end), space1.Merge(space2));
                var path = paths.ConnectByConnectivityPreservingPath(start, end, start, end, neighbors, existingPaths);
                return path != null ? path.LE(AreaStyles.Fall()) : null;
            };
        }

        /// <summary>
        /// Connects the level elements by stairs that come out of the edges of the room.
        /// </summary>
        public Connection ConnectByStairsOutside(LevelElement alreadyAdded, LevelGroupElement existingPaths)
        {
            return (le1, le2) =>
            {
                // todo: the path can moved through both start and end so it isn't connected just by balconies, maybe fix this
                var start = AreaEdgesWithFloor(le1);
                var end = AreaEdgesWithFloor(le2);

                var notIntersectingCG = alreadyAdded.CG().Minus(end);
                Neighbors<PathNode> neighbors = 
                    PathNode.NotIn(
                        PathNode.StairsNeighbors(),
                        notIntersectingCG
                    );
                var path = paths.ConnectByConnectivityPreservingPath(start, end, le1.CG().BottomLayer(), le2.CG().BottomLayer(), neighbors, existingPaths);
                return path != null ? path.LE(AreaStyles.Path()) : null;
            };
        }

        /// <summary>
        /// Connects the level elements by a straight bridge.
        /// </summary>
        public Connection ConnectByBridge(LevelElement alreadyAdded, LevelGroupElement existingPaths)
        {
            return (le1, le2) =>
            {
                var space1 = le1.CG();
                var space2 = le2.CG();
                var notIntersectingCG = alreadyAdded.CG().Minus(space2);
                Neighbors<PathNode> neighbors = 
                    PathNode.NotIn(PathNode.StraightHorizontalNeighbors(), notIntersectingCG);
                var path = paths.ConnectByConnectivityPreservingPath(space1.BottomLayer(), space2.BottomLayer(), le1.CG().BottomLayer(), le2.CG().BottomLayer(), neighbors, existingPaths);
                return path != null ? path.LE(AreaStyles.Path()) : null;
            };
        }

        /// <summary>
        /// Connects the level elements by stairs inside of them.
        /// </summary>
        public Connection ConnectByStairsInside(LevelElement _, LevelGroupElement existingPaths)
        {
            return (le1, le2) =>
            {
                var space1 = le1.CG();
                var space2 = le2.CG();
                var start = space1.BottomLayer();
                var end = space2.BottomLayer();
                Neighbors<PathNode> neighbors = 
                    PathNode.BoundedBy(PathNode.StairsNeighbors(), space1.Merge(space2));
                var path = paths.ConnectByConnectivityPreservingPath(start, end, le1.CG().BottomLayer(), le2.CG().BottomLayer(), neighbors, existingPaths);
                return path != null ? path.LE(AreaStyles.Path()) : null;
            };
        }
    }
}

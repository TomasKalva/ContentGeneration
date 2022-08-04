﻿using Assets.ShapeGrammarGenerator;
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

        public delegate Connection ConnectionNotIntersecting(LevelElement notIntersected);
        public delegate LevelGeometryElement Connection(LevelElement le1, LevelElement le2);

        public LevelGeometryElement ConnectByDoor(LevelElement le1, LevelElement le2)
        {
            var area1Floor = le1.CG().BottomLayer();
            var area2FloorSet = new HashSet<Cube>(le2.CG().BottomLayer().Cubes);
            var neighbors = from fl1 in area1Floor.Cubes
                            from fl2 in fl1.NeighborsHor().Where(neighbor => area2FloorSet.Contains(neighbor))
                            select new []{ fl1, fl2 };
            return neighbors.Any() ?
                neighbors.GetRandom().ToCubeGroup(le1.Grid).LE(AreaStyles.Door()) :
                null;
        }

        CubeGroup WallSpaceInside(LevelElement le)
        {
            var boundaryWall = le.CG().AllBoundaryFacesH()/*.Where(face => face.FaceType == FACE_HOR.Wall)*/.CG();
            return boundaryWall.ExtrudeHor(false);
        }

        public LevelGeometryElement ConnectByWallStairsIn(LevelElement le1, LevelElement le2)
        {
            var space1 = WallSpaceInside(le1);
            var space2 = WallSpaceInside(le2);
            Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.StairsNeighbors(), space1.Merge(space2));
            var path = paths.ConnectByPath(space1.BottomLayer(), space2.BottomLayer(), neighbors);
            return path != null ? path.LE(AreaStyles.Path()) : null;
        }

        /// <summary>
        /// Cubes on outside part of the wall.
        /// </summary>
        CubeGroup WallSpaceOutside(LevelElement le)
        {
            var inside = le.CG();
            var boundaryWall = inside.AllBoundaryFacesH().Where(face => face.FaceType == FACE_HOR.Wall).CG();
            return boundaryWall.ExtrudeHor(true).Cubes.SetMinus(inside.Cubes).ToCubeGroup(le.Grid);
        }

        /// <summary>
        /// Horizontal edges of the area which contain floor.
        /// </summary>
        CubeGroup AreaEdgesWithFloor(LevelElement le)
        {
            return le.CG().ExtrudeHor(outside: false).BottomLayer();
        }

        public Connection ConnectByWallStairsOut(LevelElement notIntersecting)
        {
            return (le1, le2) =>
            {
                var start = AreaEdgesWithFloor(le1);
                var end = AreaEdgesWithFloor(le2);
                var searchSpace = WallSpaceOutside(le1).Merge(WallSpaceOutside(le2)).Merge(start).Merge(end);
                Neighbors<PathNode> neighbors = PathNode.NotIn(PathNode.BoundedBy(PathNode.StairsNeighbors(), searchSpace), notIntersecting.CG().Minus(end));
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
        public LevelGeometryElement ConnectByFall(LevelElement from, LevelElement to)
        {
            var space1 = from.CG();
            var space2 = to.CG();
            var start = space1.BottomLayer();
            var end = space2.BottomLayer();
            Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.ElevatorNeighbors(end), space1.Merge(space2));
            var path = paths.ConnectByPath(start, end, neighbors);
            return path != null ? path.LE(AreaStyles.Fall()) : null;
        }

        public Connection ConnectByBalconyStairsOutside(LevelElement notIntersecting)
        {
            return (le1, le2) =>
            {
                // todo: the path can moved through both start and end so it isn't connected just by balconies, maybe fix this
                var start = AreaEdgesWithFloor(le1);
                var end = AreaEdgesWithFloor(le2);

                var balconySpaceStart = WallSpaceOutside(le1.CG().BottomLayer().LE());
                var balconySpaceEnd = WallSpaceOutside(le2.CG().BottomLayer().LE());

                var notIntersectingCG = notIntersecting.CG().Minus(end);
                Neighbors<PathNode> neighbors =
                    PathNode.NotIn(
                            PathNode.BalconyStairsBalconyNeighbors(start, end, balconySpaceStart, balconySpaceEnd),
                            notIntersectingCG
                    );
                var path = paths.ConnectByPath(start, end, neighbors);
                return path != null ? path.LE(AreaStyles.Path()) : null;
            };
        }

        public Connection ConnectByBridge(LevelElement notIntersecting)
        {
            return (le1, le2) =>
            {
                var space1 = le1.CG();
                var space2 = le2.CG();
                Neighbors<PathNode> neighbors = PathNode.NotIn(PathNode.StraightHorizontalNeighbors(), notIntersecting.CG());
                var path = paths.ConnectByPath(space1.BottomLayer(), space2.BottomLayer(), neighbors);
                return path != null ? path.LE(AreaStyles.Path()) : null;
            };
        }

        public LevelGeometryElement ConnectByStairsInside(LevelElement le1, LevelElement le2)
            => ConnectByStairsInside(le1, le2, null);

            public LevelGeometryElement ConnectByStairsInside(LevelElement le1, LevelElement le2, LevelElement bounds = null)
        {
            if (bounds == null)
                bounds = LevelElement.Empty(Grid);

            var space1 = le1.CG();
            var space2 = le2.CG();
            var start = space1.BottomLayer();
            var end = space2.BottomLayer();
            Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.StairsNeighbors(), space1.Merge(space2).Merge(bounds.CG()));
            var path = paths.ConnectByPath(start, end, neighbors);
            return path != null ? path.LE(AreaStyles.Path()) : null;
        }
    }
}

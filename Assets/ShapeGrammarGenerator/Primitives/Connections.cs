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

        public LevelGeometryElement ConnectByDoor(LevelElement le1, LevelElement le2)
        {
            var area1Floor = le1.CubeGroup().WithFloor();
            var area2FloorSet = new HashSet<Cube>(le2.CubeGroup().WithFloor().Cubes);
            var neighbors = from fl1 in area1Floor.Cubes
                            from fl2 in fl1.NeighborsHor().Where(neighbor => area2FloorSet.Contains(neighbor))
                            select new []{ fl1, fl2 };
            return neighbors.Any() ?
                neighbors.GetRandom().ToCubeGroup(le1.Grid).LevelElement(AreaType.Door) :
                null;
        }

        CubeGroup WallSpaceInside(LevelElement le)
        {
            var boundaryWall = le.CubeGroup().AllBoundaryFacesH().Where(face => face.FaceType == FACE_HOR.Wall).CubeGroup();
            return boundaryWall.ExtrudeHor(false);
        }

        public LevelGeometryElement ConnectByWallStairsIn(LevelElement le1, LevelElement le2)
        {
            var space1 = WallSpaceInside(le1);
            var space2 = WallSpaceInside(le2);
            Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.StairsNeighbors(), space1.Merge(space2));
            var path = paths.ConnectByPath(space1.WithFloor(), space2.WithFloor(), neighbors);
            return path != null ? path.LevelElement(AreaType.Path) : null;
        }

        CubeGroup WallSpaceOutside(LevelElement le)
        {
            var inside = le.CubeGroup();
            var boundaryWall = inside.AllBoundaryFacesH().Where(face => face.FaceType == FACE_HOR.Wall).CubeGroup();
            return boundaryWall.ExtrudeHor(true).Cubes.SetMinus(inside.Cubes).ToCubeGroup(le.Grid);
        }

        CubeGroup RoomEdgesWithFloor(LevelElement le)
        {
            return le.CubeGroup().ExtrudeHor(outside: false).WithFloor();
        }

        public LevelGeometryElement ConnectByWallStairsOut(LevelElement le1, LevelElement le2, LevelElement notIntersecting)
        {
            var start = RoomEdgesWithFloor(le1);
            var end = RoomEdgesWithFloor(le2);
            var searchSpace = WallSpaceOutside(le1).Merge(WallSpaceOutside(le2)).Merge(start).Merge(end);
            Neighbors<PathNode> neighbors = PathNode.NotIn(PathNode.BoundedBy(PathNode.StairsNeighbors(), searchSpace), notIntersecting.CubeGroup().Minus(end));
            var path = paths.ConnectByPath(start, end, neighbors);
            return path != null ? path.LevelElement(AreaType.Path) : null;
        }

        public LevelGeometryElement ConnectByElevator(LevelElement le1, LevelElement le2)
        {
            var space1 = le1.CubeGroup();
            var space2 = le2.CubeGroup();
            Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.ElevatorNeighbors(space2), space1.Merge(space2));
            var path = paths.ConnectByPath(space1.WithFloor(), space2.WithFloor(), neighbors);
            return path != null ? path.LevelElement(AreaType.Elevator) : null;
        }

        public LevelGeometryElement ConnectByBalconyStairsOutside(LevelElement le1, LevelElement le2, LevelElement notIntersecting)
        {
            var start = RoomEdgesWithFloor(le1);
            var end = RoomEdgesWithFloor(le2);

            var balconySpaceStart = WallSpaceOutside(le1);
            var balconySpaceEnd = WallSpaceOutside(le2);

            Neighbors<PathNode> neighbors = 
                PathNode.NotIn(
                        PathNode.BalconyStairsBalconyNeighbors(start, end, balconySpaceStart, balconySpaceEnd), 
                notIntersecting.CubeGroup().Minus(end));
            var path = paths.ConnectByPath(start, end, neighbors);
            return path != null ? path.LevelElement(AreaType.Path) : null;
        }

        public LevelGeometryElement ConnectByBridge(LevelElement le1, LevelElement le2, LevelElement notIntersecting)
        {
            var space1 = le1.CubeGroup();
            var space2 = le2.CubeGroup();
            Neighbors<PathNode> neighbors = PathNode.NotIn(PathNode.StraightHorizontalNeighbors(), notIntersecting.CubeGroup());
            var path = paths.ConnectByPath(space1.WithFloor(), space2.WithFloor(), neighbors);
            return path != null ? path.LevelElement(AreaType.Bridge) : null;
        }
    }
}

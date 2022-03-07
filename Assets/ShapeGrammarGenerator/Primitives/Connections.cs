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
        Grid Grid { get; }
        ShapeGrammarShapes sgShapes { get; }
        Paths paths { get; }

        public Connections(Grid grid)
        {
            Grid = grid;
            sgShapes = new ShapeGrammarShapes(grid);
            paths = new Paths(grid);
        }

        public LevelGeometryElement ConnectByDoor(LevelElement area1, LevelElement area2)
        {
            var area1Floor = area1.CubeGroup().WithFloor();
            var area2FloorSet = new HashSet<Cube>(area2.CubeGroup().WithFloor().Cubes);
            var neighbors = from fl1 in area1Floor.Cubes
                            from fl2 in fl1.NeighborsHor().Where(neighbor => area2FloorSet.Contains(neighbor))
                            select new []{ fl1, fl2 };
            return neighbors.Any() ?
                neighbors.GetRandom().ToCubeGroup(area1.Grid).LevelElement(AreaType.Door) :
                null;
        }
    }
}

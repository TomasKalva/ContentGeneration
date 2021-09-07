using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WorldTraversingAgent
{
    public void Traverse(ModuleGrid grid)
    {
        var areasClosenessImpl = new ImplicitGraph<Area>(
            area => area.Modules.SelectMany(
                module => module.HorizontalNeighbors(grid)
                            .Where(neighbor => neighbor.GetProperty<TopologyProperty>().HasFloor(grid, neighbor.DirectionTo(module)))
                        .Concat(
                            module.VerticalNeighbors(grid).Where(neighbor => neighbor.GetProperty<TopologyProperty>().HasFloor(grid))
                        ))
                        .Select(neighbor => neighbor.GetProperty<AreaModuleProperty>().Area)
                    .Distinct()
                    .Where(otherArea => otherArea != area)
            );
        var areas = grid.Where(module => module.GetProperty<TopologyProperty>().HasFloor(grid))
                    .Select(module => module.GetProperty<AreaModuleProperty>().Area).Distinct()
                    .ToList();

        //var areasCloseness = areasClosenessImpl.ToGraph(areas);
        var areasClosenessAlg = new GraphAlgorithms<Area, Edge<Area>, IGraph<Area, Edge<Area>>>(areasClosenessImpl);

        var areasConnnections = new Graph<Area>(areas, new List<Edge<Area>>());
        var areasConnnectionsAlg = new GraphAlgorithms<Area, Edge<Area>, IGraph<Area, Edge<Area>>>(areasConnnections);

        // choose a starting location
        var startingArea = areas.GetRandom();


        /*var found = new List<Edge<Area>>();
        var fringe = new Stack<Area>();
        fringe.Push(start);
        while (fringe.Any())
        {
            var v = fringe.Pop();
            if (found.Contains(v))
            {
                continue;
            }

            found.Add(v);
            foreach (var edge in graph.EdgesFrom(v))
            {
                yield return edge;
                fringe.Push(edge.Other(v));
            }
        }*/
        // navigate through the area graph

        // find add interesting points to the area

        // remember all the connections to other areas
        foreach (var areasConnection in areasClosenessAlg.EdgeDFS(startingArea))
        {
            if(areasConnnectionsAlg.Path(areasConnection.From, areasConnection.To))
            {
                continue;
            }

            var borderModules = areasConnection.From.Modules.Where(
                     border => border.AllNeighbors(grid)
                         .Where(neighbor => CanConnectToArea(grid, border, neighbor, areasConnection.To)).Any())
                    .Distinct();
            var connectFrom = borderModules.GetRandom();

            if(connectFrom == null)
            {
                Debug.Log("fail");
                continue;
            }
            var connectTo = connectFrom.AllNeighbors(grid).Where(neighbor => CanConnectToArea(grid, connectFrom, neighbor, areasConnection.To)).GetRandom();

            if (connectTo == null)
            {
                Debug.Log("fail");
                continue;
            }
            var dirFromTo = connectFrom.DirectionTo(connectTo);

            if(dirFromTo.y == 0)
            {
                // horizontal
                connectFrom.GetAttachmentPoint(dirFromTo).objectType = ObjectType.Door;
                connectTo.GetAttachmentPoint(-dirFromTo).objectType = ObjectType.Empty;
            }
            else if(dirFromTo.y > 0)
            {
                // up
                connectFrom.GetObject().objectType = ObjectType.Stairs;
            }
            else
            {
                // down
                connectTo.GetObject().objectType = ObjectType.Stairs;
            }
            areasConnnections.AddEdge(areasConnection);

        }
    }

    bool CanConnectToArea(ModuleGrid grid, Module from, Module to, Area toArea)
    {
        if (to.GetProperty<AreaModuleProperty>().Area != toArea)
            return false;

        var dirFromTo = from.DirectionTo(to);
        if(dirFromTo.y == 0)
        {
            // horizontal connection
            return from.GetProperty<TopologyProperty>().HasFloor(grid, dirFromTo) && to.GetProperty<TopologyProperty>().HasFloor(grid, -dirFromTo);
        }
        else
        {
            // vertical connection
            return from.GetProperty<TopologyProperty>().HasFloor(grid) && to.GetProperty<TopologyProperty>().HasFloor(grid);
        }
    }
}

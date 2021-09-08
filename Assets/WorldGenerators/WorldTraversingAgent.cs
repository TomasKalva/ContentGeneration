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
                module => module.AllNeighbors(grid).Where(neighbor => CanConnectToModule(grid, module, neighbor)))
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
        
        /*var graph = new Graph<string>(new List<string>() { "0", "1", "2", "3" }, new List<Edge<string>>() { new Edge<string>("0", "1"), new Edge<string>("1", "2"), new Edge<string>("2", "3") });
        var graphAlg = new GraphAlgorithms<string, Edge<string>, Graph<string>>(graph);

        var dist = graphAlg.Distance("0", "1");
        Debug.Log($"test distance: {dist}");
        */

        // navigate through the area graph

        // find add interesting points to the area

        // remember all the connections to other areas
        foreach (var areasConnection in areasClosenessAlg.EdgeDFS(startingArea))
        {
            /*if(areasConnnectionsAlg.Path(areasConnection.From, areasConnection.To))
            {
                continue;
            }*/




            var infinity = 1000_000;
            var distStartFrom = areasConnnectionsAlg.Distance(startingArea, areasConnection.From, infinity);
            var distStartTo = areasConnnectionsAlg.Distance(startingArea, areasConnection.To, infinity);
            //Debug.Log($"dist from: {distStartFrom}, dist to: {distStartTo}");

            ObjectType? objectType = null;

            int shortcutLength = distStartFrom - distStartTo;
            if (shortcutLength >= 7)
            {
                Debug.Log($"Placing shortcut of length: {shortcutLength}");
                //objectType = ObjectType.DebugMarker;
            }
            else if(distStartTo < infinity)
            {
                // don't put connections to already visited rooms
                continue;
            }

            areasConnnections.AddEdge(areasConnection);

            var borderModules = areasConnection.From.Modules.Where(
                     border => border.AllNeighbors(grid)
                         .Where(neighbor => CanConnectToArea(grid, border, neighbor, areasConnection.To)).Any())
                    .Distinct();
            var connectFrom = borderModules.GetRandom();

            if (connectFrom == null)
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


            if (objectType.HasValue)
            {
                connectTo.GetObject().objectType = objectType.Value;
            }
        }
    }

    bool CanConnectToModule(ModuleGrid grid, Module from, Module to)
    {
        var dirFromTo = from.DirectionTo(to);
        if (dirFromTo.y == 0)
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

    bool CanConnectToArea(ModuleGrid grid, Module from, Module to, Area toArea)
    {
        return to.GetProperty<AreaModuleProperty>().Area == toArea && CanConnectToModule(grid, from, to); 
        /*var dirFromTo = from.DirectionTo(to);
        if(dirFromTo.y == 0)
        {
            // horizontal connection
            return from.GetProperty<TopologyProperty>().HasFloor(grid, dirFromTo) && to.GetProperty<TopologyProperty>().HasFloor(grid, -dirFromTo);
        }
        else
        {
            // vertical connection
            return from.GetProperty<TopologyProperty>().HasFloor(grid) && to.GetProperty<TopologyProperty>().HasFloor(grid);
        }*/
    }

    bool AreasCanBeConnected(ModuleGrid grid, Area from, Area to)
    {
        var borderModules = from.Modules.Where(
                 border => border.AllNeighbors(grid)
                     .Where(neighbor => CanConnectToArea(grid, border, neighbor, to)).Any())
                .Distinct();
        var connectFrom = borderModules.GetRandom();

        if (connectFrom == null)
        {
            //Debug.Log("fail");
            return false;
        }
        var connectTo = connectFrom.AllNeighbors(grid).Where(neighbor => CanConnectToArea(grid, connectFrom, neighbor, to)).GetRandom();

        if (connectTo == null)
        {
            //Debug.Log("fail");
            return false;
        }

        return true;
    }
}

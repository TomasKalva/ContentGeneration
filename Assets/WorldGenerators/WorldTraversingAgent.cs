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

        // navigate through the area graph
        int connnectionCount = 0;
        foreach(var areasConnection in areasClosenessAlg.EdgeDFS(startingArea))
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
                continue;
            }
            var connectTo = connectFrom.AllNeighbors(grid).Where(neighbor => CanConnectToArea(grid, connectFrom, neighbor, areasConnection.To)).GetRandom();

            if (connectTo == null)
            {
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


            /*Debug.Log($"Area from: {connectFrom.GetProperty<AreaModuleProperty>().Area.Name}");
            Debug.Log($"Area to: {connectTo.GetProperty<AreaModuleProperty>().Area.Name}");

            Debug.Log($"Areas connection from: {areasConnection.From.Name}");
            Debug.Log($"Areas connection to: {areasConnection.To.Name}");*/

            /*connectFrom.GetAttachmentPoint(dirFromTo).objectType = ObjectType.Door;
            connectTo.GetAttachmentPoint(-dirFromTo).objectType = ObjectType.Empty;

            areasConnnections.AddEdge(areasConnection);
            //Debug.Log("Connection");
            connnectionCount++;*/
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

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
                module => module.AllNeighbors(grid)
                        .Where(neighbor =>
                            neighbor.GetProperty<AreaModuleProperty>().Area != module.GetProperty<AreaModuleProperty>().Area)
                        .Select(neighbor => neighbor.GetProperty<AreaModuleProperty>().Area))
                    .Distinct()
                    .Where(otherArea => otherArea.AreaType != "Empty" && otherArea.AreaType != "Outside" && otherArea != area)
            );
        var areas = grid.Select(module => module.GetProperty<AreaModuleProperty>().Area).Distinct()
                    .Where(area => area.AreaType != "Empty" && area.AreaType != "Outside").ToList();
        //var areasCloseness = areasClosenessImpl.ToGraph(areas);
        var areasClosenessAlg = new GraphAlgorithms<Area, Edge<Area>, IGraph<Area, Edge<Area>>>(areasClosenessImpl);

        var areasConnnections = new Graph<Area>(areas, new List<Edge<Area>>());

        // choose a starting location
        var startingArea = areas.GetRandom();

        // navigate through the area graph
        int connnectionCount = 0;
        foreach(var areasConnection in areasClosenessAlg.EdgeDFS(startingArea))
        {
            if(areasConnnections.AreConnected(areasConnection.From, areasConnection.To))
            {
                continue;
            }

            var borderModules = areasConnection.From.Modules.Where(module => !module.Empty).Where(
                     border => border.AllNeighbors(grid)
                         .Where(module => !module.Empty)
                         .Where(neighbor => neighbor.GetProperty<AreaModuleProperty>().Area == areasConnection.To).Any())
                    .Distinct();
            var connectFrom = borderModules.GetRandom();

            if(connectFrom == null)
            {
                continue;
            }
            var connectTo = connectFrom.HorizontalNeighbors(grid).Where(module => !module.Empty).Where(module => module.GetProperty<AreaModuleProperty>().Area == areasConnection.To).GetRandom();

            if (connectTo == null)
            {
                continue;
            }
            var dirFromTo = connectFrom.DirectionTo(connectTo);

            if(dirFromTo == Vector3Int.up || dirFromTo == Vector3Int.down)
            {
                continue;
            }

            /*Debug.Log($"Area from: {connectFrom.GetProperty<AreaModuleProperty>().Area.Name}");
            Debug.Log($"Area to: {connectTo.GetProperty<AreaModuleProperty>().Area.Name}");

            Debug.Log($"Areas connection from: {areasConnection.From.Name}");
            Debug.Log($"Areas connection to: {areasConnection.To.Name}");*/

            connectFrom.GetAttachmentPoint(dirFromTo).objectType = ObjectType.Door;
            connectTo.GetAttachmentPoint(-dirFromTo).objectType = ObjectType.Empty;

            areasConnnections.AddEdge(areasConnection);
            //Debug.Log("Connection");
            connnectionCount++;
        }
    }
}

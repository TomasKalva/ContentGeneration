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
            );
        var areas = grid.Select(module => module.GetProperty<AreaModuleProperty>().Area).Distinct().ToList();
        //var areasCloseness = areasClosenessImpl.ToGraph(areas);
        var areasClosenessAlg = new GraphAlgorithms<Area, Edge<Area>, IGraph<Area, Edge<Area>>>(areasClosenessImpl);

        // choose a starting location
        var startingArea = areas.GetRandom();

        // navigate through the area graph
        int connnectionCount = 0;
        foreach(var areasConnection in areasClosenessAlg.EdgeDFS(startingArea))
        {
            Debug.Log("Connection");
            connnectionCount++;
        }
    }
}

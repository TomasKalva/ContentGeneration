using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Rooftops : Template
{
    public Rooftops(Modules moduleLibrary, Styles styles) : base(moduleLibrary, styles)
    {
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        var heightGraph = new ImplicitGraph<Module>(
                module => module.HorizontalNeighbors(moduleGrid)
                            .Where(neighbor => IsRoof(moduleGrid, neighbor)));
        var heightGraphAlg = new GraphAlgorithms<Module, NoEdge<Module>, ImplicitGraph<Module>>(heightGraph);
        var roofComponents = heightGraphAlg.ConnectedComponentsSymm(moduleGrid.Where(module => IsRoof(moduleGrid, module)));
        foreach(var component in roofComponents)
        {
            var area = new Rooftop(component, moduleLibrary, styles);
            area.Generate(moduleGrid);
        }

        return true;
    }

    /*int RooftopHeight(ModuleGrid moduleGrid, Vector2Int coords)
    {
        for(int y = 0; y < moduleGrid.Height; y++)
        {
            if(moduleGrid[new Vector3Int(coords.x, y, coords.y)].empty)
            {
                return y;
            }
        }
        return moduleGrid.Height;
    }*/

    bool IsRoof(ModuleGrid moduleGrid, Module maybeRoof)
    {
        if (maybeRoof.empty)
        {
            var topology = maybeRoof.GetProperty<TopologyProperty>();
            //var oneDown = maybeRoof.coords - Vector3Int.up;
            return topology.HasFloor(moduleGrid) && maybeRoof.AllAbove(moduleGrid).All(module => module == null || module.empty);// moduleGrid.ValidCoords(oneDown) && moduleGrid[oneDown].Has;
        }
        return false;
    }
}

public class Rooftop : Template
{
    List<Module> modules;

    public Rooftop(IEnumerable<Module> modules, Modules moduleLibrary, Styles styles) : base(moduleLibrary, styles)
    {
        this.modules = modules.ToList();
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        var rooftopArea = new DisconnectedArea(new RoofDesigner(moduleGrid), styles.gothic);
        rooftopArea.Name = "Rooftop";
        foreach (var module in modules)
        {
            var m = moduleLibrary.RoomModule(rooftopArea);
            moduleGrid[module.coords] = m;
        }
        return true;
    }
}

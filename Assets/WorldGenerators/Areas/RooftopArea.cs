using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Rooftops : Area
{
    public Rooftops(Modules moduleLibrary) : base(moduleLibrary)
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
            var area = new RooftopArea(component, moduleLibrary);
            area.Generate(moduleGrid);
        }

        return true;
    }

    int RooftopHeight(ModuleGrid moduleGrid, Vector2Int coords)
    {
        for(int y = 0; y < moduleGrid.Height; y++)
        {
            if(moduleGrid[new Vector3Int(coords.x, y, coords.y)].empty)
            {
                return y;
            }
        }
        return moduleGrid.Height;
    }

    bool IsRoof(ModuleGrid moduleGrid, Module maybeRoof)
    {
        if (maybeRoof.empty)
        {
            var oneDown = maybeRoof.coords - Vector3Int.up;
            return moduleGrid.ValidCoords(oneDown) && !moduleGrid[oneDown].empty;
        }
        return false;
    }
}

public class RooftopArea : Area
{
    List<Module> modules;

    public RooftopArea(IEnumerable<Module> modules, Modules moduleLibrary) : base(moduleLibrary)
    {
        this.modules = modules.ToList();
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        foreach (var module in modules)
        {
            var areaProp = module.GetProperty<AreaModuleProperty>();
            areaProp.Area = this;
            /*
            var m = moduleLibrary.BridgeModule();
            moduleGrid[module.coords] = m;
            m.AddProperty(new AreaModuleProperty(this));*/
        }
        return true;
    }
}

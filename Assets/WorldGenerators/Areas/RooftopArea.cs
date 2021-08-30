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
        foreach (var coords in moduleGrid.Bottom())
        {
            var rooftopHeight = RooftopHeight(moduleGrid, coords);
            if(rooftopHeight > 0 && rooftopHeight < moduleGrid.Height)
            {
                
            }
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
}

public class RooftopArea : Area
{
    Box3Int box;

    public RooftopArea(Box3Int box, Modules moduleLibrary) : base(moduleLibrary)
    {
        this.box = box;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        foreach (var coords in box)
        {
            var emptyModule = moduleLibrary.EmptyModule();
            moduleGrid[coords] = emptyModule;
            emptyModule.AddProperty(new AreaModuleProperty(this));
        }

        return true;
    }
}

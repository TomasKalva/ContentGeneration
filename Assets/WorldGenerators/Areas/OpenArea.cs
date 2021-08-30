using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class OpenArea : Area
{
    Box3Int box;

    public OpenArea(Box3Int box, Modules moduleLibrary) : base(moduleLibrary)
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

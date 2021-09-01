using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Outdoor : Template
{
    Box3Int box;

    public Outdoor(Box3Int box, Modules moduleLibrary, Styles styles) : base(moduleLibrary, styles)
    {
        this.box = box;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        var outdoorArea = new Area(new Designer(), styles.gothic);
        foreach (var coords in box)
        {
            var emptyModule = moduleLibrary.EmptyModule();
            moduleGrid[coords] = emptyModule;
            emptyModule.AddProperty(new AreaModuleProperty(outdoorArea));
        }

        return true;
    }
}

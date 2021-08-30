using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BridgeArea : Area
{
    Module startModule;

    public BridgeArea(Module startModule, Modules moduleLibrary) : base(moduleLibrary)
    {
        this.startModule = startModule;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        if (startModule == null)
        {
            return false;
        }

        var possibleDirections = ExtensionMethods.HorizontalDirections()
            .Where(
            dir =>
            {
                var neighborModule = moduleGrid[startModule.coords - dir];
                return neighborModule != null && !neighborModule.empty;
            }).Shuffle();
        foreach (var direction in possibleDirections)
        {
            int dist = 0;
            var coords = startModule.coords;

            var module = moduleGrid[coords];
            while (module != null && module.empty)
            {
                coords += direction;
                dist += 1;
                module = moduleGrid[coords];
            }
            if (module == null)
            {
                return false;
            }

            if (dist <= 1)
            {
                // no bridge would be placed
                continue;
            }
            else
            {
                for (int i = 0; i < dist; i++)
                {
                    var bridgeCoords = startModule.coords + i * direction;
                    var newBridge = moduleLibrary.BridgeModule();
                    moduleGrid[bridgeCoords] = newBridge;
                    newBridge.AddProperty(new AreaModuleProperty(this));
                }
                return true;
            }
        }
        return false;
    }
}

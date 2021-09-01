using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Bridge : Template
{
    Module startModule;

    public Bridge(Module startModule, Modules moduleLibrary, Styles styles) : base(moduleLibrary, styles)
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
                var underBridgeArea = new Area(new BridgeDesigner(), styles.gothic);
                for (int i = 0; i < dist; i++)
                {
                    var bridgeCoords = startModule.coords + i * direction;
                    var newBridge = moduleLibrary.RoomModule();
                    moduleGrid[bridgeCoords] = newBridge;
                    newBridge.ClearAttachmentPoints();
                    newBridge.SetObject(ObjectType.Bridge);
                    newBridge.AddProperty(new AreaModuleProperty(underBridgeArea));
                }
                return true;
            }
        }
        return false;
    }
}

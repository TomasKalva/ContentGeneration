using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
                return neighborModule != null && neighborModule.GetProperty<TopologyProperty>().HasCeiling(dir);
            }).Shuffle();
        foreach (var direction in possibleDirections)
        {
            int dist = 0;
            var coords = startModule.coords;

            var module = moduleGrid[coords];
            while (module != null && module.Outside && !module.GetProperty<TopologyProperty>().HasCeiling() && !module.HorizontalNeighbors(moduleGrid).Any(neighbor => neighbor.GetObject().objectType == ObjectType.Bridge))
            {
                coords += direction;
                dist += 1;
                module = moduleGrid[coords];
            }
            if (module == null || !module.GetProperty<TopologyProperty>().HasCeiling(-direction))
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
                var underBridgeArea = new Area(new BridgeDesigner(moduleGrid), styles.gothic);
                underBridgeArea.Inside = false;
                var onBridgeArea = new Area(new Designer(), styles.gothic);
                for (int i = 0; i < dist; i++)
                {
                    var bridgeCoords = startModule.coords + i * direction;
                    var newBridge = moduleLibrary.RoomModule(underBridgeArea);
                    moduleGrid[bridgeCoords] = newBridge;
                    newBridge.ClearAttachmentPoints();
                    var bridgeTopology = newBridge.GetProperty<TopologyProperty>();
                    bridgeTopology.SetAllDisconnected();
                    bridgeTopology.SetConnected(direction);
                    bridgeTopology.SetConnected(-direction);
                    bridgeTopology.SetUnreachable(direction);
                    bridgeTopology.SetUnreachable(-direction);
                    newBridge.SetObject(ObjectType.Bridge);
                    newBridge.GetObject().RotateTowards(direction);


                    var onBridge = moduleLibrary.EmptyModule(onBridgeArea);
                    moduleGrid[bridgeCoords + UnityEngine.Vector3Int.up] = onBridge;
                    onBridge.ClearAttachmentPoints();
                }
                return true;
            }
        }
        return false;
    }
}

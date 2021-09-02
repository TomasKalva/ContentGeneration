using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Designer
{
    public virtual void Design(ModuleGrid grid, AreasGraph areasGraph, Module module) { }
}

public class RoomDesigner : Designer
{
    public override void Design(ModuleGrid grid, AreasGraph areasGraph, Module module)
    {
        var area = module.GetProperty<AreaModuleProperty>().Area;
        foreach (var dirObj in module.HorizontalDirectionObjects())
        {
            var direction = dirObj.direction;
            var otherModule = grid[module.coords + direction];
            if (otherModule == null)
            {
                continue;
            }

            // Connect to the same area
            if (area.ContainsModule(otherModule))
            {
                module.SetDirection(direction, ObjectType.Empty);
            }

            // Don't connect to outside
            var otherArea = otherModule.GetProperty<AreaModuleProperty>().Area;
            if (otherArea.Name == "Outside")
            {
                module.SetDirection(direction, GetObjectType(module));
                continue;
            }

            // Try to connect the areas if possible
            var myArea = module.GetProperty<AreaModuleProperty>().Area;
            if (!areasGraph.AreConnected(myArea, otherArea))
            {
                if (otherModule.ReachableFrom(direction))
                {
                    module.SetDirection(direction, ObjectType.Door);
                    module.SetDirection(-direction, ObjectType.Empty);
                    areasGraph.Connect(myArea, otherArea);
                }
            }
        }
    }

    public ObjectType GetObjectType(Module module)
    {
        return module.coords.Sum(t => t) % 2 == 0 ? ObjectType.Wall : ObjectType.Window;
    }
}

public class BridgeDesigner : Designer
{
    public override void Design(ModuleGrid grid, AreasGraph areasGraph, Module module)
    {
        foreach (var neighbor in module.HorizontalNeighbors(grid))
        {
            var neighborObject = neighbor.GetObject();
            if (neighborObject != null && neighborObject.objectType == ObjectType.Bridge)
            {
                var dir = module.DirectionTo(neighbor);
                module.GetAttachmentPoint(Vector3Int.up).RotateTowards(dir);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Designer
{
    public virtual void Design(ModuleGrid grid, AreasGraph areasGraph, Module module) { }

    public virtual bool Satisfied() => true;
}

public class RoomDesigner : Designer
{
    Func<Module, AreasGraph, Dictionary<DirectionObject, List<Rule>>> moduleRules;

    public RoomDesigner(ModuleGrid grid)
    {
        moduleRules = (module, areasGraph) =>
        {
            var rules = new Dictionary<DirectionObject, List<Rule>>();
            foreach (var dirObj in module.HorizontalDirectionObjects())
            {
                var direction = dirObj.direction;
                var otherModule = grid[module.coords + direction];
                var otherArea = otherModule?.GetProperty<AreaModuleProperty>().Area;
                var area = module.GetProperty<AreaModuleProperty>().Area;

                // Connect to the same area
                var connectSame = new Rule(
                    () => area.ContainsModule(otherModule),
                    () => module.SetDirection(direction, ObjectType.Empty)
                    );

                // Don't connect to outside
                var dontConnectOutside = new Rule(
                    () => otherArea == null || otherArea.Name == "Outside",
                    () => module.SetDirection(direction, GetObjectType(module))
                    );

                // Try to connect the areas if possible
                var connectAreas = new Rule(
                    () => !areasGraph.AreConnected(area, otherArea) && otherModule != null && otherModule.ReachableFrom(direction),
                    () =>
                    {
                        module.SetDirection(direction, ObjectType.Door);
                        otherModule.SetDirection(-direction, ObjectType.Empty);
                        areasGraph.Connect(area, otherArea);
                    }
                    );

                rules.Add(dirObj, new List<Rule>() { connectSame, dontConnectOutside, connectAreas });
            }
            return rules;
        };
    }

    public override void Design(ModuleGrid grid, AreasGraph areasGraph, Module module)
    {
        foreach (var dirObj in module.HorizontalDirectionObjects())
        {
            var directionRules = moduleRules(module, areasGraph)[dirObj];
            var bestRule = directionRules.Where(rule => rule.Condition()).GetRandom();
            bestRule?.Effect();
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

public delegate bool RuleCondition();
public delegate void RuleEffect();

public class Rule
{
    RuleCondition condition;
    RuleEffect effect;

    public Rule(RuleCondition condition, RuleEffect effect)
    {
        this.condition = condition;
        this.effect = effect;
    }

    public bool Condition() => condition();
    public void Effect() => effect();

}
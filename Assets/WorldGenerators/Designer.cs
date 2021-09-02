using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Designer
{
    protected Func<Module, AreasGraph, List<List<Rule>>> moduleRuleClasses;
    protected Dictionary<Module, List<Rule>> usedRules;

    public Designer()
    {
        usedRules = new Dictionary<Module, List<Rule>>();
        moduleRuleClasses = (module, areasGraph) => new List<List<Rule>>();
    }

    public void Design(ModuleGrid grid, AreasGraph areasGraph, Module module)
    {
        usedRules.TryAdd(module, new List<Rule>());
        foreach(var ruleClass in moduleRuleClasses(module, areasGraph))
        {
            var bestRule = ruleClass.Where(rule => rule.Condition()).GetRandom();
            if (bestRule != null)
            {
                bestRule.Effect();
                usedRules[module].Add(bestRule);
            }
        }
    }

    public bool Satisfied(Module module)
    {
        usedRules.TryGetValue(module, out var moduleRules);
        return moduleRules == null ? false : moduleRules.All(rule => rule.Condition());
    }
}

public class RoomDesigner : Designer
{
    public RoomDesigner(ModuleGrid grid)
    {
        moduleRuleClasses = (module, areasGraph) =>
        {
            var ruleClasses = new List<List<Rule>>();
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

                ruleClasses.Add(new List<Rule>() { connectSame, dontConnectOutside, connectAreas });
            }
            return ruleClasses;
        };
    }

    /*public override void Design(ModuleGrid grid, AreasGraph areasGraph, Module module)
    {
        usedRules.TryAdd(module, new List<Rule>());
        foreach (var dirObj in module.HorizontalDirectionObjects())
        {
            var directionRules = moduleRules(module, areasGraph)[dirObj];
            var bestRule = directionRules.Where(rule => rule.Condition()).GetRandom();
            if (bestRule != null)
            {
                bestRule.Effect();
                usedRules[module].Add(bestRule);
            }
        }
    }

    public override bool Satisfied(Module module)
    {
        usedRules.TryGetValue(module, out var moduleRules);
        return moduleRules == null ? false : moduleRules.All(rule => rule.Condition());
    }*/

    public ObjectType GetObjectType(Module module)
    {
        return module.coords.Sum(t => t) % 2 == 0 ? ObjectType.Wall : ObjectType.Window;
    }
}

public class BridgeDesigner : Designer
{
    public BridgeDesigner(ModuleGrid grid)
    {
        moduleRuleClasses = (module, areasGraph) =>
        {
            var ruleClasses = new List<List<Rule>>();
            var bridgeDirectionRules = new List<Rule>();
            foreach (var neighbor in module.HorizontalNeighbors(grid))
            {
                var neighborObject = neighbor.GetObject();
                var dir = module.DirectionTo(neighbor);

                var rule = new Rule(
                    () => neighborObject != null && neighborObject.objectType == ObjectType.Bridge,
                    () => module.GetAttachmentPoint(Vector3Int.up).RotateTowards(dir)
                    );
                bridgeDirectionRules.Add(rule);
            }
            ruleClasses.Add(bridgeDirectionRules);
            return ruleClasses;
        };
    }

    /*public override void Design(ModuleGrid grid, AreasGraph areasGraph, Module module)
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
    }*/
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

public class DesignerSatisfier
{
    public void SatisfyDesigners(ModuleGrid moduleGrid, AreasGraph areasGraph)
    {
        var toSatisfy = new Queue<Module>(moduleGrid);
        var maxIteration = toSatisfy.Count() * 8;
        var i = 0;
        while (toSatisfy.Any() && i++ < maxIteration)
        {
            var currModule = toSatisfy.Dequeue();
            var designer = currModule.GetProperty<AreaModuleProperty>().Area.Designer;
            if (!designer.Satisfied(currModule))
            {
                designer.Design(moduleGrid, areasGraph, currModule);
                foreach(var neighbor in currModule.HorizontalNeighbors(moduleGrid))
                {
                    toSatisfy.Enqueue(neighbor);
                }
            }
        }
    }
}
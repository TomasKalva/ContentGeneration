using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Designer
{
    protected Func<Module, AreasGraph, List<RulesClass>> moduleRuleClasses;
    protected Dictionary<Module, List<Rule>> usedRules;

    public IEnumerable<Rule> UsedRules(Module module)
    {
        if(usedRules.TryGetValue(module, out var rules))
        {
            return rules;
        }
        else
        {
            return Enumerable.Empty<Rule>();
        }
    }

    public Designer()
    {
        usedRules = new Dictionary<Module, List<Rule>>();
        moduleRuleClasses = (module, areasGraph) => new List<RulesClass>();
    }

    public void Design(ModuleGrid grid, AreasGraph areasGraph, Module module)
    {
        usedRules.TryAdd(module, new List<Rule>());
        foreach(var ruleClass in moduleRuleClasses(module, areasGraph))
        {
            var bestRule = ruleClass.BestSatisfyingRule();
            if (bestRule != null)
            {
                //bestRule.Effect();
                usedRules[module].Add(bestRule);
            }
        }
    }

    public bool Satisfied(Module module)
    {
        usedRules.TryGetValue(module, out var moduleRules);
        if (moduleRules == null) return false;

        foreach (var currentBest in moduleRules)
        {
            Debug.Log(currentBest.Priority);
            var best = currentBest.RulesClass.BestSatisfyingRule();
            if(best != null && (!currentBest.Condition() || best.Priority > currentBest.Priority))
            {
                return false;
            }
        }
        return true;
    }
}

public class RoomDesigner : Designer
{
    public RoomDesigner(ModuleGrid grid)
    {
        moduleRuleClasses = (module, areasGraph) =>
        {
            var ruleClasses = new List<RulesClass>();
            foreach (var dirObj in module.HorizontalDirectionObjects())
            {
                var direction = dirObj.direction;
                var otherModule = grid[module.coords + direction];
                var otherArea = otherModule?.GetProperty<AreaModuleProperty>().Area;
                var area = module.GetProperty<AreaModuleProperty>().Area;

                // Connect to the same area
                var connectSame = new Rule(
                    "Connect same area",
                    () => area.ContainsModule(otherModule) && otherModule != null && otherModule.HasFloor(grid),
                    () => module.SetDirection(direction, ObjectType.Empty)
                    );

                // Place railing
                var placeRailing = new Rule(
                    "Place railing",
                    () => /*area == otherArea &&*/ otherModule != null && module.HasFloor(grid) && !otherModule.HasFloor(grid),
                    () => module.SetDirection(direction, ObjectType.Railing)
                    );

                // Don't connect to outside
                var dontConnectOutside = new Rule(
                    "Don't connect outside",
                    () => otherArea == null || otherArea.Name == "Outside",
                    () => module.SetDirection(direction, GetObjectType(module))
                    );

                // Try to connect the areas if possible
                var connectAreas = new Rule(
                    "Connect areas",
                    () => !areasGraph.AreConnected(area, otherArea) && otherModule != null && otherModule.ReachableFrom(direction),
                    () =>
                    {
                        module.SetDirection(direction, ObjectType.Door);
                        otherModule.SetDirection(-direction, ObjectType.Empty);
                        areasGraph.Connect(area, otherArea);
                    }
                    );

                // Remove objects in all horiznotal directions when no floor
                var noFloor = new Rule(
                    "Remove objects when no floor",
                    () => !module.HasFloor(grid),
                    () => module.HorizontalDirectionObjects().ForEach(dirObj => module.SetDirection(dirObj.direction, ObjectType.Empty))
                    );

                // Add walls when on the very bottom
                var wallOnBottom = new Rule(
                    "Add walls on bottom",
                    () => grid[module.coords - Vector3Int.up] == null,
                    () => module.HorizontalDirectionObjects().ForEach(dirObj => module.SetDirection(dirObj.direction, ObjectType.Wall))
                    );

                var rules = new RulesClass(dirObj.direction.Name());
                rules.AddRule(wallOnBottom);
                rules.AddRule(noFloor);
                rules.AddRule(connectSame);
                rules.AddRule(dontConnectOutside);
                rules.AddRule(placeRailing);
                rules.AddRule(connectAreas);

                ruleClasses.Add(rules);
            }
            return ruleClasses;
        };
    }

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
            // direction of bridge
            var ruleClasses = new List<RulesClass>();
            var bridgeDirectionRules = new RulesClass("Bridge direction");
            foreach (var neighbor in module.HorizontalNeighbors(grid))
            {
                var neighborObject = neighbor.GetObject();
                var dir = module.DirectionTo(neighbor);

                var rule = new Rule(
                    dir.Name(),
                    () => neighborObject != null && neighborObject.objectType == ObjectType.Bridge,
                    () => module.GetAttachmentPoint(Vector3Int.up).RotateTowards(dir)
                    );
                bridgeDirectionRules.AddRule(rule);
            }
            ruleClasses.Add(bridgeDirectionRules);

            // no obstacle at the end of the bridge
            /*var noObstacleRules = new RulesClass("No obstacles");

            var noObstacleRule = new Rule(
                "No obstacles rule",
                () => true,
                () => module.GetAttachmentPoint(Vector3Int.up).RotateTowards(dir)
                );
            bridgeDirectionRules.AddRule(noObstacleRule);*/

            return ruleClasses;
        };
    }
}

public delegate bool RuleCondition();
public delegate void RuleEffect();

public class Rule
{
    RuleCondition condition;
    RuleEffect effect;
    public string Name { get; }

    public int Priority { get; set; }

    public RulesClass RulesClass { get; set; }

    public Rule(string name, RuleCondition condition, RuleEffect effect)
    {
        this.condition = condition;
        this.effect = effect;
        this.Name = name;
    }

    public bool Condition() => condition();
    public void Effect() => effect();

}

public class RulesClass
{
    public List<Rule> RulesList { get;}
    int currentPriority;

    Rule DefaultRule { get; }

    public string Name { get; }

    public RulesClass(string name)
    {
        RulesList = new List<Rule>();
        currentPriority = 0;
        this.Name = name;

        // Default rule is selected when no other rule is applicable
        DefaultRule = new Rule(
            "Default",
            () => false,
            () => { }
            );
        DefaultRule.RulesClass = this;
        DefaultRule.Priority = -1000_000;
    }

    public void AddRule(Rule rule)
    {
        rule.RulesClass = this;
        // rules specified later have lower priority
        rule.Priority = currentPriority--;

        RulesList.Add(rule);
    }

    public Rule BestSatisfyingRule()
    {
        var bestRule = RulesList.Where(rule => rule.Condition()).ArgMax(rule => rule.Priority);
        return bestRule != null ? bestRule : DefaultRule;
    }
}

public class DesignerSatisfier
{
    public void SatisfyDesigners(ModuleGrid moduleGrid, AreasGraph areasGraph)
    {
        var toSatisfy = new Queue<Module>(moduleGrid);
        var maxIteration = toSatisfy.Count() * 2;
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

        foreach(var module in moduleGrid)
        {
            var designer = module.GetProperty<AreaModuleProperty>().Area.Designer;
            foreach (var usedRule in designer.UsedRules(module))
            {
                usedRule.Effect();
            }
        }
    }
}

/*public class RulesLibrary
{
    public static RulesLibrary RulesLib { get; }

    static RulesLibrary()
    {
        RulesLib = new RulesLibrary();
    }


}*/
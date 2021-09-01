using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AreasGraph : IGraph<Area, AreasConnection>
{
    public List<Area> Vertices { get; }
    public List<AreasConnection> Edges { get; }

    public AreasGraph()
    {
        Vertices = new List<Area>();
        Edges = new List<AreasConnection>();
    }

    public void AddArea(Area area)
    {
        Vertices.Add(area);
    }

    public void Connect(Area from, Area to)
    {
        Edges.Add(new AreasConnection(from, to));
    }

    public bool AreConnected(Area from, Area to)
    {
        return Edges.Any(edge => edge.Connects(from, to));
    }

    public IEnumerable<Area> Neighbors(Area vert)
    {
        return Edges.Where(edge => edge.Contains(vert)).Select(edge => edge.Other(vert));
    }
}

public class Area
{
    public string Name { get; set; }

    List<Module> modules;

    public Style Style { get; }

    public Designer Designer { get; }

    public Area(Style style)
    {
        modules = new List<Module>();
        Style = style;
        Designer = new Designer();
    }

    public void AddModule(Module module)
    {
        modules.Add(module);
    }

    public void RemoveModule(Module module)
    {
        modules.Remove(module);
    }

    public bool ContainsModule(Module module) => modules.Where(m => m == module).Any();
}

public struct AreasConnection : Edge<Area>
{
    public Area From { get; }
    public Area To { get; }

    public AreasConnection(Area from, Area to)
    {
        From = from;
        To = to;
    }

    public bool Connects(Area from, Area to)
    {
        return (From == from && To == to) || (From == to && To == from);
    }

    public bool Contains(Area vert)
    {
        return vert == From || vert == To;
    }

    public Area Other(Area vert)
    {
        return vert == From ? To : From;
    }
}

public class Designer
{
    public void Design(ModuleGrid grid, AreasGraph areasGraph, Module module)
    {
        var area = module.GetProperty<AreaModuleProperty>().Area;
        foreach (var dirObj in module.directionBlockers)
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
                    module.SetDirection(direction, ObjectType.Empty);
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Area
{
    protected Modules moduleLibrary;

    List<Module> modules;

    public string Name { get; set; }

    protected Area(Modules moduleLibrary)
    {
        this.moduleLibrary = moduleLibrary;
        modules = new List<Module>();
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

    public abstract bool Generate(ModuleGrid moduleGrid);
}
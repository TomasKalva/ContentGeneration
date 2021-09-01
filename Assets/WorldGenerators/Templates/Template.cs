using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Template
{
    protected Modules moduleLibrary;

    protected Template(Modules moduleLibrary)
    {
        this.moduleLibrary = moduleLibrary;
    }

    public abstract bool Generate(ModuleGrid moduleGrid);
}
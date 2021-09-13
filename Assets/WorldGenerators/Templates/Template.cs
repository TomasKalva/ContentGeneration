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

    protected Styles styles;

    public Style Style { get; set; }

    protected Template(Modules moduleLibrary, Styles styles)
    {
        this.moduleLibrary = moduleLibrary;
        this.styles = styles;
        Style = styles.gothic;
    }

    public abstract bool Generate(ModuleGrid moduleGrid);
}
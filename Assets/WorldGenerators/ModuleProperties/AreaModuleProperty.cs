using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AreaModuleProperty : IModuleProperty
{
    public Area Area { get; set; }

    public AreaModuleProperty(Area area)
    {
        Area = area;
    }

    public void OnAdded(Module module)
    {
        Area.AddModule(module);
    }

    public void OnModuleDestroyed(Module module)
    {
        Area.RemoveModule(module);
    }
}
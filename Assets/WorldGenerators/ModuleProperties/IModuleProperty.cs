using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IModuleProperty
{
    void OnAdded(Module module);
    void OnModuleDestroyed(Module module);

}
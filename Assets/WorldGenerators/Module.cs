using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Module : MonoBehaviour
{
    public Vector3Int coords;

    public bool empty;

    public void Init(Vector3Int coords)
    {
        this.coords = coords;
    }

    public virtual void AfterGenerated(ModuleGrid grid)
    {

    }

    public Vector3Int DirectionTo(Module module)
    {
        return (module.coords - coords).ComponentWise(t => t > 0 ? 1 : t == 0 ? 0 : -1);
    }

    public IEnumerable<Module> HorizontalNeighbors(ModuleGrid grid)
    {
        return ExtensionMethods.HorizontalDirections().Select(dir => grid[coords + dir]).Where(m => m != null);
    }
}

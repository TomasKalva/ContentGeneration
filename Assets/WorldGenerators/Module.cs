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

    public virtual bool ReachableFrom(GridDirection dir)
    {
        return true;
    }
}

public struct GridDirection
{
    Vector3Int _direction;

    GridDirection(Vector3Int direction)
    {
        this._direction = direction;
    }

    public static GridDirection Forward { get; }
    public static GridDirection Right { get; }
    public static GridDirection Up { get; }
    public static GridDirection ForwardUp { get; }
    public static GridDirection ForwardDown { get; }
    public static GridDirection RightUp { get; }
    public static GridDirection RightDown { get; }
    public static GridDirection ForwardRight { get; }
    public static GridDirection ForwardLeft { get; }

    static GridDirection()
    {
        Forward = new GridDirection(Vector3Int.forward);
        Right = new GridDirection(Vector3Int.right);
        Up = new GridDirection(Vector3Int.up);
        ForwardUp = new GridDirection(Vector3Int.forward + Vector3Int.up);
        ForwardDown = new GridDirection(Vector3Int.forward - Vector3Int.up);
        RightUp = new GridDirection(Vector3Int.right + Vector3Int.up);
        RightDown = new GridDirection(Vector3Int.right - Vector3Int.up);
        ForwardRight = new GridDirection(Vector3Int.forward + Vector3Int.right);
        ForwardLeft = new GridDirection(Vector3Int.forward - Vector3Int.right);
    }

    public static GridDirection operator -(GridDirection d) => new GridDirection(-d._direction);

    public static implicit operator Vector3Int(GridDirection d) => d._direction;
}

interface IModuleGeometry
{

}

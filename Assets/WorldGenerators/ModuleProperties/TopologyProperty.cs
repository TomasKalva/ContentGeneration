using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

interface IModuleTopology : IModuleProperty
{
    bool ReachableFrom(Vector3Int dir);

    bool HasCeiling();

    bool HasFloor(ModuleGrid grid);

    bool HasCeiling(Vector3Int direction);

    bool HasFloor(ModuleGrid grid, Vector3Int direction);
}

public class TopologyProperty : IModuleTopology
{
    /// <summary>
    /// Directions not blocked by walls.
    /// </summary>
    public List<Vector3Int> ReachableDirections { get; }
    /// <summary>
    /// Directions connected by the ceiling.
    /// </summary>
    public List<Vector3Int> ConnectedDirections { get; }

    public bool ConnectsUp { get; set; }

    Module module;

    public TopologyProperty()
    {
        ReachableDirections = new List<Vector3Int>();
        ConnectedDirections = new List<Vector3Int>();
        ConnectsUp = false;
    }

    public void OnAdded(Module module)
    {
        this.module = module;
    }

    public void OnModuleDestroyed(Module module)
    {
    }

    public void SetAllReachable()
    {
        foreach (var dir in ExtensionMethods.HorizontalDirections())
        {
            ReachableDirections.Add(dir);
        }
    }

    public void SetAllConnected()
    {
        foreach (var dir in ExtensionMethods.HorizontalDirections())
        {
            ConnectedDirections.Add(dir);
        }
    }

    public void SetAllDisconnected()
    {
        ConnectedDirections.Clear();
    }

    public void SetReachable(Vector3Int dir)
    {
        ReachableDirections.Add(dir);
    }

    public void SetUnreachable(Vector3Int dir)
    {
        ReachableDirections.Remove(dir);
    }

    public void SetConnected(Vector3Int dir)
    {
        ConnectedDirections.Add(dir);
    }

    public void SetDisconnected(Vector3Int dir)
    {
        ConnectedDirections.Remove(dir);
    }

    public bool ReachableFrom(Vector3Int dir)
    {
        return ReachableDirections.Contains(-dir);
    }

    public bool Reachable(Vector3Int dir)
    {
        return ReachableDirections.Contains(dir);
    }

    public bool Connected(Vector3Int dir)
    {
        return ConnectedDirections.Contains(dir);
    }

    public bool HasCeiling()
    {
        return ExtensionMethods.HorizontalDirections().All(dir => ConnectedDirections.Contains(dir));

    }

    public bool HasFloor(ModuleGrid grid)
    {
        var bottomCoords = module.coords - Vector3Int.up;
        if (grid.ValidCoords(bottomCoords))
        {
            var bottomModuleTopology = grid[bottomCoords].GetProperty<TopologyProperty>();
            return bottomModuleTopology.HasCeiling();
        }
        return false;
    }

    public bool HasCeiling(Vector3Int direction)
    {
        return ConnectedDirections.Contains(direction);
    }

    public bool HasFloor(ModuleGrid grid, Vector3Int direction)
    {
        var bottomCoords = module.coords - Vector3Int.up;
        if (grid.ValidCoords(bottomCoords))
        {
            var bottomModuleTopology = grid[bottomCoords].GetProperty<TopologyProperty>();
            return bottomModuleTopology.HasCeiling(direction);
        }
        return false;
    }

    public bool Empty => !ConnectsUp && !HasCeiling() && ReachableDirections.Count() == 4;
}

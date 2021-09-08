using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ModuleGrid : IEnumerable<Module>
{
    Transform parent;

    Vector3Int sizes;

    Vector3 extents;

    Module[,,] moduleGrid;

    public int Width => sizes.x;
    public int Height => sizes.y;
    public int Depth => sizes.z;

    public bool ValidCoords(Vector3Int coords) => coords.AtLeast(Vector3Int.zero) && coords.Less(sizes);

    public Vector3Int Sizes => sizes;

    public Module this[int x, int y, int z]
    {
        get => GetModule(new Vector3Int(x, y, z));
        set => SetModule(new Vector3Int(x, y, z), value);
    }

    public Module this[Vector3Int coords]
    {
        get => GetModule(coords);
        set => SetModule(coords, value);
    }

    Module GetModule(Vector3Int coords)
    {
        return ValidCoords(coords) ? moduleGrid[coords.x, coords.y, coords.z] : null;
    }

    void SetModule(Vector3Int coords, Module module)
    {
        if (ValidCoords(coords))
        {
            var originalModule = this[coords];
            if (originalModule != null)
            {
                originalModule.OnDestroyed();
                GameObject.DestroyImmediate(originalModule.gameObject);
            }

            module.transform.SetParent(parent);
            module.transform.position = Vector3.Scale(coords, extents);
            module.coords = coords;
            moduleGrid[coords.x, coords.y, coords.z] = module;
        }
    }

    public ModuleGrid(Vector3Int sizes, Vector3 extents, Transform parent)
    {
        moduleGrid = new Module[sizes.x, sizes.y, sizes.z];
        this.sizes = sizes;
        this.extents = extents;
        this.parent = parent;
    }

    public Box2Int Bottom()
    {
        return new Box2Int(Vector2Int.zero, sizes.XZ());
    }

    public bool ContainsBuilding(Module module)
    {
        return module != null && !module.Empty;
    }

    public bool HasHorizontalNeighbor(Module module)
    {
        return ContainsBuilding(GetModule(module.coords + Vector3Int.right)) ||
                ContainsBuilding(GetModule(module.coords - Vector3Int.right)) ||
                ContainsBuilding(GetModule(module.coords + Vector3Int.forward)) ||
                ContainsBuilding(GetModule(module.coords - Vector3Int.forward));
    }

    public bool IsEmpty(Module module)
    {
        return module.Empty;
    }

    public IEnumerator<Module> GetEnumerator()
    {
        foreach (var module in moduleGrid)
        {
            yield return module;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Vector3Int WorldToGrid(Vector3 coords)
    {
        Vector3 localCoords = (parent.worldToLocalMatrix * coords);
        return (localCoords + 0.5f * extents).Divide(extents).ToVector3Int();
    }
}

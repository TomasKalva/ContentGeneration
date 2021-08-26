using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GridWorldGenerator : WorldGenerator
{
    [SerializeField]
    Module bridge;

    [SerializeField]
    Module stairs;

    [SerializeField]
    Module room;

    [SerializeField]
    Module empty;

    [SerializeField]
    Transform parent;

    [SerializeField]
    Vector3Int sizes;

    [SerializeField]
    Vector3 extents;

    [SerializeField]
    int buildingsCount;

    [SerializeField]
    int bridgesCount;

    ModuleGrid moduleGrid;

    public override void Generate(World world)
    {
        moduleGrid = new ModuleGrid(empty, sizes, extents, parent);

        AddBuildings(buildingsCount);

        AddBridges(bridgesCount);

        foreach(var module in moduleGrid)
        {
            module.AfterGenerated(moduleGrid);
        }
    }

    void AddBuildings(int n)
    {
        for (int i = 0; i < n; i++)
        {
            AddBuilding();
        }
    }

    void GetExtents(int M, int m, out int a, out int b)
    {
        int size = UnityEngine.Random.Range(1, m + 1);
        a = UnityEngine.Random.Range(0, M - m);
        b = a + size;
    }

    void AddBuilding()
    {
        GetExtents(moduleGrid.Width, 6, out var minX, out var maxX);
        GetExtents(moduleGrid.Depth, 4, out var minZ, out var maxZ);
        int minY = 0;
        int maxY = UnityEngine.Random.Range(1, 5);
        for(int i=minX; i < maxX; i++)
        {
            for(int j = minY; j < maxY; j++)
            {
                for(int k = minZ; k < maxZ; k++)
                {
                    var coords = new Vector3Int(i, j, k);
                    if (!moduleGrid.IsEmpty(moduleGrid[coords]))
                    {
                        continue;
                    }

                    var module = Instantiate(room);
                    moduleGrid[i, j, k] = module;
                }
            }
        }
    }

    Module RandomModule()
    {
        int x = UnityEngine.Random.Range(0, moduleGrid.Width);
        int y = UnityEngine.Random.Range(0, moduleGrid.Height);
        int z = UnityEngine.Random.Range(0, moduleGrid.Depth);
        return moduleGrid[x, y, z];
    }

    Module SatisfyingModule(System.Func<Module, bool> condition)
    {
        for(int i=0; i < 10; i++)
        {
            var randModule = RandomModule();
            if (condition(randModule))
            {
                return randModule;
            }
        }
        return null;
    }

    void AddBridges(int n)
    {
        int bridgesCount = 0;
        while(bridgesCount < n)
        {
            bridgesCount += AddBridge() ? 1 : 0;
        }
    }

    bool ContainsBuilding(Module module)
    {
        return module != null && !module.empty;
    }

    bool HasHorizontalNeighbor(Module module)
    {
        return ContainsBuilding(moduleGrid[module.coords + Vector3Int.right]) ||
                ContainsBuilding(moduleGrid[module.coords - Vector3Int.right]) ||
                ContainsBuilding(moduleGrid[module.coords + Vector3Int.forward]) ||
                ContainsBuilding(moduleGrid[module.coords - Vector3Int.forward]);
    }

    bool IsEmpty(Module module)
    {
        return module.empty;
    }

    /// <summary>
    /// Returns true if adding bridge was successfull.
    /// </summary>
    bool AddBridge()
    {
        var startModule = SatisfyingModule(module => IsEmpty(module) && HasHorizontalNeighbor(module));
        if(startModule == null)
        {
            return false;
        }

        int dist = 0;
        var coords = startModule.coords;
        var possibleDirections = ExtensionMethods.HorizontalDirections()
            .Where(
            dir =>
            {
                var neighborModule = moduleGrid[coords - dir];
                return neighborModule != null && !neighborModule.empty;
            }).Shuffle();
        foreach (var direction in possibleDirections)
        {
            var module = moduleGrid[coords];
            while (module != null && IsEmpty(module))
            {
                coords += direction;
                dist += 1;
                module = moduleGrid[coords];
            }
            if(module == null)
            {
                return false;
            }

            if (dist <= 1)
            {
                // no bridge would be placed
                continue;
            }
            else
            {
                for (int i = 0; i < dist; i++)
                {
                    var bridgeCoords = startModule.coords + i * direction;
                    var newBridge = Instantiate(bridge);
                    moduleGrid[bridgeCoords] = newBridge;
                }
                return true;
            }
        }
        return false;
    }

    public void DestroyAllChildren()
    {
        for (int i = parent.childCount; i > 0; --i)
        {
            GameObject.DestroyImmediate(parent.GetChild(0).gameObject);
        }
    }
}

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
            module.transform.SetParent(parent);
            module.transform.position = Vector3.Scale(coords, extents);
            module.Init(coords);
            moduleGrid[coords.x, coords.y, coords.z] = module;
        }
    }

    public ModuleGrid(Module empty, Vector3Int sizes, Vector3 extents, Transform parent)
    {
        moduleGrid = new Module[sizes.x, sizes.y, sizes.z];
        this.sizes = sizes;
        this.extents = extents;
        this.parent = parent;
        InitGrid(empty);
    }

    public void InitGrid(Module empty)
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int k = 0; k < Depth; k++)
                {
                    var module = GameObject.Instantiate(empty);
                    SetModule(new Vector3Int(i, j, k), module);
                    module.transform.SetParent(parent);
                    module.transform.position = Vector3.Scale(new Vector3(i, j, k), extents);
                }
            }
        }
    }

    public bool ContainsBuilding(Module module)
    {
        return module != null && !module.empty;
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
        return module.empty;
    }

    public IEnumerator<Module> GetEnumerator()
    {
        foreach(var module in moduleGrid)
        {
            yield return module;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

[CustomEditor(typeof(GridWorldGenerator))]
public class GridWorldGeneratorOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        if (GUILayout.Button("Generate", GUILayout.Width(100), GUILayout.Height(60)))
        {
            var generator = (GridWorldGenerator)target;

            generator.DestroyAllChildren();
            generator.Generate(null);
        }
        if (GUILayout.Button("Delete", GUILayout.Width(60), GUILayout.Height(30)))
        {
            var generator = (GridWorldGenerator)target;

            generator.DestroyAllChildren();
        }
        GUILayout.Label("generate");
    }
}
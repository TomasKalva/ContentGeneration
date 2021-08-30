using Assets;
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
    Modules modules;

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

    AreasGraph areasGraph;

    public override void Generate(World world)
    {
        moduleGrid = new ModuleGrid(sizes, extents, parent);
        areasGraph = new AreasGraph();

        InitGrid();

        AddBuildings(buildingsCount);

        AddBridges(bridgesCount);

        foreach(var module in moduleGrid)
        {
            module.AfterGenerated(moduleGrid, areasGraph);
        }
    }

    void InitGrid()
    {
        var openArea = new OpenArea(new Box3Int(Vector3Int.zero, sizes), modules);
        openArea.Generate(moduleGrid);
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
        var building = new Building(new Vector3Int(minX, minY, minZ), new Vector3Int(maxX, maxY, maxZ), modules);
        building.Generate(moduleGrid);
    }

    Module RandomModule()
    {
        int x = UnityEngine.Random.Range(0, moduleGrid.Width);
        int y = UnityEngine.Random.Range(0, moduleGrid.Height);
        int z = UnityEngine.Random.Range(0, moduleGrid.Depth);
        return moduleGrid[x, y, z];
    }

    public Module SatisfyingModule(System.Func<Module, bool> condition)
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

    public bool HasHorizontalNeighbor(Module module)
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
        var startModule = SatisfyingModule(module => module.empty && HasHorizontalNeighbor(module));
        var bridge = new BridgeArea(startModule, modules);
        return bridge.Generate(moduleGrid);
        /*
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
                    var newBridge = modules.BridgeModule();
                    moduleGrid[bridgeCoords] = newBridge;
                }
                return true;
            }
        }*/
    }



    public void DestroyAllChildren()
    {
        for (int i = parent.childCount; i > 0; --i)
        {
            GameObject.DestroyImmediate(parent.GetChild(0).gameObject);
        }
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
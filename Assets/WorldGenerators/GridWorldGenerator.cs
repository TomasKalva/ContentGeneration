using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GridWorldGenerator : WorldGenerator
{
    [SerializeField]
    Transform parent;

    [SerializeField]
    Module bridge;

    [SerializeField]
    Module stairs;

    [SerializeField]
    Module room;

    [SerializeField]
    Module empty;

    [SerializeField]
    int depth;

    [SerializeField]
    Vector3Int sizes;

    [SerializeField]
    Vector3 extents;

    [SerializeField]
    int buildingsCount;

    Module[,,] moduleGrid;

    int Width => sizes.x;
    int Height => sizes.y;
    int Depth => sizes.z;

    bool ValidCoords(Vector3Int coords) => !coords.Less(Vector3Int.zero) && coords.Less(sizes);

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

    public override void Generate(World world)
    {
        // Destroy old grid
        /*UnityEditor.EditorApplication.delayCall += () =>
        {
            foreach (Transform child in parent)
            {
                DestroyImmediate(child.gameObject);
            }
        };*/

        moduleGrid = new Module[sizes.x, sizes.y, sizes.z];
        Debug.Log("Generating grid world");


        InitGrid();

        AddBuildings(buildingsCount);

        AddBridges();
    }

    void InitGrid()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int k = 0; k < Depth; k++)
                {
                    var module = Instantiate(empty);
                    SetModule(new Vector3Int(i, j, k), module);
                    module.transform.SetParent(parent);
                    module.transform.position = Vector3.Scale(new Vector3(i, j, k), extents);
                }
            }
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
        int size = Random.Range(1, m + 1);
        a = Random.Range(0, M - m);
        b = a + size;
    }

    void AddBuilding()
    {
        GetExtents(sizes.x, 4, out var minX, out var maxX);
        GetExtents(sizes.z, 6, out var minZ, out var maxZ);
        int minY = 0;
        int maxY = Random.Range(1, 5);
        for(int i=minX; i < maxX; i++)
        {
            for(int j = minY; j < maxY; j++)
            {
                for(int k = minZ; k < maxZ; k++)
                {
                    if(!IsEmpty(moduleGrid[i, j, k]))
                    {
                        continue;
                    }

                    var module = Instantiate(room);
                    SetModule(new Vector3Int(i, j, k), module);
                }
            }
        }
    }

    Module RandomModule()
    {
        int x = Random.Range(0, Width);
        int y = Random.Range(0, Height);
        int z = Random.Range(0, Depth);
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



    void AddBridges()
    {
        for (int i = 0; i < 10; i++)
        {
            AddBridge();
        }
    }

    bool HasHorizontalNeighbor(Module module)
    {
        return GetModule(module.coords + Vector3Int.right) != null ||
                GetModule(module.coords - Vector3Int.right) != null ||
                GetModule(module.coords + Vector3Int.forward) != null ||
                GetModule(module.coords - Vector3Int.forward) != null;
    }

    bool IsEmpty(Module module)
    {
        return module.empty;
    }

    void AddBridge()
    {
        var startModule = SatisfyingModule(module => IsEmpty(module) && HasHorizontalNeighbor(module));
        if(startModule == null)
        {
            return;
        }

        int dist = 0;
        var coords = startModule.coords;
        foreach (var direction in ExtensionMethods.HorizontalDirections())
        {
            var module = GetModule(coords);
            while (module != null && IsEmpty(module))
            {
                coords += direction;
                dist += 1;
                module = GetModule(coords);
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
                    SetModule(bridgeCoords, newBridge);
                }
                return;
            }
        }
    }

    public void DestroyAllChildren()
    {
        for (int i = parent.childCount; i > 0; --i)
        {
            DestroyImmediate(parent.GetChild(0).gameObject);
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
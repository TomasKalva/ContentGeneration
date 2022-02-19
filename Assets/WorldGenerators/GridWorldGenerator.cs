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
    Styles styles;

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

    public ModuleGrid moduleGrid;

    Graph<Area> areasGraph;

    WorldTraversingAgent agent;

    public override void Generate(World world)
    {
        moduleGrid = new ModuleGrid(sizes, extents, parent);
        areasGraph = new Graph<Area>();
        agent = new WorldTraversingAgent();

        InitGrid();

        AddBuildings(buildingsCount);

        AddBridges(bridgesCount);

        AddRooftops();

        var areas = moduleGrid.Select(module => module.GetProperty<AreaModuleProperty>().Area).Distinct();
        areas.ForEach(area => area.Finish(moduleGrid));

        foreach (var module in moduleGrid)
        {
            var designer = module.GetProperty<AreaModuleProperty>().Area.Designer;
            designer.Design(moduleGrid, areasGraph, module);
        }

        agent.Traverse(moduleGrid);

        foreach (var module in moduleGrid)
        {
            module.PlaceObjects(moduleGrid);
        }

        var startArea = areas.Where(area => !area.Modules.Any(module => module.coords.y == 0 || module.Outside)).GetRandom();
        startArea.AddObject(new WorldObjectObject(interactiveObjects.Grave().transform));
        foreach (var area in areas)
        {
            if(!area.Modules.Any(module => module.coords.y == 0 || module.Outside))
            {
                area.AddCharacter(new CharacterWorldObject(libraries.Enemies.MayanThrower()));
                area.AddCharacter(new CharacterWorldObject(libraries.Enemies.MayanSwordsman()));
                //area.AddCharacter(new CharacterWorldObject(libraries.Enemies.MayanSwordsman()));
                area.AddItem(new ItemWorldObject(physicalItems.BlueIchorEssence()));
            }
            area.PlaceObjects(moduleGrid, libraries);
        }
    }

    void InitGrid()
    {
        var openArea = new Outdoor(new Box3Int(Vector3Int.zero, sizes), modules, styles);
        openArea.Generate(moduleGrid);
        foreach(var module in moduleGrid)
        {
            module.GetProperty<AreaModuleProperty>().Area.AreaType = "Outside";
            module.GetProperty<AreaModuleProperty>().Area.Inside = false;
        }
    }

    void AddBuildings(int n)
    {
        /*var town = new UniformTown(new Box3Int(Vector3Int.zero, moduleGrid.Sizes).FlattenY(), new Vector2Int(5, 5), modules, styles);
        town.Generate(moduleGrid);
        */
        /*var modernBuilding = new ModernBuilding(new Box3Int(Vector3Int.zero, 5 * Vector3Int.one), modules, styles);
        modernBuilding.Generate(moduleGrid);*/
        /*
        var rectangles = new UniformRectangles(new Box3Int(Vector3Int.zero, moduleGrid.Sizes).FlattenY(), new Vector2Int(5, 5), modules, styles);
        rectangles.Generate(moduleGrid);*/
        
        
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
        var building = new Building(new Box3Int(new Vector3Int(minX, minY, minZ), new Vector3Int(maxX, maxY, maxZ)), modules, styles);
        building.Generate(moduleGrid);
    }

    Module RandomModule()
    {
        int x = UnityEngine.Random.Range(0, moduleGrid.Width);
        int y = UnityEngine.Random.Range(0, moduleGrid.Height);
        int z = UnityEngine.Random.Range(0, moduleGrid.Depth);
        return moduleGrid[x, y, z];
    }

    void AddRooftops()
    {
        var rooftops = new Rooftops(modules, styles);
        rooftops.Generate(moduleGrid);
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
        int i = 0;
        while(bridgesCount < n)
        {
            bridgesCount += AddBridge() ? 1 : 0;
            if(i++ > 100)
            {
                return;
            }
        }
    }

    bool ContainsBuilding(Module module)
    {
        return module != null && module.GetProperty<TopologyProperty>().HasCeiling();
    }

    public bool HasHorizontalNeighbor(Module module)
    {
        return ContainsBuilding(moduleGrid[module.coords + Vector3Int.right]) ||
                ContainsBuilding(moduleGrid[module.coords - Vector3Int.right]) ||
                ContainsBuilding(moduleGrid[module.coords + Vector3Int.forward]) ||
                ContainsBuilding(moduleGrid[module.coords - Vector3Int.forward]);
    }

    /// <summary>
    /// Returns true if adding bridge was successfull.
    /// </summary>
    bool AddBridge()
    {
        var startModule = SatisfyingModule(module => HasHorizontalNeighbor(module) && module.Outside);
        var bridge = new Bridge(startModule, modules, styles);
        return bridge.Generate(moduleGrid);
    }

    public override void DestroyWorld()
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

            generator.DestroyWorld();
            generator.Generate(null);
        }
        if (GUILayout.Button("Delete", GUILayout.Width(60), GUILayout.Height(30)))
        {
            var generator = (GridWorldGenerator)target;

            generator.DestroyWorld();
        }
        GUILayout.Label("generate");
    }
}
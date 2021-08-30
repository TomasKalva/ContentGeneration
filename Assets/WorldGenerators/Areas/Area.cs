using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Area : Vertex
{
    protected Modules moduleLibrary;

    List<Module> modules;

    protected Area(Modules moduleLibrary)
    {
        this.moduleLibrary = moduleLibrary;
        modules = new List<Module>();
    }

    public void AddModule(Module module)
    {
        modules.Add(module);
    }

    public void RemoveModule(Module module)
    {
        modules.Remove(module);
    }

    public bool ContainsModule(Module module) => modules.Where(m => m == module).Any();

    public abstract bool Generate(ModuleGrid moduleGrid);
}

public class Building : Area
{
    Vector3Int leftBottomBack;
    Vector3Int rightTopFront;

    public Building(Vector3Int leftBottomBack, Vector3Int rightTopFront, Modules moduleLibrary) : base(moduleLibrary)
    {
        this.leftBottomBack = leftBottomBack;
        this.rightTopFront = rightTopFront;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        for (int j = leftBottomBack.y; j < rightTopFront.y; j++)
        {
            var room = new Room(leftBottomBack.XZ(), rightTopFront.XZ(), j, moduleLibrary);
            room.Generate(moduleGrid);
        }
        //var openArea = ExtensionMethods.RandomBox(new Box3Int(leftBottomBack, rightTopFront));
        var openArea = new OpenArea(new Box3Int(leftBottomBack, rightTopFront).Padding(Vector3Int.one), moduleLibrary);
        openArea.Generate(moduleGrid);

        return true;
    }
}

public class Room : Area
{
    Vector2Int leftFront;
    Vector2Int rightBack;
    int height;

    public Room(Vector2Int leftFront, Vector2Int rightBack, int height, Modules moduleLibrary) : base(moduleLibrary)
    {
        this.leftFront = leftFront;
        this.rightBack = rightBack;
        this.height = height;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        for (int i = leftFront.x; i < rightBack.x; i++)
        {
            for (int k = leftFront.y; k < rightBack.y; k++)
            {
                var coords = new Vector3Int(i, height, k);
                /*if (!moduleGrid.IsEmpty(moduleGrid[coords]))
                {
                    continue;
                }*/

                var module = moduleLibrary.RoomModule();
                moduleGrid[coords] = module;
                module.AddProperty(new AreaModuleProperty(this));
            }
        }
        var stairsPos = ExtensionMethods.RandomVector2Int(leftFront, rightBack);
        var stairsCoords = new Vector3Int(stairsPos.x, height, stairsPos.y);
        //GameObject.DestroyImmediate(moduleGrid[stairsCoords].gameObject);

        var stModule = moduleLibrary.StairsModule();
        moduleGrid[stairsCoords] = stModule;
        stModule.AddProperty(new AreaModuleProperty(this));

        return true;
    }
}

public class OpenArea : Area
{
    Box3Int box;

    public OpenArea(Box3Int box, Modules moduleLibrary) : base(moduleLibrary)
    {
        this.box = box;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        foreach(var coords in box)
        {
            var emptyModule = moduleLibrary.EmptyModule();
            moduleGrid[coords] = emptyModule;
            emptyModule.AddProperty(new AreaModuleProperty(this));
        }

        return true;
    }
}

public class BridgeArea : Area
{
    Module startModule;

    public BridgeArea(Module startModule, Modules moduleLibrary) : base(moduleLibrary)
    {
        this.startModule = startModule;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        //var startModule = SatisfyingModule(module => module.empty && HasHorizontalNeighbor(module));
        if (startModule == null)
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
            while (module != null && module.empty)
            {
                coords += direction;
                dist += 1;
                module = moduleGrid[coords];
            }
            if (module == null)
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
                    var newBridge = moduleLibrary.BridgeModule();
                    moduleGrid[bridgeCoords] = newBridge;
                    newBridge.AddProperty(new AreaModuleProperty(this));
                }
                return true;
            }
        }
        return false;
    }
}

public struct Box3Int : IEnumerable<Vector3Int>
{
    public Vector3Int leftBottomBack;
    public Vector3Int rightTopFront;

    public Box3Int(Vector3Int leftBottomBack, Vector3Int rightTopFront)
    {
        this.leftBottomBack = leftBottomBack;
        this.rightTopFront = rightTopFront;
    }

    public Box3Int Padding(Vector3Int border) 
    {
        return new Box3Int(leftBottomBack + border, rightTopFront - border);
    }

    public IEnumerator<Vector3Int> GetEnumerator()
    {
        for (int i = leftBottomBack.x; i < rightTopFront.x; i++)
        {
            for (int j = leftBottomBack.y; j < rightTopFront.y; j++)
            {
                for (int k = leftBottomBack.z; k < rightTopFront.z; k++)
                {
                    yield return new Vector3Int(i, j, k);
                }
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
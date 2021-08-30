using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Area
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

    public bool ContainsCoords(Vector3Int coords) => modules.Where(module => module.coords == coords).Any();

    public abstract void Generate(ModuleGrid moduleGrid);
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

    public override void Generate(ModuleGrid moduleGrid)
    {
        for (int j = leftBottomBack.y; j < rightTopFront.y; j++)
        {
            var room = new Room(leftBottomBack.XZ(), rightTopFront.XZ(), j, moduleLibrary);
            room.Generate(moduleGrid);
        }
        //var openArea = ExtensionMethods.RandomBox(new Box3Int(leftBottomBack, rightTopFront));
        var openArea = new Box3Int(leftBottomBack, rightTopFront).Padding(Vector3Int.one);

        foreach (var coord in openArea)
        {
            moduleGrid[coord] = moduleLibrary.EmptyModule();
        }
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

    public override void Generate(ModuleGrid moduleGrid)
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
    }
}

public class OpenArea : Area
{
    Vector2Int leftFront;
    Vector2Int rightBack;
    int height;

    public OpenArea(Vector2Int leftFront, Vector2Int rightBack, int height, Modules moduleLibrary) : base(moduleLibrary)
    {
        this.leftFront = leftFront;
        this.rightBack = rightBack;
        this.height = height;
    }

    public override void Generate(ModuleGrid moduleGrid)
    {
        /*for (int i = leftFront.x; i < rightBack.x; i++)
        {
            for (int k = leftFront.y; k < rightBack.y; k++)
            {
                var coords = new Vector3Int(i, height, k);
                if (!moduleGrid.IsEmpty(moduleGrid[coords]))
                {
                    continue;
                }

                var module = moduleLibrary.RoomModule();
                moduleGrid[coords] = module;
                module.AddProperty(new AreaModuleProperty(this));
            }
        }
        var stairsPos = ExtensionMethods.RandomVector2Int(leftFront, rightBack);
        var stairsCoords = new Vector3Int(stairsPos.x, height, stairsPos.y);
        GameObject.DestroyImmediate(moduleGrid[stairsCoords].gameObject);

        var stModule = moduleLibrary.StairsModule();
        moduleGrid[stairsCoords] = stModule;
        stModule.AddProperty(new AreaModuleProperty(this));*/
    }
}

struct Box3Int : IEnumerable<Vector3Int>
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
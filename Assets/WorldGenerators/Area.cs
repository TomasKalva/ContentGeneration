using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Area
{
    static int areaId = 0;

    string _name;
    public string Name => _name;

    string _areaType;
    public string AreaType
    {
        get => _areaType;
        set
        {
            _name = $"{value}{areaId++}";
            _areaType = value;
        }
    }

    public bool Inside { get; set; }
    public bool Outside => !Inside;

    protected List<Module> modules;

    public Style Style { get; }

    public Designer Designer { get; }

    List<WorldObject> objectsToPlace;

    List<Transform> placedObjects;

    public IEnumerable<Module> Modules => modules;

    public Area(Designer designer, Style style)
    {
        modules = new List<Module>();
        objectsToPlace = new List<WorldObject>();
        placedObjects = new List<Transform>();
        Style = style;
        Designer = designer;
        Inside = false;
    }

    public void AddModule(Module module)
    {
        modules.Add(module);
    }

    public void RemoveModule(Module module)
    {
        modules.Remove(module);
    }

    public void AddCharacter(CharacterWorldObject character)
    {
        objectsToPlace.Add(character);
    }

    public void AddItem(ItemWorldObject item)
    {
        objectsToPlace.Add(item);
    }

    public bool ContainsModule(Module module) => modules.Where(m => m == module).Any();

    public void PlaceObjects(ModuleGrid grid)
    {
        foreach(var character in objectsToPlace)
        {
            var dest = modules.GetRandom();
            var placedObj = character.SetObject(Style, dest.transform);
            placedObjects.Add(placedObj);
        }
    }

    public virtual void Finish(ModuleGrid grid) { }

    public void Disable()
    {
        foreach(var obj in placedObjects)
        {
            if (obj != null)
            {
                obj.gameObject.SetActive(false);
            }
        }
    }

    public void Enable()
    {
        foreach (var obj in placedObjects)
        {
            if (obj != null)
            {
                obj.gameObject.SetActive(true);
            }
        }
    }
}

public class DisconnectedArea : Area
{
    public DisconnectedArea(Designer designer, Style style) : base(designer, style)
    {
    }

    public override void Finish(ModuleGrid grid)
    {
        foreach(var module in modules)
        {
            var topology = module.GetProperty<TopologyProperty>();
            foreach (var dir in ExtensionMethods.HorizontalDirections())
            {
                var neighbor = grid[module.coords + dir];
                if (neighbor != null && neighbor.GetProperty<AreaModuleProperty>().Area == module.GetProperty<AreaModuleProperty>().Area)
                {
                    topology.SetReachable(dir);
                }
                else
                {
                    topology.SetUnreachable(dir);
                }
            }
        }
    }
}

public enum CharacterType
{
    Tall
}

public abstract class WorldObject
{
    public abstract Transform SetObject(Style style, Transform parent);
}

public class CharacterWorldObject : WorldObject
{
    public CharacterType characterType { get; }

    public CharacterWorldObject(CharacterType characterType)
    {
        this.characterType = characterType;
    }

    public override Transform SetObject(Style style, Transform parent)
    {
        var character = style.GetCharacter(characterType);
        character.SetParent(parent);
        character.SetPositionAndRotation(parent.position, parent.rotation);
        return character;
    }
}

public class ItemWorldObject : WorldObject
{
    public ItemRef itemPrefab { get; }

    public ItemWorldObject(ItemRef itemPrefab)
    {
        this.itemPrefab = itemPrefab;
    }

    public override Transform SetObject(Style style, Transform parent)
    {
        var item = GameObject.Instantiate(itemPrefab);
        item.transform.SetParent(parent);
        item.transform.SetPositionAndRotation(parent.position, parent.rotation);
        return item.transform;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Module : MonoBehaviour
{
    public Vector3Int coords;

    public bool Empty => GetProperty<AreaModuleProperty>().Area.AreaType == "Empty";
    public bool Outside => GetProperty<AreaModuleProperty>().Area.AreaType == "Outside";

    List<IModuleProperty> properties;

    [SerializeField]
    AttachmentPoint[] attachmentPoints;

    [SerializeField]
    public DirectionObject[] directionObjects;

    public List<Rule> UsedRules { get; private set; }

    public IEnumerable<DirectionObject> HorizontalDirectionObjects() => directionObjects.Where(dirObj => dirObj.direction.y == 0);

    public PropertyT GetProperty<PropertyT>() where PropertyT : class
    {
        return properties.FirstOrDefault(prop => prop.GetType() == typeof(PropertyT)) as PropertyT;
    }

    public void AddProperty<PropertyT>(PropertyT property) where PropertyT : IModuleProperty
    {
        properties.Add(property);
        property.OnAdded(this);
    }

    public void OnDestroyed()
    {
        foreach(var property in properties)
        {
            property.OnModuleDestroyed(this);
        }
    }

    public void Init()
    {
        attachmentPoints = GetComponentsInChildren<AttachmentPoint>();

        properties = new List<IModuleProperty>();

        UsedRules = new List<Rule>();

        var boundingBox = transform.Cast<Transform>().FirstOrDefault(child => child.tag == "BuildingBoundingBox");
        if (boundingBox != null)
        {
            DestroyImmediate(boundingBox.gameObject);
        }
    }

    public void PlaceObjects(ModuleGrid grid)
    {
        var style = GetProperty<AreaModuleProperty>().Area.Style;
        foreach(var ap in attachmentPoints)
        {
            ap.SetObject(style);
        }
    }

    public Vector3Int DirectionTo(Module module)
    {
        return (module.coords - coords).ComponentWise(t => t > 0 ? 1 : t == 0 ? 0 : -1);
    }

    public IEnumerable<Module> HorizontalNeighbors(ModuleGrid grid)
    {
        return ExtensionMethods.HorizontalDirections().Select(dir => grid[coords + dir]).Where(m => m != null);
    }

    public IEnumerable<Module> AllNeighbors(ModuleGrid grid)
    {
        return ExtensionMethods.Directions().Select(dir => grid[coords + dir]).Where(m => m != null);
    }

    public IEnumerable<Module> AllAbove(ModuleGrid moduleGrid)
    {
        for(int y = coords.y; y < moduleGrid.Height; y++)
        {
            yield return moduleGrid[new Vector3Int(coords.x, y, coords.z)];
        }
    }

    public void SetDirection(Vector3Int direction, ObjectType objectType)
    {
        var dirObj = directionObjects.FirstOrDefault(dirObj => dirObj.direction == direction);
        if (dirObj != null)
        {
            dirObj.obj.objectType = objectType;
        }
    }

    public AttachmentPoint GetAttachmentPoint(Vector3Int direction)
    {
        var dirObj = directionObjects.FirstOrDefault(dirObj => dirObj.direction == direction);
        if (dirObj != null)
        {
            return dirObj.obj;
        }
        return null;
    }

    public void ClearAttachmentPoints()
    {
        foreach(var att in attachmentPoints)
        {
            att.objectType = ObjectType.Empty;
        }
    }

    public void SetObject(ObjectType objectType)
    {
        SetDirection(Vector3Int.up, objectType);
    }

    public AttachmentPoint GetObject()
    {
        return GetAttachmentPoint(Vector3Int.up);
    }
}

public enum ObjectType
{
    Wall,
    Railing,
    Window,
    Door,
    Ceiling,
    Bridge,
    Roof,
    Stairs,
    Empty
}
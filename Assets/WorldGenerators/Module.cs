using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Module : MonoBehaviour
{
    public Vector3Int coords;

    public bool empty;

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

    public void Init(Vector3Int coords)
    {
        this.coords = coords;

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

    public IEnumerable<Module> AllAbove(ModuleGrid moduleGrid)
    {
        for(int y = coords.y; y < moduleGrid.Height; y++)
        {
            yield return moduleGrid[new Vector3Int(coords.x, y, coords.y)];
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

    public virtual bool ReachableFrom(Vector3Int dir)
    {
        return true;
    }

    public bool HasCeiling()
    {
        var obj = GetObject();
        return obj != null && (obj.objectType == ObjectType.Ceiling || obj.objectType == ObjectType.Stairs);
    }

    public bool HasFloor(ModuleGrid grid)
    {
        var bottomCoords = coords - Vector3Int.up;
        if (grid.ValidCoords(bottomCoords))
        {
            var bottomModule = grid[bottomCoords];
            return bottomModule.HasCeiling();
        }
        return false;
    }

    public bool HasCeiling(Vector3Int direction)
    {
        var obj = GetObject();
        return HasCeiling() || (obj != null && (obj.objectType == ObjectType.Bridge && (obj.PointingDirection == direction || obj.PointingDirection == -direction)));
    }

    public bool HasFloor(ModuleGrid grid, Vector3Int direction)
    {
        var bottomCoords = coords - Vector3Int.up;
        if (grid.ValidCoords(bottomCoords))
        {
            var bottomModule = grid[bottomCoords];
            return bottomModule.HasCeiling(direction);
        }
        return false;
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


public struct GridDirection
{
    Vector3Int _direction;

    GridDirection(Vector3Int direction)
    {
        this._direction = direction;
    }

    public static GridDirection Forward { get; }
    public static GridDirection Right { get; }
    public static GridDirection Up { get; }
    public static GridDirection ForwardUp { get; }
    public static GridDirection ForwardDown { get; }
    public static GridDirection RightUp { get; }
    public static GridDirection RightDown { get; }
    public static GridDirection ForwardRight { get; }
    public static GridDirection ForwardLeft { get; }

    static GridDirection()
    {
        Forward = new GridDirection(Vector3Int.forward);
        Right = new GridDirection(Vector3Int.right);
        Up = new GridDirection(Vector3Int.up);
        ForwardUp = new GridDirection(Vector3Int.forward + Vector3Int.up);
        ForwardDown = new GridDirection(Vector3Int.forward - Vector3Int.up);
        RightUp = new GridDirection(Vector3Int.right + Vector3Int.up);
        RightDown = new GridDirection(Vector3Int.right - Vector3Int.up);
        ForwardRight = new GridDirection(Vector3Int.forward + Vector3Int.right);
        ForwardLeft = new GridDirection(Vector3Int.forward - Vector3Int.right);
    }

    public static GridDirection operator -(GridDirection d) => new GridDirection(-d._direction);

    public static implicit operator Vector3Int(GridDirection d) => d._direction;
}

interface IModuleGeometry : IModuleProperty
{

}

public interface IModuleProperty
{
    void OnAdded(Module module);
    void OnModuleDestroyed(Module module);

}

public class AreaModuleProperty : IModuleProperty
{
    public Area Area { get; set; }

    public AreaModuleProperty(Area area)
    {
        Area = area;
    }

    public void OnAdded(Module module)
    {
        Area.AddModule(module);
    }

    public void OnModuleDestroyed(Module module)
    {
        Area.RemoveModule(module);
    }
}


[CustomEditor(typeof(Module))]
public class ModuleOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        var module = (Module)target;
        var areaProp = module.GetProperty<AreaModuleProperty>();
        if(areaProp == null)
        {
            return;
        }
        var designer = areaProp.Area.Designer;

        var style = new GUIStyle();
        style.fontSize = 20;
        GUILayout.Label("Rules", style);
        foreach (var rule in designer.UsedRules(module))
        {
            GUILayout.Label($"{rule.RulesClass.Name}: {rule.Name}");
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomModule : Module
{
    [SerializeField]
    DirectionObject[] walls;

    public override void AfterGenerated(ModuleGrid grid)
    {
        var area = GetProperty<AreaModuleProperty>().Area;
        foreach(var dirObj in walls)
        {
            if (area.ContainsCoords(coords + dirObj.direction)) 
            {
                GameObject.DestroyImmediate(dirObj.obj);
            }
        }
    }

    public override bool ReachableFrom(GridDirection dir)
    {
        return true;
    }
}

[Serializable]
public class DirectionObject
{
    public Vector3Int direction;

    public GameObject obj;
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomModule : Module
{
    [SerializeField]
    DirectionObject[] walls;

    public override void AfterGenerated(ModuleGrid grid, AreasGraph areasGraph)
    {
        var area = GetProperty<AreaModuleProperty>().Area;
        foreach(var dirObj in walls)
        {
            var otherModule = grid[coords + dirObj.direction];
            if (area.ContainsModule(otherModule)) 
            {
                GameObject.DestroyImmediate(dirObj.obj);
            }

            if(otherModule == null)
            {
                continue;
            }

            var otherArea = otherModule.GetProperty<AreaModuleProperty>().Area;
            if(otherArea.Name == "Outside")
            {
                continue;
            }

            // Try to connect the areas if possible
            var myArea = GetProperty<AreaModuleProperty>().Area;
            if (!areasGraph.AreConnected(myArea, otherArea))
            {
                if (otherModule.ReachableFrom(dirObj.direction))
                {
                    GameObject.DestroyImmediate(dirObj.obj);
                    areasGraph.Connect(myArea, otherArea);
                }
            }
        }
    }

    public override bool ReachableFrom(Vector3Int dir)
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
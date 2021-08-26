using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeModule : Module
{
    [SerializeField]
    Transform bridge;

    public override void AfterGenerated(ModuleGrid grid)
    {
        foreach(var neighbor in HorizontalNeighbors(grid))
        {
            if(neighbor is BridgeModule)
            {
                var dir = DirectionTo(neighbor);
                var angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
                bridge.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
        } 
    }
}

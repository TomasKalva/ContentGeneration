using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BlobAgent : Agent
{
    public void Rush(Vector2 direction)
    {
        ResetState();
        var rush = acting.SelectAct("Rush") as Roll;
        rush.Direction = direction;
    }
    
    public void Explode()
    {
        ResetState();
        var attack = acting.SelectAct("Explode") as Attack;
        //attack.Direction = movement.AgentForward;
    }
}
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
    
    public void Attack()
    {
        ResetState();
        var currentAct = acting.ActiveAct;

        // do a combo if slash is currently active
        Attack attack;
        if (currentAct && currentAct.actName == "Slash")
        {
            attack = acting.SelectAct("LeftSlash") as Attack;
        }
        else
        {
            attack = acting.SelectAct("Slash") as Attack;
        }
        attack.Direction = movement.AgentForward;
    }
}
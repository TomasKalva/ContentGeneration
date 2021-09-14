using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HumanAgent : Agent
{
    public void Backstep()
    {
        acting.SelectAct("Backstep");
    }

    public void Roll(Vector2 direction)
    {
        var roll = acting.SelectAct("Roll") as Roll;
        roll.Direction = direction;
    }

    public void Attack()
    {
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
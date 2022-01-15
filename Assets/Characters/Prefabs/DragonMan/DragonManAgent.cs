using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DragonManAgent : Agent
{
    public Act Slash()
    {
        var attack = acting.SelectAct("Slash") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act FlapWings()
    {
        var attack = acting.SelectAct("FlapWings") as Shoot;
        return attack;
    }

    public Act SpitFire()
    {
        var attack = acting.SelectAct("SpitFire") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }
}
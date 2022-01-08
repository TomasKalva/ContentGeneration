using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SkinnyWomanAgent : Agent
{
    public Act RushForward()
    {
        var attack = acting.SelectAct("RushForward") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act Cast()
    {
        var attack = acting.SelectAct("Cast") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act Enchant()
    {
        var attack = acting.SelectAct("Enchant") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }
}
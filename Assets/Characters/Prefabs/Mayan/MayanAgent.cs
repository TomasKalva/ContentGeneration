using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MayanAgent : Agent
{
    public Act OverheadAttack()
    {
        var attack = acting.SelectAct("Overhead") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act LongOverheadAttack()
    {
        var attack = acting.SelectAct("LongOverhead") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act LeftSwing()
    {
        var attack = acting.SelectAct("LeftSwing") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act RightSwing()
    {
        var attack = acting.SelectAct("RightSwing") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act Throw()
    {
        var thr = acting.SelectAct("Throw") as Shoot;
        return thr;
    }
}
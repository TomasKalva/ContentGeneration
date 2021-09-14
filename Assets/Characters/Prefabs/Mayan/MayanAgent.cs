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
}
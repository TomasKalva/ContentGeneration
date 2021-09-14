using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MayanAgent : Agent
{
    public void OverheadAttack()
    {
        ResetState();
        var attack = acting.SelectAct("Overhead") as Attack;
        attack.Direction = movement.AgentForward;
    }
}
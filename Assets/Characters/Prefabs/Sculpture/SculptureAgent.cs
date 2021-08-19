using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SculptureAgent : Agent
{
    public void OverheadAttack()
    {
        ResetState();
        acting.SelectAct("Overhead");
    }

    public void WideAttack()
    {
        ResetState();
        var currentAct = acting.ActiveAct;

        // do a combo if slash is currently active
        Attack attack;
        if (currentAct && currentAct.actName == "LeftWide")
        {
            attack = acting.SelectAct("RightWideDown") as Attack;
        }
        else
        {
            attack = acting.SelectAct("LeftWide") as Attack;
        }
        attack.Direction = movement.AgentForward;
    }

    public void DoubleSwipe()
    {
        ResetState();
        acting.SelectAct("DoubleSwipe");
    }

    public void GroundSlam()
    {
        ResetState();
        acting.SelectAct("GroundSlam");
    }
}
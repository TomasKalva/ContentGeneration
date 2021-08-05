using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CrossbowmanAgent : Agent
{
    public void Backstep()
    {
        ResetState();
        acting.SelectAct("Backstep");
    }

    public void Shoot()
    {
        ResetState();
        var attack = acting.SelectAct("Shoot");
    }

    public void Strafe()
    {
        ResetState();
        var attack = acting.SelectAct("Strafe");
    }
}
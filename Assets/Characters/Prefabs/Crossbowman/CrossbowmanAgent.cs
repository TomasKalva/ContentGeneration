using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CrossbowmanAgent : Agent
{
    public void Backstep()
    {
        acting.SelectAct("Backstep");
    }

    public void Shoot()
    {
        var attack = acting.SelectAct("Shoot");
    }

    public void Strafe()
    {
        var attack = acting.SelectAct("Strafe");
    }
}
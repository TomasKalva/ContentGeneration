using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LurkerAgent : Agent
{
    public bool Burrowed { get; private set; }

    public void Burrow()
    {
        ResetState();
        acting.SelectAct("Burrow");
        Burrowed = true;
    }

    public void Unburrow()
    {
        ResetState();
        acting.SelectAct("Unburrow");
        Burrowed = false;
    }
}
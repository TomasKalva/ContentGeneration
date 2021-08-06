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

    public void Shockwave()
    {
        /*if (!Burrowed)
        {
            Debug.LogError("Can't cast shockwave while not burrowed!");
            return;
        }*/

        ResetState();
        acting.SelectAct("Shockwave");
    }
}
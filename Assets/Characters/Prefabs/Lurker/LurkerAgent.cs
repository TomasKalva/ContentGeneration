using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LurkerAgent : Agent
{
    public bool Burrowed { get; private set; }

    public void Burrow()
    {
        acting.SelectAct("Burrow");
        Burrowed = true;
    }

    public void Unburrow()
    {
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

        acting.SelectAct("Shockwave");
    }
}
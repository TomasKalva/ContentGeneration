using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MayanAgent : Agent
{
    public void OverheadAttack()
    {
        ResetState();
        acting.SelectAct("Overhead");
    }
}
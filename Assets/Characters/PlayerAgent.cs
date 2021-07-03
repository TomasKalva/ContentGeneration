using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerAgent : Agent
{
    public void Shoot()
    {
        acting.SelectAct("Shoot");
    }

    public void Dodge()
    {
        ResetState();
        movement.Dodge(20f);
        //animator.SetTrigger("Dodge");
        acting.SelectAct("Dodge");
    }

    public void Roll(Vector2 direction)
    {
        ResetState();
        var roll = acting.SelectAct("Roll") as Roll;
        roll.Direction = direction;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerAgent : Agent
{
    ManualActing manActing => (ManualActing)acting;

    public void Shoot()
    {
        manActing.DoAct("Shoot");
    }

    public void Dodge()
    {
        movement.Dodge(20f);
        manActing.DoAct("Dodge");
    }

    public void Roll()
    {
        movement.Roll(20f);
        manActing.DoAct("Roll");
    }
}
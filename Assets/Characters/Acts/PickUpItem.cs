using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpItem : AnimatedAct
{
    public PhysicalItem PhysicalItem { get; set; }

    public override void StartAct(Agent agent)
    {
        timeElapsed = 0f;

        agent.animator.CrossFade(animationName, 0.05f);
    }

    public override void EndAct(Agent agent)
    {
        PhysicalItem.PickUpItem(agent);
    }
}
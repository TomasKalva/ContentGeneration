﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaggeredAct : AnimatedAct
{
    public Vector3 PushForce { get; set; }

    public StaggeredAct()
    {
        actName = "Staggered";
        type = ActType.IDLE;
        priority = 0;
    }

    public override void OnStart(Agent agent)
    {
        agent.movement.Impulse(PushForce);
    }

    public override bool UpdateAct(Agent agent)
    {
        timeElapsed += Time.fixedDeltaTime;
        return timeElapsed >= duration;
    }
}
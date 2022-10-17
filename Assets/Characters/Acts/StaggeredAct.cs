﻿using UnityEngine;

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
        PlayAnimation(agent);

        agent.movement.Impulse(PushForce);

        agent.movement.VelocityUpdater = new DontChangeVelocityUpdater(duration);
    }
}
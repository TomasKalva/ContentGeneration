using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleAct : AnimatedAct
{
    public IdleAct()
    {
        actName = "Idle";
        type = ActType.IDLE;
        priority = -100;
    }

    public override bool UpdateAct(Agent agent)
    {
        PlayIfNotActive(agent, 0.1f);

        return true;
    }
}
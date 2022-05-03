using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wait : Behavior
{
	Func<Agent, Vector3> WaitPosition { get; }

    Func<Agent, bool> ShouldWait { get; }

    float MinDistance => 0.5f;

    public Wait(Func<Agent, bool> shouldWait, Func<Agent, Vector3> waitPosition)
    {
        ShouldWait = shouldWait;
        WaitPosition = waitPosition;
    }

    public override bool CanEnter(Agent agent)
    {
        return ShouldWait(agent);
    }

    public override int Priority(Agent agent) => 100;

    public override void Enter(Agent agent)
    {
    }

    public override bool Update(Agent agent)
    {
        var targetPoint = WaitPosition(agent);
        if((targetPoint - agent.transform.position).magnitude > MinDistance &&
            agent.CanMove)
        {
            agent.Run(targetPoint.XZ());
        }

        return !ShouldWait(agent);
    }
}

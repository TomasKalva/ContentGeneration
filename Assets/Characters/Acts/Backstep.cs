using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backstep : AnimatedAct
{
    [SerializeField, Curve(0f, 0f, 1f, 30f, true)]
    AnimationCurve speedF;

    public override void OnStart(Agent agent)
    {
        PlayAnimation(agent);

        Direction3F direction = () => -agent.movement.AgentForward;
        agent.movement.VelocityUpdater = new CurveVelocityUpdater(speedF, duration, direction);

        SetupMovementConstraints(agent, new VelocityInDirection(direction));
    }

    public override void EndAct(Agent agent)
    {
        MovementContraints.ForEach(con => con.Finished = true);
    }
}

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

        movementContraints = new List<MovementConstraint>()
        {
            new VelocityInDirection(direction),
        };

        movementContraints.ForEach(con => agent.movement.Constraints.Add(con));
    }

    public override void EndAct(Agent agent)
    {
        movementContraints.ForEach(con => con.Finished = true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backstep : AnimatedAct
{
    [SerializeField, Curve(0f, 0f, 1f, 15f, true)]
    AnimationCurve speedF;

    public override IEnumerator Perform(Agent agent)
    {
        agent.animator.CrossFade(animationName, 0.05f);
        agent.movement.VelocityUpdater = new CurveVelocityUpdater(speedF, duration, -agent.movement.AgentForward);
        var dirConstr = new VelocityInDirection(-agent.movement.AgentForward);
        agent.movement.Constraints.Add(dirConstr);

        yield return new WaitForSeconds(duration);
        dirConstr.Finished = true;
    }
}

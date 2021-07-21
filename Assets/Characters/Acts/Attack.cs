using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : AnimatedAct
{
    [SerializeField, Curve(0f, 0f, 1f, 15f, true)]
    AnimationCurve speedF;

    Vector3 direction;
    public Vector3 Direction
    {
        get => direction;
        set => direction = value.normalized;
    }

    public override IEnumerator Perform(Agent agent)
    {
        agent.animator.CrossFade(animationName, 0.05f);
        agent.movement.VelocityUpdater = new CurveVelocityUpdater(speedF, duration, Direction);
        var dirConstr = new VelocityInDirection(Direction);
        agent.movement.Constraints.Add(dirConstr);
        //var turnConstr = new TurnToDirection(Direction);
        //agent.movement.Constraints.Add(turnConstr);

        yield return new WaitForSeconds(duration);
        dirConstr.Finished = true;
        //turnConstr.Finished = true;
    }
}
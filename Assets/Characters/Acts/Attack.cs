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

    [SerializeField]
    Weapon weapon;

    public override void StartAct(Agent agent)
    {
        timeElapsed = 0f;

        agent.animator.CrossFade(animationName, 0.05f);
        agent.movement.VelocityUpdater = new CurveVelocityUpdater(speedF, duration, Direction);

        movementContraints = new List<MovementConstraint>()
        {
            new VelocityInDirection(Direction),
        };

        movementContraints.ForEach(con => agent.movement.Constraints.Add(con));

        weapon.Active = true;
    }

    public override void EndAct(Agent agent)
    {
        weapon.Active = false;
        movementContraints.ForEach(con => con.Finished = true);
    }
}
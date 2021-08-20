using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnBy : AnimatedAct
{
    [SerializeField]
    float angle;

    Vector2 desiredDirection;

    public override void OnStart(Agent agent)
    {
        var dir = agent.movement.direction;
        var desDir3d = Quaternion.Euler(0, angle, 0) * new Vector3(dir.x, 0f, dir.y);
        desiredDirection = new Vector2(desDir3d.x, desDir3d.z);
    }

    public override bool UpdateAct(Agent agent, float dt)
    {
        agent.movement.Turn(desiredDirection);
        var epsilonDegrees = 1f;
        return Mathf.Abs(Vector2.Angle(agent.movement.direction, desiredDirection)) <= epsilonDegrees;
    }
}
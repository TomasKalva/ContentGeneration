using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Awareness : Behavior
{
	Transform targetPoint;

    protected Vector3 TargetPoint => targetPoint ? targetPoint.position : Vector3.zero;

    Vector2 optimalDistance;

	float maxDistance;

    bool moveForward;

    public Awareness(float maxDistance, Vector2 optimalDistance)
    {
        this.maxDistance = maxDistance;
        this.optimalDistance = optimalDistance;
        this.moveForward = false;
    }

    bool BreakAwareness(Agent agent)
	{
        return agent.Behaviors.BehaviorPossible(agent, 4) || Vector3.Distance(agent.transform.position, TargetPoint) > maxDistance;
	}

    public override bool CanEnter(Agent agent)
    {
        return true;
    }

    public override int Priority(Agent agent) => 3;

    public override void Enter(Agent agent)
    {
        targetPoint = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override bool UpdateBehavior(Agent agent)
    {
        Vector3 direction = TargetPoint - agent.movement.body.position;
        var toTargetDir = new Vector2(direction.x, direction.z);

        // decide where to go
        var distToTarget = direction.magnitude;
        if (distToTarget > optimalDistance.y)
        {
            moveForward = true;
        }
        else if (distToTarget < optimalDistance.x)
        {
            moveForward = false;
        }

        // go
        if (moveForward)
        {
            agent.Run(toTargetDir);
        }
        else
        {
            agent.WalkBack(-toTargetDir);
        }

        return BreakAwareness(agent);
    }
}

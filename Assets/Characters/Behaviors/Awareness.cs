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

    float MinWill { get; }

    float RequiredWill { get; }

    bool LowWill { get; set; }


    public Awareness(float maxDistance, Vector2 optimalDistance, float minWill, float requiredWill)
    {
        this.maxDistance = maxDistance;
        this.optimalDistance = optimalDistance;
        this.moveForward = false;
        this.MinWill = minWill;
        this.RequiredWill = requiredWill;
        this.LowWill = false;
    }

    bool BreakAwareness(Agent agent)
	{
        return (!LowWill || agent.CharacterState.Stamina > RequiredWill) && (agent.Behaviors.BehaviorPossible(agent, 4) || Vector3.Distance(agent.transform.position, TargetPoint) > maxDistance);
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

    public override bool Update(Agent agent)
    {
        Vector3 direction = TargetPoint - agent.movement.body.position;
        var toTargetDir = direction.XZ();

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

        if(agent.CharacterState.Stamina < MinWill)
        {
            LowWill = true;
        }
        if (agent.CharacterState.Stamina >= RequiredWill)
        {
            LowWill = false;
        }

        return BreakAwareness(agent);
    }
}

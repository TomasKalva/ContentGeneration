using ContentGeneration.Assets.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnToTargetBehavior : Behavior
{
	Transform targetPoint;

	protected Vector3 TargetPoint => targetPoint != null ? targetPoint.position : Vector3.zero;

	float maxAngle;

    float cosMaxAngle;

    public TurnToTargetBehavior(float maxAngle)
    {
        this.maxAngle = maxAngle;
        cosMaxAngle = Mathf.Cos(maxAngle * Mathf.Deg2Rad);
        World.OnCreated += Initialize;
    }

    bool ShouldTurnToTarget(Agent agent)
	{
        return Vector2.Dot(agent.movement.direction, (TargetPoint - agent.transform.position).XZ().normalized) < cosMaxAngle;
	}

    void Initialize()
    {
    }

    public override bool CanEnter(Agent agent)
    {
        return ShouldTurnToTarget(agent);
    }

    public override void Enter(Agent agent)
    {
        targetPoint = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override int Priority(Agent agent) => 1;

    public override bool Update(Agent agent)
    {
        Vector3 direction = TargetPoint - agent.transform.position;
        var moveDirection = direction.XZ().normalized;
        agent.Turn(moveDirection); 

        return !ShouldTurnToTarget(agent);
    }
}

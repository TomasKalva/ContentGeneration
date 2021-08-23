using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnToTargetBehavior : Behavior
{
	[SerializeField]
	Transform targetPoint;

	protected Vector3 TargetPoint => targetPoint != null ? targetPoint.position : Vector3.zero;

	[SerializeField]
	float maxAngle;

    float cosMaxAngle;

	bool ShouldTurnToTarget(Agent agent)
	{
        return Vector2.Dot(agent.movement.direction, (TargetPoint - agent.transform.position).XZ().normalized) < cosMaxAngle;
	}

    private void Awake()
    {
        cosMaxAngle = Mathf.Cos(maxAngle * Mathf.Deg2Rad);
        World.OnCreated += Initialize;
    }

    void Initialize()
    {
        targetPoint = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override bool CanEnter(Agent agent)
    {
        return ShouldTurnToTarget(agent);
    }

    public override int Priority(Agent agent) => 1;

    public override bool UpdateBehavior(Agent agent)
    {
        Vector3 direction = TargetPoint - agent.transform.position;
        var moveDirection = direction.XZ().normalized;
        agent.Turn(moveDirection); 

        return !ShouldTurnToTarget(agent);
    }
}

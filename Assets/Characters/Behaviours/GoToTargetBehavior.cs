using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoToTargetBehavior : Behavior
{
	[SerializeField]
	Transform targetPoint;

	protected Vector3 TargetPoint => targetPoint ? targetPoint.position : Vector3.zero;

	[SerializeField]
	float minDistance;

	bool ShouldGoToTarget(Agent agent)
	{
		return agent.CanMove && Vector3.Distance(transform.position, TargetPoint) > minDistance;
	}

	protected float DistanceToTarget(Agent agent) => Vector3.Distance(agent.transform.position, TargetPoint);

    [SerializeField]
    Move moveAct;

    public override bool CanEnter(Agent agent)
    {
        return ShouldGoToTarget(agent);
    }
    /// <summary>
    /// Number in [0, 10]. The higher the more the agent wants to do this behaviour.
    /// </summary>
    public override int Priority(Agent agent) => 2;

    public override void Enter(Agent agent)
    {
        var move = agent.acting.SelectAct(moveAct) as Move; 
        Vector3 direction = TargetPoint - agent.movement.body.position;
        move.Direction = new Vector2(direction.x, direction.z);
    }
}

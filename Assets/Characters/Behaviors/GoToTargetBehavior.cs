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
	float maxDistance;

	bool ShouldGoToTarget(Agent agent)
	{
		return agent.CanMove && Vector3.Distance(transform.position, TargetPoint) > maxDistance;
	}

    private void Awake()
    {
        World.OnCreated += Initialize;
    }

    void Initialize()
    {
        targetPoint = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override bool CanEnter(Agent agent)
    {
        return ShouldGoToTarget(agent);
    }

    public override int Priority(Agent agent) => 2;

    public override void Enter(Agent agent)
    {
    }

    public override bool UpdateBehavior(Agent agent)
    {
        Vector3 direction = TargetPoint - agent.movement.body.position;
        var moveDirection = new Vector2(direction.x, direction.z);
        agent.Run(moveDirection); 

        return !ShouldGoToTarget(agent);
    }
}

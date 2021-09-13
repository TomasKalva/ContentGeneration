using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaitForPlayer : Behavior
{
	[SerializeField]
	Transform targetPoint;

    bool PlayerFound { get; set; }

	protected Vector3 TargetPoint => targetPoint ? targetPoint.position : Vector3.zero;

	[SerializeField]
	float maxDistance;

	bool CloseToTarget(Agent agent)
	{
        var agentPos = agent.transform.position;
        return Vector3.Distance(agentPos, TargetPoint) <= maxDistance;
	}

    bool TargetPointVisible(Agent agent)
    {
        var agentPos = agent.transform.position;
        return ExtensionMethods.IsPointInDirection(agentPos, agent.movement.AgentForward, TargetPoint);
    }

    private void Awake()
    {
        World.OnCreated += Initialize;
        PlayerFound = false;
    }

    void Initialize()
    {
        targetPoint = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override bool CanEnter(Agent agent)
    {
        return !CloseToTarget(agent) || !TargetPointVisible(agent);
    }

    public override int Priority(Agent agent) => PlayerFound ? 0 : 10;

    public override void Enter(Agent agent)
    {
        PlayerFound = false;
    }

    public override bool UpdateBehavior(Agent agent)
    {
        PlayerFound = CloseToTarget(agent) && TargetPointVisible(agent);
        return PlayerFound;
    }
}

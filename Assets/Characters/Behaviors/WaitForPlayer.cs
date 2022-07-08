using ContentGeneration.Assets.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaitForPlayer : Behavior
{
	Transform targetPoint;

    bool PlayerFound { get; set; }

	protected Vector3 TargetPoint => targetPoint ? targetPoint.position : Vector3.zero;

	float maxDistance;

    public WaitForPlayer(float maxDistance)
    {
        this.maxDistance = maxDistance;
        PlayerFound = false;
        World.OnCreated += Initialize;
    }

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

    void Initialize()
    {
    }

    public override bool CanEnter(Agent agent)
    {
        return !CloseToTarget(agent) || !TargetPointVisible(agent);
    }

    public override int Priority(Agent agent) => PlayerFound ? 0 : 10;

    public override void Enter(Agent agent)
    {
        targetPoint = GameObject.FindGameObjectWithTag("Player").transform;
        PlayerFound = false;
    }

    public override bool Update(Agent agent)
    {
        if(targetPoint == null)
        {
            targetPoint = GameObject.FindGameObjectWithTag("Player").transform;
        }

        PlayerFound = CloseToTarget(agent) && TargetPointVisible(agent);
        return PlayerFound;
    }
}

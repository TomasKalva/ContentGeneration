using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

[RequireComponent(typeof(Agent))]
public class EnemyController<AgentT> : MonoBehaviour where AgentT : Agent
{
	protected AgentT agent;

	[SerializeField]
	Transform targetPoint;

	Vector3 TargetPoint => targetPoint ? targetPoint.position : Vector3.zero;

	[SerializeField]
	float minDistance;

	bool GoToTarget()
    {
		return Vector3.Distance(transform.position, TargetPoint) > minDistance;
    }

	// Start is called before the first frame update
	void Awake()
	{
		agent = GetComponent<AgentT>();
	}

    private void Start()
	{
		targetPoint = GameObject.FindGameObjectWithTag("Player").transform;
	}

    // Update is called once per frame
    void Update()
	{
		agent.StartReceivingControls();

		Vector3 direction = TargetPoint - agent.movement.body.position;
		Vector2 movementDirection = new Vector2(direction.x, direction.z);
		movementDirection = Vector2.ClampMagnitude(movementDirection, 1f);
		if (GoToTarget())
		{
			agent.Move(movementDirection);
		}
		else
		{
			agent.Turn(movementDirection);
		}

		if (!agent.acting.Busy)
		{
			UpdateController(movementDirection);
		}

		agent.UpdateAgent();
	}

	protected virtual void UpdateController(Vector2 movementDirection)
	{
		foreach (var act in agent.acting.acts)
		{
			if (act.CanBeUsed())
			{
				agent.acting.SelectAct(act);
			}
		}
	}
}

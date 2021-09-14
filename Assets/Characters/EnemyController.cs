using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

[RequireComponent(typeof(Agent))]
public abstract class EnemyController<AgentT> : MonoBehaviour where AgentT : Agent
{
	protected AgentT agent;

	[SerializeField]
	Transform targetPoint;

	protected Vector3 TargetPoint => targetPoint ? targetPoint.position : Vector3.zero;

	[SerializeField]
	float minDistance;

	bool GoToTarget()
    {
		return agent.CanMove && Vector3.Distance(transform.position, TargetPoint) > minDistance;
    }

	protected float DistanceToTarget => (TargetPoint - agent.transform.position).magnitude;

	protected Behaviors Behaviors { get; set; }

	// Start is called before the first frame update
	void Awake()
	{
		agent = GetComponent<AgentT>();
		Behaviors = new Behaviors();
		Initialize();
		World.OnCreated += OnWorldCreated;
	}

	protected virtual void Initialize() { }

	void OnWorldCreated()
	{
		targetPoint = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void FixedUpdate()
	{
		agent.StartReceivingControls();

		/*Vector3 direction = TargetPoint - agent.movement.body.position;
		Vector2 movementDirection = new Vector2(direction.x, direction.z);
		movementDirection = Vector2.ClampMagnitude(movementDirection, 1f);
		if (GoToTarget())
		{
			agent.Run(movementDirection);
		}
		else
		{
			agent.Turn(movementDirection);
		}

		UpdateController(movementDirection);
		*/
		Behaviors.UpdateBehavior(agent);

		agent.UpdateAgent();
	}

	protected virtual void UpdateController(Vector2 movementDirection)
	{
		foreach (var act in agent.acting.Acts)
		{
			if (act.CanBeUsed())
			{
				agent.acting.SelectAct(act);
			}
		}
	}
}

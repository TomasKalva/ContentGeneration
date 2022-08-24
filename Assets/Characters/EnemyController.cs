using ContentGeneration.Assets.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

[RequireComponent(typeof(Agent))]
public abstract class EnemyController<AgentT> : MonoBehaviour where AgentT : Agent
{
	protected AgentT agent;



	// Start is called before the first frame update
	void Awake()
	{
		agent = GetComponent<AgentT>();
	}

    private void Start()
    {
		AddBehaviors(agent.CharacterState.Behaviors);
		agent.acting.MyReset();
	}

    public virtual void AddBehaviors(Behaviors behaviors) { }

	void OnWorldCreated()
	{
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
		agent.CharacterState?.Behaviors.UpdateBehavior(agent);

		agent.UpdateAgent();
	}
	/*
	protected virtual void UpdateController(Vector2 movementDirection)
	{
	}*/
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

[RequireComponent(typeof(Agent))]
public class EnemyController : MonoBehaviour
{
	Agent agent;

	[SerializeField]
	Transform targetPoint;

	[SerializeField]
	float minDistance;

	bool GoToTarget()
    {
		return Vector3.Distance(transform.position, targetPoint.position) > minDistance;
    }

	// Start is called before the first frame update
	void Start()
	{
		agent = GetComponent<Agent>();
	}

	// Update is called once per frame
	void Update()
	{
		agent.StartReceivingControls();

		Vector3 direction = targetPoint.position - agent.movement.body.position;
		Vector2 movementDirection = new Vector2(direction.x, direction.z);
		if (GoToTarget())
		{
			movementDirection = Vector2.ClampMagnitude(movementDirection, 1f);
			agent.Move(movementDirection);
		}
		agent.movement.Turn(movementDirection);
		foreach (var act in agent.acting.acts) 
		{
            if (act.CanBeUsed())
            {
				agent.acting.SelectAct(act);
            }
		}

		agent.UpdateAgent();
	}


}

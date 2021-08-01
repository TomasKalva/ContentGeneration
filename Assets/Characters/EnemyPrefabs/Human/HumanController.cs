using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HumanAgent))]
public class HumanController : MonoBehaviour
{
	HumanAgent agent;

	[SerializeField]
	Transform targetPoint;

	[SerializeField]
	float minDistance;

	[SerializeField]
	Detector attackArea;

	bool GoToTarget()
	{
		return Vector3.Distance(transform.position, targetPoint.position) > minDistance;
	}

	// Start is called before the first frame update
	void Awake()
	{
		agent = GetComponent<HumanAgent>();
		targetPoint = GameObject.Find("Player").transform;
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
        else
        {
			agent.Turn(movementDirection);
		}
		//agent.Turn(movementDirection);

		if (!agent.acting.Busy)
		{
			// check if my attack hits enemy
			if (attackArea.triggered)
			{
				var hitAgent = attackArea.other.GetComponentInParent<Agent>();
				if (hitAgent != agent)
				{
					Debug.Log(agent.acting.ActiveAct.type);
					if (hitAgent.acting.ActiveAct.type == ActType.OFFENSIVE)
					{
						agent.Backstep();
					}
					else
					{
						agent.Attack();
					}
				}
			}
		}

		agent.UpdateAgent();
	}


}
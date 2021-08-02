using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BlobAgent))]
public class BlobController : MonoBehaviour
{
	BlobAgent agent;

	[SerializeField]
	Transform targetPoint;

	[SerializeField]
	float minDistance;

	[SerializeField]
	Detector rushArea;

	[SerializeField]
	Detector explosionArea;

	bool GoToTarget()
	{
		return Vector3.Distance(transform.position, targetPoint.position) > minDistance;
	}

	// Start is called before the first frame update
	void Awake()
	{
		agent = GetComponent<BlobAgent>();
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
			// check if I can explode
			if (explosionArea.triggered)
			{
				var hitAgent = explosionArea.other.GetComponentInParent<Agent>();
				if (hitAgent != agent)
				{
					agent.Explode();
				}
			}
			// check if I can rush towards enemy
			else if (rushArea.triggered)
			{
				var hitAgent = rushArea.other.GetComponentInParent<Agent>();
				if (hitAgent != agent)
				{
					agent.Rush(movementDirection);
				}
			}
		}

		agent.UpdateAgent();
	}


}
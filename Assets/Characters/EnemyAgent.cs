using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Acting))]
public class EnemyAgent : MonoBehaviour
{
	Movement movement;
	Acting acting;

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
		movement = GetComponent<Movement>();
		acting = GetComponent<Acting>();
	}

	// Update is called once per frame
	void Update()
	{
		movement.TryClearInstructions();
		if (acting.busy)
			return;

		if (acting.CanAttack()){
			StartCoroutine(acting.Act(movement));
        }

		Vector3 direction = targetPoint.position - movement.body.position;
		Vector2 movementDirection = new Vector2(direction.x, direction.z);
		if (GoToTarget())
		{
			movementDirection = Vector2.ClampMagnitude(movementDirection, 1f);
			movement.Move(movementDirection);
		}
		movement.Turn(movementDirection);
	}


}

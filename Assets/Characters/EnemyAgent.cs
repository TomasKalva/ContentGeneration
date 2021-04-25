﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Fighting))]
public class EnemyAgent : MonoBehaviour
{
	Movement movement;
	Fighting fighting;

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
		fighting = GetComponent<Fighting>();
	}

	// Update is called once per frame
	void Update()
	{
		movement.TryClearInstructions();
		if (fighting.busy)
			return;

		if (fighting.CanAttack()){
			StartCoroutine(fighting.Attack());
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

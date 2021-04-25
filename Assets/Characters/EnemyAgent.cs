using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

[RequireComponent(typeof(Movement))]
public class EnemyAgent : MonoBehaviour
{
	Movement movement;

	[SerializeField]
	Transform targetPoint;

	// Start is called before the first frame update
	void Start()
	{
		movement = GetComponent<Movement>();
	}

	// Update is called once per frame
	void Update()
	{
		Vector3 direction = targetPoint.position - movement.body.position;
		Vector2 movementDirection = new Vector2(direction.x, direction.z);
		movement.TryClearInstructions();
		movement.PerformInstruction(new MoveInstruction(movementDirection));
	}
}

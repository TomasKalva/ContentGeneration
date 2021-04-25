using System.Collections;
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



	// Start is called before the first frame update
	void Start()
	{
		movement = GetComponent<Movement>();
		fighting = GetComponent<Fighting>();
	}

	// Update is called once per frame
	void Update()
	{
        if(!fighting.busy && fighting.CanAttack()){
			StartCoroutine(fighting.Attack());
        }

        if (!fighting.busy)
		{
			Vector3 direction = targetPoint.position - movement.body.position;
			Vector2 movementDirection = new Vector2(direction.x, direction.z);
			movement.TryClearInstructions();
			movement.PerformInstruction(new MoveInstruction(movementDirection));
		}
	}
}

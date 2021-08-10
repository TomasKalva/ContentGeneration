using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EmptyAgent))]
public class EmptyController : EnemyController<EmptyAgent>
{
	// Start is called before the first frame update
	void Awake()
	{
		agent = GetComponent<EmptyAgent>();
	}

    protected override void UpdateController(Vector2 movementDirection)
	{
	}
}
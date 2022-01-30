using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DogAgent))]
public class DogController : EnemyController<DogAgent>
{
	[SerializeField]
	public ColliderDetector dashForwardDetector;

	[SerializeField]
	public ColliderDetector slashDetector;

	private void Start()
	{
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DragonManAgent))]
public class DragonManController : EnemyController<DragonManAgent>
{
	[SerializeField]
	public ColliderDetector slashDetector;

	[SerializeField]
	public ColliderDetector castDetector;

	private void Start()
	{
	}
}
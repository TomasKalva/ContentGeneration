using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SculptureAgent))]
public class SculptureController : EnemyController<SculptureAgent>
{
	[SerializeField]
	ColliderDetector overheadArea;

	protected override void UpdateController(Vector2 movementDirection)
	{
		// check if my attack hits enemy
		if (overheadArea.Triggered)
		{
			var hitAgent = overheadArea.other.GetComponentInParent<Agent>();
			if (hitAgent != agent)
			{
				agent.DoubleSwipe();
			}
		}
	}
}
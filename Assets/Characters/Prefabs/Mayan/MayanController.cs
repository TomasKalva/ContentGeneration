using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MayanAgent))]
public class MayanController : EnemyController<MayanAgent>
{
	[SerializeField]
	ColliderDetector overheadDetector;

	private void Start()
	{
		var behaviors = agent.Behaviors;

		behaviors.AddBehavior(new TurnToTargetBehavior(10));
		behaviors.AddBehavior(new GoToTargetBehavior(5));
		behaviors.AddBehavior(new WaitForPlayer(10));
		behaviors.AddBehavior(new Awareness(10, new Vector2(3.0f, 5.0f)));

		behaviors.AddBehavior(new DetectorBehavior(agent.OverheadAttack, overheadDetector));
	}
}
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
		Behaviors.AddBehavior(new TurnToTargetBehavior(10));
		Behaviors.AddBehavior(new GoToTargetBehavior(5));
		Behaviors.AddBehavior(new WaitForPlayer(10));

		Behaviors.AddBehavior(new DetectorBehavior(agent.OverheadAttack, overheadDetector));
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SculptureAgent))]
public class SculptureController : EnemyController<SculptureAgent>
{
	[SerializeField]
	ColliderDetector leftWideDetector;
	
	[SerializeField]
	ColliderDetector rightWideDownDetector;

	[SerializeField]
	ColliderDetector doubleSwipeLeftDetector;

	[SerializeField]
	ColliderDetector doubleSwipeRightDetector;

	[SerializeField]
	ColliderDetector overheadDetector;

	[SerializeField]
	ColliderDetector groundSlamDetector;

    private void Start()
	{
		var behaviors = agent.Behaviors;

		behaviors.AddBehavior(new TurnToTargetBehavior(10));
		behaviors.AddBehavior(new GoToTargetBehavior(5));
		behaviors.AddBehavior(new WaitForPlayer(10));

		behaviors.AddBehavior(new DetectorBehavior(agent.WideAttack, leftWideDetector, rightWideDownDetector));
		behaviors.AddBehavior(new DetectorBehavior(agent.OverheadAttack, overheadDetector));
		behaviors.AddBehavior(new DetectorBehavior(agent.DoubleSwipe, doubleSwipeLeftDetector, doubleSwipeRightDetector));
		behaviors.AddBehavior(new DetectorBehavior(agent.GroundSlam, groundSlamDetector));

		agent.acting.MyReset();
	}
}
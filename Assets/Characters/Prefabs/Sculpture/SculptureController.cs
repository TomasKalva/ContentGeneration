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
		Behaviors.AddBehavior(new TurnToTargetBehavior(10));
		Behaviors.AddBehavior(new GoToTargetBehavior(5));
		Behaviors.AddBehavior(new WaitForPlayer(10));

		Behaviors.AddBehavior(new DetectorBehavior(agent.WideAttack, leftWideDetector, rightWideDownDetector));
		Behaviors.AddBehavior(new DetectorBehavior(agent.OverheadAttack, overheadDetector));
		Behaviors.AddBehavior(new DetectorBehavior(agent.DoubleSwipe, doubleSwipeLeftDetector, doubleSwipeRightDetector));
		Behaviors.AddBehavior(new DetectorBehavior(agent.GroundSlam, groundSlamDetector));
	}

    protected override void UpdateController(Vector2 movementDirection)
	{
	}
}
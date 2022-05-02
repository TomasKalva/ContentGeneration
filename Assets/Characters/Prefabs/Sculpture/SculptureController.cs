using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SculptureAgent))]
public class SculptureController : EnemyController<SculptureAgent>
{
	[SerializeField]
	public ColliderDetector leftWideDetector;
	
	[SerializeField]
	public ColliderDetector rightWideDownDetector;

	[SerializeField]
	public ColliderDetector doubleSwipeLeftDetector;

	[SerializeField]
	public ColliderDetector doubleSwipeRightDetector;

	[SerializeField]
	public ColliderDetector overheadDetector;

	[SerializeField]
	public ColliderDetector groundSlamDetector;

    public override void AddBehaviors(Behaviors behaviors)
	{
		behaviors.AddBehavior(new DetectorBehavior(agent.WideAttack, leftWideDetector, rightWideDownDetector));
		behaviors.AddBehavior(new DetectorBehavior(agent.OverheadAttack, overheadDetector));
		behaviors.AddBehavior(new DetectorBehavior(agent.DoubleSwipe, doubleSwipeLeftDetector, doubleSwipeRightDetector));
		behaviors.AddBehavior(new DetectorBehavior(agent.GroundSlam, groundSlamDetector));
	}
}
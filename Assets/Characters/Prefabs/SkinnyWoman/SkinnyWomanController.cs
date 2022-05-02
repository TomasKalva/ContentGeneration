using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnyWomanAgent))]
public class SkinnyWomanController : EnemyController<SkinnyWomanAgent>
{
	[SerializeField]
	public ColliderDetector rushForwardDetector;

	[SerializeField]
	public ColliderDetector castDetector;

    public override void AddBehaviors(Behaviors behaviors)
    {
        behaviors.AddBehavior(new DetectorBehavior(agent.RushForward, rushForwardDetector));
        behaviors.AddBehavior(new DetectorBehavior(agent.CastFireball, castDetector));
    }
}
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

    public override void AddBehaviors(Behaviors behaviors)
    {
        behaviors.AddBehavior(new DetectorBehavior(agent.DashForward, dashForwardDetector));
        behaviors.AddBehavior(new DetectorBehavior(agent.LeftSlash, slashDetector));
        behaviors.AddBehavior(new DetectorBehavior(agent.RightSlash, slashDetector));
    }
}
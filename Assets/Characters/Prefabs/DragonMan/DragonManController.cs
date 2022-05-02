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

	[SerializeField]
	public ColliderDetector spitFireDetector;

    public override void AddBehaviors(Behaviors behaviors)
    {
        behaviors.AddBehavior(new DetectorBehavior(agent.Slash, slashDetector));
        behaviors.AddBehavior(new DetectorBehavior(agent.FlapWings, castDetector));
        behaviors.AddBehavior(new DetectorBehavior(agent.SpitFire, spitFireDetector));
    }
}
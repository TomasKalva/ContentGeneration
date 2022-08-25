using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MayanAgent))]
public class MayanController : EnemyController<MayanAgent>
{
    /*
	[SerializeField]
	public ColliderDetector overheadDetector;

	[SerializeField]
	public ColliderDetector throwDetector;

	[SerializeField]
	public ColliderDetector swingDetector;

    public override void AddBehaviors(Behaviors behaviors)
    {
        behaviors.AddBehavior(new DetectorBehavior(agent.OverheadAttack, overheadDetector));
        behaviors.AddBehavior(new DetectorBehavior(agent.Throw, throwDetector));
        behaviors.AddBehavior(new DetectorBehavior(agent.LeftSwing, swingDetector));
    }*/
}
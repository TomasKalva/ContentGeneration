using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HumanAgent))]
public class HumanController : EnemyController<HumanAgent>
{
	[SerializeField]
	public ColliderDetector attackArea;

	public override void AddBehaviors(Behaviors behaviors)
	{
		behaviors.AddBehavior(new DetectorBehavior(agent.Attack, attackArea));
	}
}
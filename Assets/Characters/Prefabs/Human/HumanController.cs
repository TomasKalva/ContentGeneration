using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HumanAgent))]
public class HumanController : EnemyController<HumanAgent>
{
	[SerializeField]
	Detector attackArea;

	void Awake()
	{
		agent = GetComponent<HumanAgent>();
	}

	protected override void UpdateController(Vector2 movementDirection)
	{
		// check if my attack hits enemy
		if (attackArea.Triggered)
		{
			var hitAgent = attackArea.other.GetComponentInParent<Agent>();
			if (hitAgent != agent)
			{
				if (hitAgent.acting.ActiveAct.type == ActType.OFFENSIVE)
				{
					agent.Backstep();
				}
				else
				{
					agent.Attack();
				}
			}
		}
	}
}
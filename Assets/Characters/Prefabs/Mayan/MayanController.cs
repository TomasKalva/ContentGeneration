using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MayanAgent))]
public class MayanController : EnemyController<MayanAgent>
{
	[SerializeField]
	public ColliderDetector overheadDetector;

	[SerializeField]
	public ColliderDetector throwDetector;

	[SerializeField]
	public ColliderDetector swingDetector;

	private void Start()
	{
		/*var behaviors = agent.Behaviors;

		behaviors.AddBehavior(new TurnToTargetBehavior(10));
		behaviors.AddBehavior(new GoToTargetBehavior(10));
		behaviors.AddBehavior(new WaitForPlayer(10));
		behaviors.AddBehavior(new Awareness(10, new Vector2(3.0f, 5.0f), 5f, 15f));

		behaviors.AddBehavior(new DetectorBehavior(agent.OverheadAttack, overheadDetector));
		behaviors.AddBehavior(new DetectorBehavior(agent.Throw, throwDetector));
		behaviors.AddBehavior(new DetectorBehavior(agent.LeftSwing, leftSwingDetector));

		agent.acting.MyReset();*/
	}
}
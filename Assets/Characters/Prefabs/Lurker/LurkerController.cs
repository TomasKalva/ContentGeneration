using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LurkerAgent))]
public class LurkerController : EnemyController<LurkerAgent>
{
	[SerializeField]
	ColliderDetector burrowArea;

	[SerializeField]
	ColliderDetector unburrowArea;

	// Start is called before the first frame update
	void Awake()
	{
		agent = GetComponent<LurkerAgent>();
	}

    protected override void UpdateController(Vector2 movementDirection)
	{

		if (!agent.Burrowed && burrowArea.Triggered)
		{
			agent.Burrow();
		}
		else if (agent.Burrowed && unburrowArea.Triggered)
		{
			agent.Unburrow();
		}
	}
}
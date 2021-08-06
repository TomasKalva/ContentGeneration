using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CrossbowmanAgent))]
public class CrossbowmanController : EnemyController<CrossbowmanAgent>
{
	[SerializeField]
	ColliderDetector backstepArea;

	[SerializeField]
	RaycastDetector shootDetector;

	[SerializeField]
	ColliderDetector strafeArea;

	// Start is called before the first frame update
	void Awake()
	{
		agent = GetComponent<CrossbowmanAgent>();
	}

    protected override void UpdateController(Vector2 movementDirection)
	{
		if (backstepArea.Triggered)
		{
			agent.Backstep();
		}
		else if (strafeArea.Triggered)
		{
			agent.Strafe();
		}
		else if (shootDetector.Triggered)
        {
			agent.Shoot();
        }
	}
}
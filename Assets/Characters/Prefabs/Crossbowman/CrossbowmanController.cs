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

	// Start is called before the first frame update
	void Awake()
	{
		agent = GetComponent<CrossbowmanAgent>();
	}

    protected override void UpdateController(Vector2 movementDirection)
	{
		// check if I can explode
		if (backstepArea.Triggered)
		{
			agent.Strafe();
			/*var hitAgent = backstepArea.other.GetComponentInParent<Agent>();
			if (hitAgent != agent)
			{
				agent.Explode();
			}*/
		} else if (shootDetector.Triggered)
        {
			agent.Shoot();
        }
		// check if I can rush towards enemy
		/*else if (rushArea.Triggered)
		{
			var hitAgent = rushArea.other.GetComponentInParent<Agent>();
			if (hitAgent != agent)
			{
				agent.Rush(movementDirection);
			}
		}*/
	}
}
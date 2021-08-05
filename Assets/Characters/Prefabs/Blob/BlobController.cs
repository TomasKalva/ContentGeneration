using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BlobAgent))]
public class BlobController : EnemyController<BlobAgent>
{
	[SerializeField]
	ColliderDetector rushArea;

	[SerializeField]
	ColliderDetector explosionArea;

	// Start is called before the first frame update
	void Awake()
	{
		agent = GetComponent<BlobAgent>();
	}

    protected override void UpdateController(Vector2 movementDirection)
	{
		// check if I can explode
		if (explosionArea.Triggered)
		{
			var hitAgent = explosionArea.other.GetComponentInParent<Agent>();
			if (hitAgent != agent)
			{
				agent.Explode();
			}
		}
		// check if I can rush towards enemy
		else if (rushArea.Triggered)
		{
			var hitAgent = rushArea.other.GetComponentInParent<Agent>();
			if (hitAgent != agent)
			{
				agent.Rush(movementDirection);
			}
		}
	}
}
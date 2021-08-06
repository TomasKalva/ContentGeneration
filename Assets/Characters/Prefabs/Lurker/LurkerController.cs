using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LurkerAgent))]
public class LurkerController : EnemyController<LurkerAgent>
{
	[SerializeField]
	ColliderDetector unburrowArea;

	[SerializeField]
	RaycastDetector shockwaveDetector;

	// Start is called before the first frame update
	void Awake()
	{
		agent = GetComponent<LurkerAgent>();
	}

	bool WantsToUnburrow => unburrowArea.Triggered;


	protected override void UpdateController(Vector2 movementDirection)
	{
        if (WantsToUnburrow)
        {
            if (agent.Burrowed)
			{
				agent.Unburrow();
			}
            else
            {
				// move away from target
				var toTarget = TargetPoint - agent.transform.position;
				var toTarget2d = new Vector2(toTarget.x, toTarget.z).normalized;
				agent.Move(-toTarget2d);
            }
        }
        else
        {
			if (shockwaveDetector.Triggered)
			{
				if (!agent.Burrowed)
				{
					agent.Burrow();
				}
                else
				{
					agent.Shockwave();
				}
            }
            else if(DistanceToTarget >= shockwaveDetector.distance && agent.Burrowed)
            {
				// target is far away => unburrow to chase it
				agent.Unburrow();
            }
		}
	}
}
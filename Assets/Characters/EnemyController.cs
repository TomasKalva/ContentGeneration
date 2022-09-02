using ContentGeneration.Assets.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

[RequireComponent(typeof(Agent))]
public class EnemyController : MonoBehaviour
{
	Agent agent;

	void Awake()
	{
		agent = GetComponent<Agent>();
	}

    private void Start()
    {
		agent.acting.MyReset();
	}

    void FixedUpdate()
	{
		agent.StartReceivingControls();

		agent.Behaviors.UpdateBehavior(agent);

		agent.UpdateAgent();
	}
}

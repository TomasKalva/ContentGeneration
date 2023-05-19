﻿using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
	/// <summary>
	/// Controls enemy agents.
	/// </summary>
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
}
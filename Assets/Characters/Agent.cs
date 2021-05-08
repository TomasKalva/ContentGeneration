using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Acting))]
public class Agent : MonoBehaviour
{
    public Movement movement;
    public Acting acting;

	// Start is called before the first frame update
	void Start()
	{
		movement = GetComponent<Movement>();
		acting = GetComponent<Acting>();
	}

	public void UpdateAgent()
	{
		movement.TryClearInstructions();
	}
}

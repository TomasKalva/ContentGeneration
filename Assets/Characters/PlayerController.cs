using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

[RequireComponent(typeof(Agent))]
public class PlayerController : MonoBehaviour
{
	Agent agent;

	[SerializeField]
	Transform playerInputSpace;

    // Start is called before the first frame update
    void Start()
	{
		agent = GetComponent<Agent>();
	}

    // Update is called once per frame
    void Update()
	{
		agent.UpdateAgent();
		if (agent.acting.busy)
		{
			return;
		}

		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");
		playerInput = Vector2.ClampMagnitude(playerInput, 1f);
		if (playerInputSpace != null)
		{
			agent.movement.Move(playerInput);
		}
		else
		{
			Debug.LogError("Input space is null");
		}

        if (Input.GetMouseButtonDown(0))
        {
			StartCoroutine(agent.acting.Act(agent));
        }

		if (Input.GetButtonDown("Dodge"))
		{
			agent.movement.Dodge(20f);
		}
		if (Input.GetButtonDown("Roll"))
        {
			agent.movement.Roll(20f);
		}
	}
}

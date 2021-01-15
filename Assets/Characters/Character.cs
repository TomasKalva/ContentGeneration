using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MovingAgent;

[RequireComponent(typeof(MovingAgent))]
public class Character : MonoBehaviour
{
	MovingAgent agent;
	[SerializeField]
	Transform playerInputSpace;
	Dictionary<string, AgentInstruction> instructions;

    // Start is called before the first frame update
    void Start()
    {
		agent = GetComponent<MovingAgent>();
		instructions = new Dictionary<string, AgentInstruction>()
		{
			{"Jump", new JumpInstruction(15f) }
		};
    }

    // Update is called once per frame
    void Update()
	{
		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");
		playerInput = Vector2.ClampMagnitude(playerInput, 1f);
		agent.TryClearInstructions();
		if (playerInputSpace != null)
		{
			agent.PerformInstruction(new MoveInstruction(playerInput));
		}
		else
		{
			Debug.LogError("Input space is null");
		}

		foreach(var kvp in instructions)
        {
            if (Input.GetButtonDown(kvp.Key))
            {
				agent.PerformInstruction(kvp.Value);
            }
        }


		//desiredJump |= Input.GetButtonDown("Jump");
	}
}

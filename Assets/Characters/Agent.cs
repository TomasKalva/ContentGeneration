using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

[RequireComponent(typeof(Movement))]
public class Agent : MonoBehaviour
{
	Movement movement;
	[SerializeField]
	Transform playerInputSpace;
	Dictionary<string, AgentInstruction> instructions;

    // Start is called before the first frame update
    void Start()
    {
		movement = GetComponent<Movement>();
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
		movement.TryClearInstructions();
		if (playerInputSpace != null)
		{
			movement.PerformInstruction(new MoveInstruction(playerInput));
		}
		else
		{
			Debug.LogError("Input space is null");
		}

		foreach(var kvp in instructions)
        {
            if (Input.GetButtonDown(kvp.Key))
            {
				movement.PerformInstruction(kvp.Value);
            }
        }


		//desiredJump |= Input.GetButtonDown("Jump");
	}
}

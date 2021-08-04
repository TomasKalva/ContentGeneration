using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

[RequireComponent(typeof(HumanAgent))]
public class PlayerController : MonoBehaviour
{
	HumanAgent agent;

	[SerializeField]
	Transform playerInputSpace;

	Dictionary<string, bool> buttonDown;

	// Start is called before the first frame update
	void Awake()
	{
		agent = GetComponent<HumanAgent>();
		//Application.targetFrameRate = 60;

		buttonDown = new Dictionary<string, bool>()
		{
			{"Roll", false },
			{"Attack", false },
		};
	}

    void Update()
    {
		AddButtonsDown();
    }

	void AddButtonsDown()
    {
		foreach (var key in new List<string>(buttonDown.Keys))
		{
			buttonDown[key] |= Input.GetButtonDown(key);
		}
	}

	void ClearButtonsDown()
	{
		foreach (var key in new List<string>(buttonDown.Keys))
		{
			buttonDown[key] = false;
		}
	}

    // Update is called once per frame
    void FixedUpdate()
	{
		agent.StartReceivingControls();

		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");

		playerInput = Vector2.ClampMagnitude(playerInput, 1f);
		if (playerInputSpace != null)
		{
			agent.Move(playerInput);
		}
		else
		{
			Debug.LogError("Input space is null");
		}
		
		/*if (Input.GetMouseButtonDown(0))
		{
			agent.Shoot();
		}*/

		if (buttonDown["Roll"])
		{
			if( playerInput.sqrMagnitude > 0.001f)
			{
				agent.Roll(playerInput);
			}
            else
			{
				agent.Backstep();
			}
		}

		if (buttonDown["Attack"])
		{
			agent.Attack();
		}

		ClearButtonsDown();

		agent.UpdateAgent();

		agent.movement.MovementUpdate();

	}
}

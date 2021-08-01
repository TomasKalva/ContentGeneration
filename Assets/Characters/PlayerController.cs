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

    // Start is called before the first frame update
    void Awake()
	{
		agent = GetComponent<HumanAgent>();
	}

    // Update is called once per frame
    void Update()
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

		if (Input.GetButtonDown("Roll"))
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

		if (Input.GetButtonDown("Attack"))
		{
			agent.Attack();
		}

		agent.UpdateAgent();
	}
}

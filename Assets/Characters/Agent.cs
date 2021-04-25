using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Fighting))]
public class Agent : MonoBehaviour
{
	Movement movement;
	Fighting fighting;

	[SerializeField]
	Transform playerInputSpace;

    // Start is called before the first frame update
    void Start()
    {
		movement = GetComponent<Movement>();
		fighting = GetComponent<Fighting>();
    }

    // Update is called once per frame
    void Update()
	{
		movement.TryClearInstructions();

		if (fighting.busy)
		{
			return;
		}

		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");
		playerInput = Vector2.ClampMagnitude(playerInput, 1f);
		if (playerInputSpace != null)
		{
			movement.Move(playerInput);
		}
		else
		{
			Debug.LogError("Input space is null");
		}

        if (Input.GetMouseButtonDown(0))
        {
			StartCoroutine(fighting.Attack());
        }

        if (Input.GetButtonDown("Jump"))
        {
			movement.Jump(15f);
		}
	}
}

using Assets;
using ContentGeneration.Assets.UI;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Movement;

[RequireComponent(typeof(HumanAgent))]
public class PlayerController : MonoBehaviour
{
	HumanAgent agent;

	PlayerCharacterState PlayerCharacterState => (PlayerCharacterState)agent.CharacterState;

	World world;

	[SerializeField]
	Transform playerInputSpace;

	Dictionary<string, bool> buttonDown;

	bool respawned;

	// Start is called before the first frame update
	void Awake()
	{
		agent = GetComponent<HumanAgent>();
		world = GameObject.Find("World").GetComponent<World>();

		Application.targetFrameRate = 80;

		buttonDown = new Dictionary<string, bool>()
		{
			{"Roll", false },
			{"Attack", false },
			{"Interact", false },
			{"Suicide", false },
		};
	}

    private void Start()
    {
		var camera = GameObject.Find("Main Camera");
		playerInputSpace = camera.transform;
		agent.movement.playerInputSpace = playerInputSpace;
		camera.GetComponent<OrbitCamera>().focus = transform;

		var viewModel = camera.GetComponent<ViewModel>();
		if (viewModel.PlayerState != null)
        {
			// already spawned before
			agent.GetComponent<CharacterRef>().CharacterState = viewModel.PlayerState;
			viewModel.PlayerState.Reset();
        }
        else
		{
			// first spawn
			viewModel.PlayerState = PlayerCharacterState;
			PlayerCharacterState.SpawnPoint = GameObject.FindGameObjectWithTag("DefaultSpawnPoint").GetComponent<Bonfire>();
		}
	}

    void Update()
    {
		AddButtonsDown();

		PlayerCharacterState.CurrentInteractiveObject = world.ObjectsCloseTo(transform.position, 5f).FirstOrDefault();

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
        if (agent.CharacterState.Dead)
		{
			if (PlayerCharacterState.SpawnPoint && !respawned)
			{
				PlayerCharacterState.SpawnPoint.SpawnPlayer();
				respawned = true;
			}
			//return;
		}

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

		var interactiveObject = PlayerCharacterState.CurrentInteractiveObject;

		if (buttonDown["Interact"] && interactiveObject != null)
		{
			interactiveObject.Interact(agent);
		}

		if (buttonDown["Suicide"])
		{
			agent.CharacterState.Health -= 1000f;
		}

		ClearButtonsDown();

		agent.UpdateAgent();

	}
}

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
	HumanAgent myAgent;

	PlayerCharacterState PlayerCharacterState => (PlayerCharacterState)myAgent.CharacterState;

	OrbitCamera orbitCamera;

	World world;

	Agent lockOnTarget;

	[SerializeField]
	Transform playerInputSpace;


	Dictionary<string, bool> buttonDown;

	bool respawned;

	[SerializeField]
	float interactionDistance = 1f;

	// Start is called before the first frame update
	void Awake()
	{
		myAgent = GetComponent<HumanAgent>();
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
		myAgent.movement.playerInputSpace = playerInputSpace;
		orbitCamera = camera.GetComponent<OrbitCamera>();
		orbitCamera.DefaultCamUpdater = orbitCamera.FocusPlayer(transform);
		lockOnTarget = null;

		var viewModel = camera.GetComponent<ViewModel>();
		if (viewModel.PlayerState != null)
        {
			// already spawned before
			myAgent.GetComponent<CharacterRef>().CharacterState = viewModel.PlayerState;
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
		if (!PlayerCharacterState.InteractingWithUI)
		{
			AddButtonsDown();
		}

		PlayerCharacterState.CurrentInteractiveObject = world.ObjectsCloseTo(transform.position, interactionDistance).FirstOrDefault();

		UpdateLockOn();

	}

	#region Lock on

	float switchTargetTimer;

	bool CanSwitchTarget()
    {
		return switchTargetTimer <= 0f;
    }

	void UpdateLockOn()
    {
		// Switch between free and locked camera
		if (Input.GetButtonDown("LockOn"))
		{
			if (lockOnTarget)
			{
				LockOn(null);
			}
			else
			{
				LockOn(LockOnTarget(myAgent, orbitCamera.transform));
			}
		}

		// Switch targets
		switchTargetTimer -= Time.deltaTime;
		if (lockOnTarget && CanSwitchTarget())
		{
			Vector2 cameraInput = new Vector2(
					Input.GetAxis("Horizontal Camera"),
					-Input.GetAxis("Vertical Camera")
				);
			if (cameraInput.magnitude > 0.1f)
			{
				var newLockOnTarget = SwitchLockOnTarget(myAgent, lockOnTarget, cameraInput);
				if (newLockOnTarget != null)
				{
					LockOn(newLockOnTarget);
					switchTargetTimer = 0.5f;
				}
			}
		}

		// Remove dead target from ui
		if (PlayerCharacterState.TargetedEnemy != null && PlayerCharacterState.TargetedEnemy.agent == null)
		{
			LockOn(null);
		}
	}

    /// <summary>
    /// Lock on is removed if agent is null.
    /// </summary>
    void LockOn(Agent selectedAgent)
	{
		if (selectedAgent != null)
		{
			lockOnTarget = selectedAgent;
			orbitCamera.CamUpdater = orbitCamera.FocusOnEnemy(myAgent.transform, lockOnTarget?.transform);
			PlayerCharacterState.TargetedEnemy = lockOnTarget?.CharacterState;
		}
		else
		{
			orbitCamera.CamUpdater = orbitCamera.FocusPlayer(myAgent.transform);
			lockOnTarget = null;
			PlayerCharacterState.TargetedEnemy = null;
		}
	}

	Agent LockOnTarget(Agent player, Transform cam)
	{
		return world.Agents.Where(agent => agent != player).ArgMax(agent => Vector3.Dot((agent.transform.position - cam.position).normalized, cam.forward));
	}

	Agent SwitchLockOnTarget(Agent player, Agent selected, Vector2 screenDirection)
	{
		return world.Agents.Where(agent => agent != player &&
									Vector2.Dot(screenDirection, agent.CharacterState.ScreenPos - selected.CharacterState.ScreenPos) > 0f &&
									ExtensionMethods.PointInDirection(orbitCamera.transform.position, orbitCamera.transform.forward, agent.transform.position))
					.ArgMin(agent => (selected.CharacterState.ScreenPos - agent.CharacterState.ScreenPos).sqrMagnitude);
	}
#endregion

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

	int physicsSteps = 0;
	float time = 0f;

    // Update is called once per frame
    void FixedUpdate()
	{
        if (myAgent.CharacterState.Dead)
		{
			if (PlayerCharacterState.SpawnPoint && !respawned)
			{
				PlayerCharacterState.SpawnPoint.SpawnPlayer();
				respawned = true;
			}
			//return;
		}

		myAgent.StartReceivingControls();

		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");

		playerInput = Vector2.ClampMagnitude(playerInput, 1f);
		if (playerInputSpace != null)
		{
			myAgent.Move(playerInput);
		}
		else
		{
			Debug.LogError("Input space is null");
		}

		if (buttonDown["Roll"])
		{
			if( playerInput.sqrMagnitude > 0.001f)
			{
				myAgent.Roll(playerInput);
			}
            else
			{
				myAgent.Backstep();
			}
		}

		if (buttonDown["Attack"])
		{
			myAgent.Attack();
		}

		var interactiveObject = PlayerCharacterState.CurrentInteractiveObject;

		if (buttonDown["Interact"] && interactiveObject != null)
		{
			interactiveObject.Interact(myAgent);
		}

		if (buttonDown["Suicide"])
		{
			myAgent.CharacterState.Health -= 1000f;
		}

		ClearButtonsDown();

		myAgent.UpdateAgent();

	}
}

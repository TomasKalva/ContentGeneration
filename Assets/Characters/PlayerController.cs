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

	Reality reality;

	Agent lockOnTarget;

	[SerializeField]
	Transform playerInputSpace;

	Dictionary<string, bool> buttonDown;

	bool respawned;

	[SerializeField]
	float interactionDistance = 1f;

	Module previousModule;
	Module currentModule;

	// Start is called before the first frame update
	void Awake()
	{
		myAgent = GetComponent<HumanAgent>();
		world = GameObject.Find("World").GetComponent<World>();
		reality = GameObject.Find("Reality").GetComponent<Reality>();

		Application.targetFrameRate = 80;

		buttonDown = new Dictionary<string, bool>()
		{
			{"Roll", false },
			{"Attack", false },
			{"Interact", false },
			{"Suicide", false },
			{"UseItem", false },
		};
	}

    private void Start()
	{
		// lock the cursor
		Cursor.lockState = CursorLockMode.Locked;

		var camera = GameObject.Find("Main Camera");
		playerInputSpace = camera.transform;
		//myAgent.movement.playerInputSpace = playerInputSpace;
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
			//PlayerCharacterState.SpawnPoint = GameObject.FindGameObjectWithTag("DefaultSpawnPoint").GetComponent<Bonfire>();
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

	MovementConstraint turnToTarget;

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
			if (cameraInput.magnitude > 0.5f)
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
			// Lock
			lockOnTarget = selectedAgent;
			orbitCamera.CamUpdater = orbitCamera.FocusOnEnemy(myAgent.transform, lockOnTarget);
			PlayerCharacterState.TargetedEnemy = lockOnTarget != null ? lockOnTarget.CharacterState : null;
			
			// Remove the constraint in case it wasn't removed properly with unlock
			myAgent.movement.Constraints.Remove(turnToTarget);

			turnToTarget = new TurnToTransform(lockOnTarget.transform);
			myAgent.movement.Constraints.Add(turnToTarget);
		}
		else
		{
			// Unlock
			orbitCamera.CamUpdater = orbitCamera.FocusPlayer(myAgent.transform);
			lockOnTarget = null;
			PlayerCharacterState.TargetedEnemy = null;
			myAgent.movement.Constraints.Remove(turnToTarget);
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
									ExtensionMethods.IsPointInDirection(orbitCamera.transform.position, orbitCamera.transform.forward, agent.transform.position))
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

    // Update is called once per frame
    void FixedUpdate()
	{
		// Death and respawning
        if (myAgent.CharacterState.Dead && !respawned)
		{
			world.OnPlayerDeath();
			respawned = true;
			/*if (PlayerCharacterState.SpawnPoint && !respawned)
			{
				PlayerCharacterState.SpawnPoint.SpawnPlayer();
				respawned = true;
			}*/
		}

		UpdateCurrentModule();

		myAgent.StartReceivingControls();

		Vector2 playerInput;
		playerInput.x = Input.GetAxisRaw("Horizontal");
		playerInput.y = Input.GetAxisRaw("Vertical");

		playerInput = Vector2.ClampMagnitude(playerInput, 1f);

		var worldInputDirection = InputToWorld(playerInput).XZ().normalized;
		if (playerInputSpace != null)
		{
			if (lockOnTarget == null)
			{
				myAgent.Run(worldInputDirection);
			}
            else
			{
				myAgent.RunLockedOn(worldInputDirection);
			}
		}
		else
		{
			Debug.LogError("Input space is null");
		}

		if (buttonDown["Roll"])
		{
			if( playerInput.sqrMagnitude > 0.001f)
			{
				myAgent.Roll(worldInputDirection);
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

		if (buttonDown["UseItem"])
		{
			PlayerCharacterState.PlayerInventory.UseItem();
		}

		ClearButtonsDown();

		myAgent.UpdateAgent();

	}

	void UpdateCurrentModule()
    {
		var grid = reality.ModuleGrid;
		var gridPos = grid.WorldToGrid(transform.position);

		previousModule = currentModule;
		currentModule = grid[gridPos];
		/*if (currentModule != null)
		{
			Debug.Log(gridPos);
			Debug.Log(currentModule.GetProperty<AreaModuleProperty>().Area.Name);
		}*/
    }

	Vector3 InpForward
	{
		get
		{
			if (playerInputSpace != null)
			{
				return playerInputSpace.forward;
			}
			else
			{
				return Vector3.forward;
			}
		}
	}

	Vector3 InpRight
	{
		get
		{
			if (playerInputSpace != null)
			{
				return playerInputSpace.right;
			}
			else
			{
				return Vector3.right;
			}
		}
	}

	Vector3 InpForwardHoriz
	{
		get
		{
			return ExtensionMethods.ProjectDirectionOnPlane(InpForward, Vector3.up);
		}
	}

	Vector3 InpRightHoriz
	{
		get
		{
			return ExtensionMethods.ProjectDirectionOnPlane(InpRight, Vector3.up);
		}
	}

	public Vector3 InputToWorld(Vector2 inputVec)
	{
		return inputVec.x * InpRightHoriz + inputVec.y * InpForwardHoriz;
	}
}

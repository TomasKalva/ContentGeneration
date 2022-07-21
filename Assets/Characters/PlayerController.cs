using Assets;
using ContentGeneration.Assets.UI;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using ShapeGrammar;
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

	[SerializeField]
	Transform orbitCameraFocusPoint;

	OrbitCamera orbitCamera;

	public World World { get; set; }

	//Reality reality;

	Agent lockOnTarget;

	SpacePartitioning spacePartitioning;

	[SerializeField]
	Transform playerInputSpace;

	Dictionary<string, bool> buttonDown;

	bool respawned;

	[SerializeField]
	float interactionDistance = 1f;

	[SerializeField]
	Libraries libraries;

	// Start is called before the first frame update
	void Awake()
	{
		myAgent = GetComponent<HumanAgent>();
		//world = GameObject.Find("World").GetComponent<World>();
		//reality = GameObject.Find("Reality").GetComponent<Reality>();

		Application.targetFrameRate = 80;

		buttonDown = new Dictionary<string, bool>()
		{
			{"Roll", false },
			{"Attack", false },
			{"Interact", false },
			{"Suicide", false },
			{"UseItem", false },
			{"Option1", false },
			{"Option2", false },
			{"Option3", false },
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
		orbitCamera.DefaultCamUpdater = orbitCamera.FocusPlayer(orbitCameraFocusPoint);
		lockOnTarget = null;

		var viewModel = camera.GetComponent<ViewModel>();
		myAgent.CharacterState = viewModel.PlayerState;
		viewModel.PlayerState.Reset();

		cameraInputs = new Queue<Vector2>();

		myAgent.CharacterState.SetItemToSlot(SlotType.LeftWeapon, libraries.Items.MayanKnife());
		myAgent.CharacterState.SetItemToSlot(SlotType.RightWeapon, libraries.Items.Katana());
		//myAgent.CharacterState.SetItemToSlot(SlotType.Active, libraries.Items.FreeWill());
	}

    void Update()
    {
		if (!PlayerCharacterState.InteractingWithUI)
		{
			AddButtonsDown();
		}

		PlayerCharacterState.CurrentInteractiveObjectState = World.ObjectsCloseTo(transform.position, interactionDistance).FirstOrDefault();

		UpdateLockOn();

	}

	#region Lock on

	float switchTargetTimer;

	Queue<Vector2> cameraInputs;

	MovementConstraint turnToTarget;

	float maxDistance = 15f;

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
			cameraInputs.Enqueue(cameraInput);
			if(cameraInputs.Count >= 30)
            {
				cameraInputs.Dequeue();
			}

			if (cameraInputs.Average(x => x.magnitude) > 35.0f)
			{
				cameraInputs.Clear();
				var newLockOnTarget = SwitchLockOnTarget(myAgent, lockOnTarget, cameraInput);
				if (newLockOnTarget != null)
				{
					LockOn(newLockOnTarget);
					switchTargetTimer = 0.5f;
				}
			}
		}

		// Remove dead target from ui
		if (PlayerCharacterState.TargetedEnemy != null && (PlayerCharacterState.TargetedEnemy.Agent == null || !(orbitCamera.CamUpdater is OrbitCamera.LockOn)))
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
			orbitCamera.CamUpdater = orbitCamera.FocusOnEnemy(myAgent.transform, lockOnTarget, maxDistance);
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

	IEnumerable<Agent> LockableTargets(Agent player)
    {
		return World.AliveEnemies.SelectNN(e => e.Agent).Where(agent => agent != player && (agent.transform.position - player.transform.position).sqrMagnitude < maxDistance * maxDistance);

	}

	Agent LockOnTarget(Agent player, Transform cam)
	{
		return LockableTargets(player)
							.ArgMax(agent => Vector3.Dot((agent.transform.position - cam.position).normalized, cam.forward));
	}

	Agent SwitchLockOnTarget(Agent player, Agent selected, Vector2 screenDirection)
	{
		return LockableTargets(player).Where(agent => 
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
			World.OnPlayerDeath();
			respawned = true;
			/*if (PlayerCharacterState.SpawnPoint && !respawned)
			{
				PlayerCharacterState.SpawnPoint.SpawnPlayer();
				respawned = true;
			}*/
		}

		//UpdateCurrentModule();

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

		var interactiveObject = PlayerCharacterState.CurrentInteractiveObjectState;

		if(interactiveObject != null)
		{
			if (buttonDown["Interact"])
			{
				interactiveObject.Interact(myAgent);
            }
            else
            {
				var buttonIndex = buttonDown["Option1"] ? 0 :
									buttonDown["Option2"] ? 1 :
										buttonDown["Option3"] ? 2 : -1;
				interactiveObject.OptionalInteract(myAgent, buttonIndex);
			}
		}

		if (buttonDown["Suicide"])
		{
			myAgent.CharacterState.Health -= 1000f;
		}

		// Interact and UseItem are the same button
		if (interactiveObject == null && buttonDown["UseItem"])
		{
			myAgent.UseItem();
		}

		ClearButtonsDown();

		myAgent.UpdateAgent();

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
﻿using OurFramework.UI.Util;
using OurFramework.Gameplay.State;
using OurFramework.LevelDesignLanguage;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OurFramework.Util;
using OurFramework.Environment.ShapeGrammar;

namespace OurFramework.Gameplay.RealWorld
{
	/// <summary>
	/// Handles player's input.
	/// </summary>
    [RequireComponent(typeof(HumanAgent))]
	public class PlayerController : MonoBehaviour
	{
		HumanAgent myAgent;

		PlayerCharacterState PlayerCharacterState => (PlayerCharacterState)myAgent.CharacterState;

		[SerializeField]
		Transform orbitCameraFocusPoint;

		OrbitCamera orbitCamera;

		SpacePartitioning spacePartitioning;

		Transform playerInputSpace;

		/// <summary>
		/// The input buttons which are used.
		/// </summary>
		Dictionary<string, bool> buttonDown;

		[SerializeField]
		float interactionDistance = 1f;

		// Start is called before the first frame update
		void Awake()
		{
			myAgent = GetComponent<HumanAgent>();

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
			orbitCamera = camera.GetComponent<OrbitCamera>();
			orbitCamera.DefaultCamUpdater = orbitCamera.FocusPlayer(orbitCameraFocusPoint);

			var viewModel = GameViewModel.ViewModel;
			myAgent.CharacterState = viewModel.PlayerState;
			viewModel.PlayerState.Reset();
		}

		void Update()
		{
			if (!PlayerCharacterState.InteractingWithUI)
			{
				AddButtonsDown();
			}

			PlayerCharacterState.CurrentInteractiveObjectState = PlayerCharacterState.World.ObjectsCloseTo(transform.position, interactionDistance).FirstOrDefault();

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
			myAgent.StartReceivingControls();

			Vector2 playerInput;
			playerInput.x = Input.GetAxisRaw("Horizontal");
			playerInput.y = Input.GetAxisRaw("Vertical");

			playerInput = Vector2.ClampMagnitude(playerInput, 1f);

			var worldInputDirection = InputToWorld(playerInput).XZ().normalized;
			if (playerInputSpace != null)
			{
				myAgent.Run(worldInputDirection);
			}
			else
			{
				Debug.LogError("Input space is null");
			}

			if (buttonDown["Roll"])
			{
				if (playerInput.sqrMagnitude > 0.001f)
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

			if (interactiveObject != null)
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

			if (buttonDown["UseItem"])
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

	/// <summary>
	/// Used for despawning enemies in far areas.
	/// </summary>
	class SpacePartitioning
	{
		HashSet<Area> activeAreas;
		TraversabilityGraph traversabilityGraph;
		bool initialized = false; // initialize during first update so that UI can get reference to this enemy

		public SpacePartitioning(TraversabilityGraph traversabilityGraph)
		{
			this.traversabilityGraph = traversabilityGraph;
		}

		public void Initialize()
		{
			traversabilityGraph.Areas.ForEach(area => area.Disable());
			activeAreas = new HashSet<Area>();
		}

		public void Update(Node activeNode)
		{
			if (!initialized)
			{
				Initialize();
				initialized = true;
			}

			if (activeNode == null)
			{
				// Player is outside of all areas
				return;
			}

			var activeArea = traversabilityGraph.TryGetArea(activeNode);
			if (activeArea == null)
			{
				// Player is in a non-traversable area
				return;
			}

			var currentAreas = traversabilityGraph.Neighbors(activeArea).Append(activeArea).ToList();

			// remove no longer active
			var notActive = activeAreas.Where(area => traversabilityGraph.Distance(area, activeArea, int.MaxValue) > 1).ToList();
			notActive.ForEach(area => area.Disable());
			notActive.ForEach(area => activeAreas.Remove(area));

			// add newly active
			var newActive = currentAreas.Except(activeAreas);
			newActive.ForEach(area => area.Enable());
			newActive.ForEach(area => activeAreas.Add(area));
		}
	}
}
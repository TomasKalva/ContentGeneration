using OurFramework.UI.Util;
using OurFramework.Gameplay.Data;
using OurFramework.LevelDesignLanguage;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
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
}

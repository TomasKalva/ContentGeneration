﻿using Animancer;
using UnityEngine;
using OurFramework.Gameplay.State;
using OurFramework.Util;

namespace OurFramework.Gameplay.RealWorld
{
	/// <summary>
	/// Real world body of a character. Defines its movement, acting,
	/// animations and behaviours.
	/// </summary>
    [RequireComponent(typeof(Movement))]
	[RequireComponent(typeof(Acting))]
	[RequireComponent(typeof(AnimancerComponent))]
	public class Agent : MonoBehaviour
	{
		public Movement movement;
		public Acting acting;
		public AnimancerComponent animancerAnimator;
		public Renderer myRenderer;

		CharacterState _characterState;
		public CharacterState CharacterState
		{
			get => _characterState;
			set
			{
				_characterState = value;
				_characterState.Agent = this;
			}
		}

		public Behaviors Behaviors { get; set; }

		public bool CanMove { get; set; } = true;

		bool died;

		[SerializeField]
		float centerOffset = 0f;

		public float CenterOffset => centerOffset;

		[SerializeField]
		float uiOffset = 1f;

		public float UIOffset => uiOffset;

		[SerializeField]
		AgentStateMaterials stateMaterials;

		AgentState _state;
		public AgentState State
		{
			get => _state;
			set
			{
				_state = value;
				GoToState(value);
			}
		}



		[SerializeField]
		WeaponSlot leftWeaponSlot;

		[SerializeField]
		WeaponSlot rightWeaponSlot;

		[SerializeField]
		AccessorySlot leftWristSlot;

		[SerializeField]
		AccessorySlot rightWristSlot;

		[SerializeField]
		AccessorySlot headSlot;

		EquipmentSlot[] slots;

		void Awake()
		{
			movement = GetComponent<Movement>();
			acting = GetComponent<Acting>();
			animancerAnimator = GetComponent<AnimancerComponent>();
			myRenderer = GetComponentInChildren<Renderer>();
			slots = new EquipmentSlot[]
			{
				leftWeaponSlot,
				rightWeaponSlot,
				leftWristSlot,
				rightWristSlot,
				headSlot,
			};
			Behaviors = new Behaviors();
		}

		private void Start()
		{
			slots.SelectNN(slot => slot).ForEach(slot => slot.World = CharacterState.World);

			SynchronizeWithState(CharacterState);
			acting.UseItem.Inventory = CharacterState.Inventory;
		}

		public void StartReceivingControls()
		{
			movement.ResetDesiredValues();
		}

		public void GoToState(AgentState phase)
		{
			if (myRenderer == null)
			{
				Debug.LogError($"Missing renderer");
				return;
			}
			if (stateMaterials.materials.Length < 4)
			{
				Debug.LogError($"Missing state materials");
				return;
			}

			var materialIndex = phase == AgentState.PREPARE ? 1 :
								phase == AgentState.DAMAGE ? 2 :
								phase == AgentState.RESTORE ? 3 : 0;
			//myRenderer.material = stateMaterials.materials[materialIndex];
		}

		public void UpdateAgent()
		{
			if (CharacterState.Dead)
			{
				if (!died)
				{
					died = true;
					CharacterState.TryDie();
				}
			}

			acting.Act();

			movement.MovementUpdate();
			CharacterState.Update();
		}

		public void Run(Vector2 direction)
		{
			if (!CanMove)
			{
				Debug.LogError("Trying to move when CanMove is false!");
				return;
			}

			if (direction.sqrMagnitude > 0.0001f && !acting.Busy)
			{
				var run = acting.SelectAct("Run") as Move;
				run.Direction = direction;
				run.SetDirection = true;
			}
		}

		public void WalkBack(Vector2 direction)
		{
			if (!CanMove)
			{
				Debug.LogError("Trying to move when CanMove is false!");
				return;
			}

			if (direction.sqrMagnitude > 0.0001f && !acting.Busy)
			{
				var walkBack = acting.SelectAct("WalkBack") as MoveBack;
				walkBack.Direction = direction;
				walkBack.SetDirection = true;
			}
		}

		public void Walk(Vector2 direction)
		{
			if (!CanMove)
			{
				Debug.LogError("Trying to move when CanMove is false!");
				return;
			}

			if (direction.sqrMagnitude > 0.0001f && !acting.Busy)
			{
				var walk = acting.SelectAct("Walk") as Move;
				walk.Direction = direction;
				walk.SetDirection = true;
			}
		}

		public void UseItem()
		{
			var useItem = acting.SelectAct("UseItem") as UseItem;
		}

		public void SynchronizeWithState(CharacterState state)
		{
			var inventory = state.Inventory;

			var leftWeaponItem = inventory.LeftWeapon.Item;
			if (leftWeaponSlot != null)
			{
				leftWeaponSlot.Equipment = (leftWeaponItem as WeaponItem)?.Weapon;
			}

			var rightWeaponItem = inventory.RightWeapon.Item;
			if (rightWeaponSlot != null)
			{
				rightWeaponSlot.Equipment = (rightWeaponItem as WeaponItem)?.Weapon;
			}


			var leftWristItem = inventory.LeftWrist.Item;
			if (leftWristSlot != null)
			{
				leftWristSlot.Equipment = (leftWristItem as AccessoryItem)?.Accessory;
			}

			var rightWristItem = inventory.RightWrist.Item;
			if (rightWristSlot != null)
			{
				rightWristSlot.Equipment = (rightWristItem as AccessoryItem)?.Accessory;
			}

			var headItem = inventory.Head.Item;
			if (headSlot != null)
			{
				headSlot.Equipment = (headItem as AccessoryItem)?.Accessory;
			}

			var skinItem = (inventory.Skin.Item as MaterialItem);
			if (myRenderer != null)
			{
				if (skinItem != null)
				{
					myRenderer.sharedMaterial = skinItem.Material;
				}
				else
				{
					// Set some default material
					myRenderer.sharedMaterial = stateMaterials.materials[0];
				}
			}


			state.Stats.Update();

			CharacterState.viewCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
		}

		public void MyReset()
		{
			died = false;
		}

		public Vector3 GetGroundPosition()
		{
			return transform.position;
		}

		public Vector3 GetRightHandPosition()
		{
			return rightWeaponSlot.transform.position;
		}

		public Vector3 GetLeftHandPosition()
		{
			return leftWeaponSlot.transform.position;
		}

		public void Turn(Vector2 direction)
		{
			if (direction.sqrMagnitude > 0.0001f)
			{
				movement.Turn(direction);
			}
		}

		public virtual void PickUpItem(PhysicalItemState physicalItem)
		{
			Debug.Log($"Picking up item {physicalItem.Item.Name}");
			var pickUpItem = acting.SelectAct("PickUpItem") as PickUpItem;
			if (pickUpItem)
			{
				pickUpItem.PhysicalItem = physicalItem;
			}
		}

		public virtual void Stagger()
		{
			acting.ForceIntoAct(acting.Staggered);
		}
	}
}
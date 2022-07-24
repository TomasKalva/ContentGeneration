using Animancer;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public class BoolState
{
	public bool prev;
	public bool curr;

	public void SetNext(bool next)
    {
		prev = curr;
		curr = next;
	}

	public void Set(bool next)
	{
		curr = next;
	}

	public bool TrueNow()
    {
		return curr && !prev;
	}

	public bool FalseNow()
	{
		return !curr && prev;
	}


}*/

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Acting))]
[RequireComponent(typeof(AnimancerComponent))]
public class Agent : MonoBehaviour
{
	public Movement movement;
	public Acting acting;
	public Animator animator;
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

	// Start is called before the first frame update
	void Awake()
	{
		movement = GetComponent<Movement>();
		acting = GetComponent<Acting>();
		animator = GetComponent<Animator>();
		animancerAnimator = GetComponent<AnimancerComponent>();
	}

    private void Start()
    {
		SynchronizeWithState(CharacterState);
		acting.UseItem.Inventory = CharacterState.Inventory;
	}

    public void StartReceivingControls()
	{
		movement.ResetDesiredValues();
	}

	public void GoToState(AgentState phase)
    {
		if(myRenderer == null)
		{
			Debug.LogError($"Missing renderer");
			return;
        }
		if(stateMaterials.materials.Length < 4)
        {
			Debug.LogError($"Missing state materials");
			return;
        }

		var materialIndex = phase == AgentState.PREPARE ? 1 :
							phase == AgentState.DAMAGE ? 2 :
							phase == AgentState.RESTORE ? 3 : 0;
		myRenderer.material = stateMaterials.materials[materialIndex];
    }

	public void UpdateAgent()
	{
		if(CharacterState.Dead)
        {
            if (!died)
			{
				CharacterState.Die();
				died = true;
			}
			return;
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

	/// <summary>
	/// Todo: turn this into movement constraint.
	/// </summary>
	public void RunLockedOn(Vector2 direction)
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
			run.SetDirection = false;
		}
	}

	public void SynchronizeWithState(CharacterState state)
    {
		var inventory = state.Inventory;

		var leftWeaponItem = inventory.LeftWeaponSlot.Item;
		if(leftWeaponSlot != null)
		{
			leftWeaponSlot.Weapon = (leftWeaponItem as WeaponItem)?.Weapon;
		}

		var rightWeaponItem = inventory.RightWeaponSlot.Item;
		if(rightWeaponSlot != null)
		{
			rightWeaponSlot.Weapon = (rightWeaponItem as WeaponItem)?.Weapon;
		}

		state.Stats.Update();

		CharacterState.viewCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
	}

	public Vector3 GetGroundPosition()
    {
		return transform.position;
	}

	public Vector3 GetRightHandPosition()
	{
		return rightWeaponSlot.transform.position;
	}

	public void Turn(Vector2 direction)
	{
		if (direction.sqrMagnitude > 0.0001f/* && !acting.Busy*/)
		{
			movement.Turn(direction);
		}
	}

	public virtual void Die()
    {
		CharacterState.World.RemoveEnemy(CharacterState);
		CharacterState.OnDeath();
		//GameViewModel.ViewModel.Enemies.Remove(CharacterState);
		//Destroy(gameObject, 1f);
		enabled = false;
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
		//acting.Staggered.PushForce = pushForce;
		acting.ForceIntoAct(acting.Staggered);
	}
}

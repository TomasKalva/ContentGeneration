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
	public CharacterRef characterRef;
	public Renderer renderer;

	public CharacterState CharacterState => characterRef.CharacterState;

	public bool CanMove { get; set; } = true;

	/// <summary>
	/// Used for reseting animation back to idle.
	/// </summary>
	int stepsSinceMoved;

	bool died;


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

	// Start is called before the first frame update
	void Awake()
	{
		movement = GetComponent<Movement>();
		acting = GetComponent<Acting>();
		animator = GetComponent<Animator>();
		animancerAnimator = GetComponent<AnimancerComponent>();
		characterRef = GetComponent<CharacterRef>();
	}

    public void StartReceivingControls()
	{
		movement.ResetDesiredValues();
	}

	public void GoToState(AgentState phase)
    {
		if(renderer == null)
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
		renderer.material = stateMaterials.materials[materialIndex];
    }

	public void UpdateAgent()
	{
		if(CharacterState.Dead)
        {
            if (!died)
			{
				Die();
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

	public void Turn(Vector2 direction)
	{
		if (direction.sqrMagnitude > 0.0001f && !acting.Busy)
		{
			movement.Turn(direction);
		}
	}

	protected void ResetState()
	{
		animator.SetBool("IsMoving", false);
	}

	public virtual void Die()
    {
		GameViewModel.ViewModel.Enemies.Remove(CharacterState);
		Destroy(gameObject, 1f);
		enabled = false;
    }

	public virtual void PickUpItem(PhysicalItem physicalItem)
    {
		Debug.Log("Picking up item");
		var pickUpItem = acting.SelectAct("PickUpItem") as PickUpItem;
        if (pickUpItem)
        {
			pickUpItem.PhysicalItem = physicalItem;
        }
    }

	public virtual void Stagger(Vector3 pushForce)
    {
		acting.Staggered.PushForce = pushForce;
		acting.ForceIntoAct(acting.Staggered);
	}
}

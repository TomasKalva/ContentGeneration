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
public class Agent : MonoBehaviour
{
    public Movement movement;
    public Acting acting;
	public Animator animator;
	public CharacterState character;

	/// <summary>
	/// Used for reseting animation back to idle.
	/// </summary>
	int stepsSinceMoved;

	bool died;

	// Start is called before the first frame update
	void Awake()
	{
		movement = GetComponent<Movement>();
		acting = GetComponent<Acting>();
		animator = GetComponent<Animator>();
		character = GetComponent<CharacterState>();
		stepsSinceMoved = 0;
	}

	public void StartReceivingControls()
	{
		movement.TryClearInstructions();

		if(stepsSinceMoved-- < 0)
		{
			animator.SetBool("IsMoving", false);
		}
	}

	public void UpdateAgent()
	{
		if(character.Dead)
        {
            if (!died)
			{
				Die();
				died = true;
			}
			return;
        }

		acting.Act(this);

		movement.MovementUpdate();
	}

	public void Move(Vector2 direction)
    {
		if (direction.sqrMagnitude > 0.0001f && !acting.Busy)
		{
			var run = acting.SelectAct("Run") as Move;
			run.Direction = direction;
			stepsSinceMoved = 3;
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
		GameViewModel.ViewModel.Enemies.Remove(character);
		Destroy(gameObject, 1f);
		enabled = false;
    }
}

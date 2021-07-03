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

	int stepsSinceMoved;

	// Start is called before the first frame update
	void Start()
	{
		movement = GetComponent<Movement>();
		acting = GetComponent<Acting>();
		animator = GetComponent<Animator>();
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
		if (!acting.Busy)
        {
			acting.Act(this);
		}

	}

	public void Move(Vector2 direction)
    {
		if (direction.sqrMagnitude > 0.0001f && !acting.Busy)
		{
			var run = acting.SelectAct("Run") as Move;
			run.SetDirection(direction);
			stepsSinceMoved = 3;
        }
	}

	protected void ResetState()
	{
		animator.SetBool("IsMoving", false);
	}
}

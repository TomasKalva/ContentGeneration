using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(IActing))]
public class Agent : MonoBehaviour
{
    public Movement movement;
    public IActing acting;
	public Animator animator;

	// Start is called before the first frame update
	void Start()
	{
		movement = GetComponent<Movement>();
		acting = GetComponent<IActing>();
		animator = GetComponent<Animator>();
	}

	public void UpdateAgent()
	{
		movement.TryClearInstructions();
        if (!acting.Busy)
        {
			acting.Act(this);
        }
	}

	public void Move(Vector2 direction)
    {
		movement.Move(direction);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : AnimatedAct
{ 
    Vector2 direction;

    public Vector2 Direction{
        get => direction;
        set => direction = value;
    }

    public override bool UpdateAct(Agent agent)
    {
        agent.animator.SetBool("IsMoving", true);
        agent.movement.Move(direction);
        return true;
    }
}

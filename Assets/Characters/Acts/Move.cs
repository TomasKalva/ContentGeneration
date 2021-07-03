using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : AnimatedAct
{
    private Vector2 direction;

    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }

    public override IEnumerator Perform(Agent agent)
    {
        agent.animator.SetBool("IsMoving", true);
        agent.movement.Move(direction);
        return null;
    }
}

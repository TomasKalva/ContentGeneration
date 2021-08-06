using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeAgentState : AnimatedAct
{
    [SerializeField]
    bool setCanMove;

    [SerializeField]
    bool canMove;

    public override void StartAct(Agent agent)
    {
        timeElapsed = 0f;

        agent.animator.CrossFade(animationName, 0.05f);

        if (setCanMove)
        {
            agent.CanMove = canMove;
        }
    }
}
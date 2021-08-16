using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeAgentState : AnimatedAct
{
    [SerializeField]
    bool setCanMove;

    [SerializeField]
    bool canMove;

    public override void OnStart(Agent agent)
    {
        PlayAnimation(agent);

        if (setCanMove)
        {
            agent.CanMove = canMove;
        }
    }
}
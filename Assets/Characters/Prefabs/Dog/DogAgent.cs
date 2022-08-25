using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DogAgent : Agent
{
    [SerializeField]
    public ColliderDetector dashForwardDetector;

    [SerializeField]
    public ColliderDetector slashDetector;

    public Act DashForward()
    {
        var attack = acting.SelectAct("DashForward") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act LeftSlash()
    {
        var attack = acting.SelectAct("LeftSlash") as Attack;
        return attack;
    }

    public Act RightSlash()
    {
        var attack = acting.SelectAct("RightSlash") as Attack;
        return attack;
    }
}
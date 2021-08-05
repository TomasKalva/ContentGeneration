using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleAct : Act
{
    public IdleAct()
    {
        actName = "Idle";
        type = ActType.IDLE;
        priority = -100;
    }

    public override void StartAct(Agent agent) { }
    public override void EndAct(Agent agent) { }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DragonManAgent : Agent
{
    public Act Slash()
    {
        var attack = acting.SelectAct("Slash") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act FlapWings()
    {
        var attack = acting.SelectAct("FlapWings") as Shoot;
        attack.ThrowEffect = Lib.Spells.Cloud(Lib.VFXs.MovingCloud, Color.white, Lib.VFXs.WindTexture, 1.0f, 4f, 700f, new DamageDealt(DamageType.Divine, 2f));
        return attack;
    }

    public Act SpitFire()
    {
        var attack = acting.SelectAct("SpitFire") as SpitFire;
        return attack;
    }
}
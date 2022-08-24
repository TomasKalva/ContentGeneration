using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SkinnyWomanAgent : Agent
{
    public Act RushForward()
    {
        var attack = acting.SelectAct("RushForward") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act CastFireball()
    {
        var attack = acting.SelectAct("CastFireball") as Shoot;
        attack.ThrowEffect = Lib.Spells.Bolt(Lib.VFXs.Fireball, Color.yellow, Lib.VFXs.FireTexture, 0.5f, 10f, new DamageDealt(DamageType.Chaos, 10f), false);
        return attack;
    }

    public Act Enchant()
    {
        var attack = acting.SelectAct("Enchant") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }
}
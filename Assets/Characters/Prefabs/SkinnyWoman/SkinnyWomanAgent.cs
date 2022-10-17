using OurFramework.Characters.SpellClasses;
using UnityEngine;


public class SkinnyWomanAgent : Agent
{
    [SerializeField]
    public ColliderDetector rushForwardDetector;

    [SerializeField]
    public ColliderDetector castDetector;

    public Act RushForward()
    {
        var attack = acting.SelectAct("RushForward") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act CastFireball(Effect castFireball)
    {
        var attack = acting.SelectAct("CastFireball") as Shoot;
        attack.ShotEffect = castFireball;
        return attack;
    }

    public Act Enchant()
    {
        var attack = acting.SelectAct("Enchant") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }
}
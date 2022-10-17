using OurFramework.Characters.SpellClasses;
using UnityEngine;


public class MayanAgent : Agent
{
    [SerializeField]
    public ColliderDetector overheadDetector;

    [SerializeField]
    public ColliderDetector throwDetector;

    [SerializeField]
    public ColliderDetector swingDetector;

    public Act OverheadAttack()
    {
        var attack = acting.SelectAct("Overhead") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act LongOverheadAttack()
    {
        var attack = acting.SelectAct("LongOverhead") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act LeftSwing()
    {
        var attack = acting.SelectAct("LeftSwing") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act RightSwing()
    {
        var attack = acting.SelectAct("RightSwing") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act Throw(Effect throwEffect)
    {
        var thr = acting.SelectAct("Throw") as Shoot;
        thr.ShotEffect = throwEffect;
        return thr;
    }
}
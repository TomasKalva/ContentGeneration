using OurFramework.Characters.SpellClasses;
using UnityEngine;


public class DragonManAgent : Agent
{
    [SerializeField]
    public ColliderDetector slashDetector;

    [SerializeField]
    public ColliderDetector castDetector;

    [SerializeField]
    public ColliderDetector spitFireDetector;

    public Act Slash()
    {
        var attack = acting.SelectAct("Slash") as Attack;
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act FlapWings(Effect flapWings)
    {
        var attack = acting.SelectAct("FlapWings") as Shoot;
        attack.ShotEffect = flapWings;
        return attack;
    }

    public Act SpitFire(ByTransform<Effect> spitFire)
    {
        var attack = acting.SelectAct("SpitFire") as SpitFire;
        attack.SpitFromPositionDirectionEffect = spitFire;
        return attack;
    }
}
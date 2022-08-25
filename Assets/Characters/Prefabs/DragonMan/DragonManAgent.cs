using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using System.Collections;
using System.Collections.Generic;
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
        attack.ShotEffect = flapWings;// Lib.Spells.Cloud(Lib.VFXs.MovingCloud, Color.white, Lib.VFXs.WindTexture, 1.5f, 7f, 700f, new DamageDealt(DamageType.Divine, 2f));
        return attack;
    }

    public Act SpitFire(ByTransform<Effect> spitFire)
    {
        var attack = acting.SelectAct("SpitFire") as SpitFire;
        attack.SpitFromPositionDirectionEffect = spitFire;
            /*(position, direction) => user => user.World.CreateOccurence(
                Lib.Selectors.GeometricSelector(Lib.VFXs.Fireball, 4f, Lib.Selectors.Initializator()
                    .ConstPosition(position)
                    .SetVelocity(user => direction, 6f)
                    .RotatePitch(-90f)
                    .Scale(1.5f)
                    )(new SelectorArgs(Color.yellow, Lib.VFXs.FireTexture))(user),
                Lib.Effects.Damage(new DamageDealt(DamageType.Chaos, 6f))
            );*/
        return attack;
    }
}
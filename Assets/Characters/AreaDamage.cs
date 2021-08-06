using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ColliderDetector))]
public class AreaDamage : DamageDealer
{
    protected ColliderDetector detector;

    protected override void Damage(Agent agent)
    {
        agent.CharacterState.Health -= damage;
    }

    protected override IEnumerable<Agent> HitAgents()
    {
        if (detector.Triggered)
        {
            return new Agent[1] { detector.other.GetComponentInParent<Agent>() };
        }
        else
        {
            return Enumerable.Empty<Agent>();
        }
    }

    protected override void Initialize()
    {
        detector = GetComponent<ColliderDetector>();
    }
}

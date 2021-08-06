using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AreaDamage : DamageDealer
{
    /// <summary>
    /// The collider detector can be in children.
    /// </summary>
    protected ColliderDetector detector;

    public override bool Active 
    { 
        get => true;
        set { } 
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
        detector = GetComponentInChildren<ColliderDetector>();
    }
}

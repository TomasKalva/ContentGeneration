using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Weapon : AreaDamage
{
    bool _active;

    [SerializeField]
    float pushForceIntensity = 500f;

    public override bool Active
    {
        get
        {
            return _active;
        }
        set
        {
            _active = value;
            if (!_active)
            {
                ResetHitAgents();
            }
        }
    }

    protected override IEnumerable<Agent> HitAgents()
    {
        if (Active && detector.Triggered)
        {
            return new Agent[1] { detector.other.GetComponentInParent<Agent>() };
        }
        else
        {
            return Enumerable.Empty<Agent>();
        }
    }

    public override Vector3 PushForce(Transform enemy)
    {
        return pushForceIntensity * (enemy.position - Owner.transform.position).normalized;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class Projectile : AreaDamage
{
    [SerializeField]
    float timeUntilDestroyed;

    [SerializeField]
    float pushForceIntensity = 500f;

    protected override void DealDamage(Agent agent)
    {
        if (!Active)
            return;

        agent.CharacterState.TakeDamage(Damage);
        Destroy(gameObject);
    }

    protected override void OnFixedUpdate()
    {
        if (!Active)
            return;

        if((timeUntilDestroyed -= Time.fixedDeltaTime) <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public override bool Active { get; set; }

    public override Vector3 PushForce(Transform enemy)
    {
        return pushForceIntensity * transform.forward;
    }
}

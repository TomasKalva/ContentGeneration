using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Projectile : AreaDamage
{
    [SerializeField]
    float timeUntilDestroyed;

    protected override void DealDamage(Agent agent)
    {
        if (!Active)
            return;

        agent.CharacterState.Health -= damage;
        Destroy(gameObject);
    }

    protected override void OnFixedUpdate()
    {
        if (!Active)
            return;

        if((timeUntilDestroyed -= Time.fixedDeltaTime) <= 0f){
            Destroy(gameObject);
        }
    }

    public override bool Active { get; set; }
}

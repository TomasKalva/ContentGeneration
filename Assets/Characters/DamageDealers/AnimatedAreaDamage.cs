using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatedAreaDamage : AreaDamage
{
    Animator animator;

    protected override void Initialize()
    {
        base.Initialize();
        animator = GetComponent<Animator>();
    }

    protected override void Damage(Agent agent)
    {
        if (!Active)
            return;

        agent.CharacterState.Health -= damage;
    }

    protected override void OnFixedUpdate()
    {
        if (!Active)
            return;

        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if(stateInfo.normalizedTime >= 1f)
        {
            Destroy(gameObject);
        }
    }

    bool _activate;
    public override bool Active 
    {
        get => _activate;
        set
        {
            _activate = value;
            /*if (_activate)
            {
                animator.Play
            }*/
        } 
    }
}

using Animancer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatedAct : Act
{
    [SerializeField]
    protected ClipTransition anim;

    [SerializeField]
    protected float duration = 1f;

    protected float Duration => duration == 0f ? 0.01f : duration;

    protected float timeElapsed;

    protected List<MovementConstraint> movementContraints;

    public void Initialize(Agent agent)
    {
        // Set speed so that the animation takes duration seconds
        var speed = anim.Clip.length / Duration;
        anim.Speed = speed;
    }

    public sealed override void StartAct(Agent agent)
    {
        Initialize(agent);
        timeElapsed = 0f;

        OnStart(agent);
    }

    public virtual void OnStart(Agent agent) { }

    public override bool UpdateAct(Agent agent)
    {
        timeElapsed += Time.fixedDeltaTime;
        OnUpdate(agent);
        return timeElapsed >= duration;
    }

    public virtual void OnUpdate(Agent agent) { }

    public override void EndAct(Agent agent)
    {
            
    }

    protected void PlayAnimation(Agent agent)
    {
        agent.animancerAnimator.Play(anim, 0.1f);
    }

    protected void PlayIfNotActive(Agent agent, float transitionTime)
    {
        if (!agent.animancerAnimator.IsPlaying(anim))
        {
            agent.animancerAnimator.Play(anim, transitionTime);
        }
    }
}

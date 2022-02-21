using Animancer;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedAct : Act
{
    [SerializeField]
    protected ClipTransition anim;


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
        agent.CharacterState.Will -= cost;

        OnStart(agent);
    }

    public virtual void OnStart(Agent agent) { }

    public override bool UpdateAct(Agent agent, float dt)
    {
        timeElapsed += dt;
        OnUpdate(agent);
        return timeElapsed >= duration;
    }

    public virtual void OnUpdate(Agent agent) { }

    public override void EndAct(Agent agent)
    {
            
    }

    protected void PlayAnimation(Agent agent)
    {
        agent.animancerAnimator.Play(anim, 1f);
    }

    protected AnimancerState PlayIfNotActive(Agent agent, float transitionTime)
    {
        if (!agent.animancerAnimator.IsPlaying(anim))
        {
            return agent.animancerAnimator.Play(anim, 1f/* transitionTime*/);
        }
        return agent.animancerAnimator.States.Current;
    }
}

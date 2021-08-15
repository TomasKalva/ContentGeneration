using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedAct : Act
{
    [SerializeField]
    protected string animationName;

    [SerializeField]
    protected float duration;

    protected float timeElapsed;

    protected List<MovementConstraint> movementContraints;

    public override void StartAct(Agent agent)
    {
        timeElapsed = 0f;
    }

    public override bool UpdateAct(Agent agent)
    {
        timeElapsed += Time.fixedDeltaTime;
        return timeElapsed >= duration;
    }

    public override void EndAct(Agent agent)
    {
            
    }

    protected void PlayIfNotActive(Agent agent, float normalisedTransitionTime)
    {
        var clipInfos = agent.animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfos.Length != 0)
        {
            var clip = clipInfos[0].clip;
            if (clip.name != animationName)
            {
                agent.animator.CrossFade(animationName, normalisedTransitionTime);
            }
        }
    }
}

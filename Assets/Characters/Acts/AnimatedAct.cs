using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatedAct : Act
{
    [SerializeField]
    protected string animationName;

    [SerializeField]
    protected float duration;

    protected float timeElapsed;

    protected List<MovementConstraint> movementContraints;

    public override void Initialize(Agent agent)
    {
        // Set duration of the act to length of animation
        var runtimeAnimatorController = agent.animator.runtimeAnimatorController;
        var animationClip = runtimeAnimatorController.animationClips.Where(c => c.name == animationName).FirstOrDefault();
        if(animationClip != null)
        {
            duration = animationClip.length + 0.021f;
        }
    }

    public override void StartAct(Agent agent)
    {
        timeElapsed = 0f;
        //agent.animator.GetCurrentAnimatorClipInfo(0)[0].clip.
        
        /*animator.CrossFade("StateToSetTo", 0);
        //I think you'll have to yield and set this next frame, unsure
        animator.speed = 0;*/
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

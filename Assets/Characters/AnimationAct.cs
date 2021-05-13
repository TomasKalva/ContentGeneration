using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAct : Act
{
    [SerializeField]
    string animationName;

    [SerializeField]
    float duration;

    public override IEnumerator Perform(Agent agent)
    {
        agent.animator.Play(animationName);
        yield return new WaitForSeconds(duration); 
    }
}

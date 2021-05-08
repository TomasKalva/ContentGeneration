using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAct : Act
{
    public override IEnumerator Perform(Agent agent)
    {
        agent.animator.Play("Base Layer.Armature|TailStab");
        yield return new WaitForSeconds(5); 
    }
}

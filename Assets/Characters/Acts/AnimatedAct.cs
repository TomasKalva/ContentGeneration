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

    public override bool UpdateAct(Agent agent)
    {
        timeElapsed += Time.fixedDeltaTime;
        return timeElapsed >= duration;
    }
}

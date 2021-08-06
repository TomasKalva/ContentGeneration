using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shockwave : AnimatedAct
{
    [SerializeField]
    int totalShocks;

    int createdShocks;

    [SerializeField]
    float distBetween;

    [SerializeField]
    Transform start;

    [SerializeField]
    AreaDamage shockPrefab;

    float Delay => duration / totalShocks;

    public override void StartAct(Agent agent)
    {
        timeElapsed = 0f;
        createdShocks = 0;

        agent.animator.CrossFade(animationName, 0.05f);
    }

    public override bool UpdateAct(Agent agent)
    {
        if(timeElapsed >= createdShocks * Delay)
        {
            var shock = Instantiate(shockPrefab);
            // todo: make shocks always appear on the ground
            shock.transform.position = start.position + createdShocks * distBetween * agent.movement.AgentForward;
            shock.Owner = agent;
            shock.Active = true;
            createdShocks++;
        }
        timeElapsed += Time.fixedDeltaTime;
        return createdShocks >= totalShocks;
    }
}
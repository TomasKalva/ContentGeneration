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

    public override void OnStart(Agent agent)
    {
        createdShocks = 0;

        PlayAnimation(agent);
    }

    public override bool UpdateAct(Agent agent, float dt)
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
        timeElapsed += dt;
        return createdShocks >= totalShocks;
    }
}
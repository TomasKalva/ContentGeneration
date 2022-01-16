using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiEventAct : AnimatedAct
{
    [Serializable]
    public class TimeAction
    {
        public float time;
        public Action action;

        public TimeAction(float time, Action action)
        {
            this.time = time;
            this.action = action;
        }
    }

    [SerializeField]
    protected List<TimeAction> timedActions;

    int currentActionI;

    public override void OnStart(Agent agent)
    {
        PlayAnimation(agent);

        timedActions.Sort((ta1, ta2) => (int)(1000_000f * ta1.time - ta2.time));
        currentActionI = 0;
    }

    public override void OnUpdate(Agent agent)
    {
        if (currentActionI >= timedActions.Count)
            return;

        var normalizedElapsed = timeElapsed / duration;
        var current = timedActions[currentActionI];
        if (normalizedElapsed >= current.time)
        {
            currentActionI++;
            current.action();
        }
    }
}
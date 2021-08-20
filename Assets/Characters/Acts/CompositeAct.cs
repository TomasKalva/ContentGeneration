using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositeAct : Act
{
    [SerializeField]
    Act[] acts;

    int currentActIndex;

    Act CurrentAct => currentActIndex < acts.Length ? acts[currentActIndex] : null;

    public override void StartAct(Agent agent)
    {
        currentActIndex = 0;
        if(CurrentAct == null)
        {
            Debug.LogError("Composite act is empty!");
            return;
        }
        CurrentAct.StartAct(agent);
    }

    public override bool UpdateAct(Agent agent, float dt)
    {
        if(acts.Length == 0)
        {
            Debug.LogError("Composite act is empty!");
        }

        if (CurrentAct.UpdateAct(agent, dt))
        {
            CurrentAct.EndAct(agent);
            currentActIndex++;
            if(CurrentAct != null)
            {
                CurrentAct.StartAct(agent);
                return false;
            }
            else
            {
                return true;
            }
        }
        return false;
    }
}

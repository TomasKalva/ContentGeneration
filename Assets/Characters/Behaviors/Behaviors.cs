using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Behaviors
{
    List<Behavior> behaviors;

    public Behavior CurrentBehaviour;

    public Behaviors()
    {
        behaviors = new List<Behavior>();
    }

    public void AddBehavior(Behavior behavior)
    {
        behaviors.Add(behavior);
    }

    public bool BehaviorPossible(Agent agent, int minPriority)
    {
        return behaviors.Where(behavior => behavior.CanEnter(agent) && behavior.Priority(agent) > minPriority).Any();
    }

    public Behavior NextBehavior(Agent agent)
    {
        return behaviors.Where(behavior => behavior.CanEnter(agent)).ArgMax(behavior => behavior.Priority(agent));
    }

    public void UpdateBehavior(Agent agent)
    {
        if(CurrentBehaviour == null)
        {
            CurrentBehaviour = NextBehavior(agent);
            //Debug.Log(CurrentBehaviour);
            if (CurrentBehaviour != null)
            {
                CurrentBehaviour.Enter(agent);
            }
            else
            {
                return;
            }
        }

        if (CurrentBehaviour.Update(agent))
        {
            CurrentBehaviour.Exit(agent);
            CurrentBehaviour = null;
        }
    }
}

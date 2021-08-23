using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Behaviors: MonoBehaviour
{
    [SerializeField]
    GameObject behaviorsObject;

    Behavior[] behaviors;

    public Behavior CurrentBehaviour;

    private void Awake()
    {
        behaviors = behaviorsObject.GetComponents<Behavior>();
    }

    public Behavior NextBehavior(Agent agent)
    {
        return behaviors.Where(behavior => behavior.CanEnter(agent)).GetRandom(behavior => behavior.Priority(agent));
    }

    public void UpdateBehavior(Agent agent)
    {
        if(CurrentBehaviour == null)
        {
            CurrentBehaviour = NextBehavior(agent);
            Debug.Log(CurrentBehaviour);
            if (CurrentBehaviour != null)
            {
                CurrentBehaviour.Enter(agent);
            }
            else
            {
                return;
            }
        }

        if (CurrentBehaviour.UpdateBehavior(agent))
        {
            CurrentBehaviour.Exit(agent);
            CurrentBehaviour = null;
        }
    }
}
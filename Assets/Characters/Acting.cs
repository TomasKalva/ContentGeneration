using Assets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IActing
{
    bool Busy { get; set; }
    void Act(Agent agent);
}

/// <summary>
/// Contains all acts that the agent can perform. Each frame takes requests on which act should
/// be performed and then chooses the best option. Supports action queueing
/// </summary>
public class Acting : MonoBehaviour, IActing
{
    static Act Idle;

    public List<Act> acts;

    public bool Busy { get; set; }

    [SerializeField]
    Act _activeAct;
    public Act ActiveAct {
        get => _activeAct ? _activeAct : Idle;
        private set => _activeAct = value; 
    }

    List<Act> selectedActs = new List<Act>();

    void Awake()
    {
        Idle = gameObject.AddComponent<IdleAct>();
        acts = GetComponents<Act>().ToList();
    }

    public Act SelectAct(string actName)
    {
        var selected = acts.Where(act => act.actName == actName).FirstOrDefault();
        selectedActs.Add(selected);
        return selected;
    }

    public Act SelectAct(Act act)
    {
        selectedActs.Add(act);
        return act;
    }

    private Act GetNextAct()
    {
        return selectedActs.ArgMax(act => act ? act.priority : -1000_000);
    }

    public bool CanAct()
    {
        return GetNextAct() != null;
    }


    public void Act(Agent agent)
    {
        if (!Busy)
        {
            ActiveAct = GetNextAct();
            if (_activeAct != null)
            {
                // Activate best act and remove the rest from the queue
                ActiveAct.StartAct(agent);
                Busy = true;
                selectedActs.Clear();
            }
        }

        if (Busy)
        {
            if (ActiveAct.UpdateAct(agent))
            {
                ActiveAct.EndAct(agent);
                ActiveAct = null;
                Busy = false;
            }
            // Queue selected acts if currently Busy
            return;
        }

        // Clear all unimportant acts
        selectedActs.RemoveAll(act => act.priority < 0f);
    }
}

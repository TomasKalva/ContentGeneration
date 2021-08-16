using Assets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IActing
{
    bool Busy { get; set; }
    void Act();
}

/// <summary>
/// Contains all acts that the agent can perform. Each frame takes requests on which act should
/// be performed and then chooses the best option. Supports action queueing.
/// </summary>
[RequireComponent(typeof(IdleAct))]
public class Acting : MonoBehaviour, IActing
{
    Agent agent;

    Act Idle;

    public StaggeredAct Staggered;

    public List<Act> acts;

    public bool Busy { get; set; }

    bool ActiveActStarted { get; set; }

    [SerializeField]
    Act _activeAct;
    public Act ActiveAct {
        get => _activeAct;
        private set => _activeAct = value; 
    }

    List<Act> selectedActs = new List<Act>();

    void Awake()
    {
        Idle = GetComponent<IdleAct>();
        Staggered = GetComponent<StaggeredAct>();
        acts = GetComponents<Act>().ToList();
        agent = GetComponent<Agent>();
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

    /// <summary>
    /// Force agent to do this act. Should only be used for acts not dependent
    /// on agents choice (getting staggered, ...).
    /// </summary>
    public void ForceIntoAct(Act act)
    {
        ActiveAct?.EndAct(agent);
        ActiveAct = act;
        ActiveActStarted = false;
        Busy = true;
    }

    private Act GetNextAct()
    {
        return selectedActs.ArgMax(act => act ? act.priority : -1000_000);
    }

    public bool CanAct()
    {
        return GetNextAct() != null;
    }


    public void Act()
    {
        if (!Busy)
        {
            ActiveAct = GetNextAct();
            if (_activeAct != null)
            {
                // Activate best act and remove the rest from the queue
                Busy = true;
            }
            else
            {
                ActiveAct = Idle;
            }
            ActiveActStarted = false;
            selectedActs.Clear();
        }

        // Start act if not started yet
        if (!ActiveActStarted)
        {
            ActiveAct.StartAct(agent);
            ActiveActStarted = true;
        }

        if (ActiveAct != null)
        {
            if (ActiveAct.UpdateAct(agent))
            {
                ActiveAct.EndAct(agent);
                ActiveAct = Idle;
                Busy = false;
            }
            // Queue selected acts if currently Busy
            return;
        }

        // Clear all unimportant acts
        selectedActs.RemoveAll(act => act.priority < 0f);
    }
}

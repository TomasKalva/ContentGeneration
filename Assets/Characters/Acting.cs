﻿using Assets;
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
public class Acting : MonoBehaviour, IActing
{
    Agent agent;

    Act Idle;

    StaggeredAct staggered;

    public StaggeredAct Staggered => staggered;

    List<Act> acts;

    public IEnumerable<Act> Acts => acts;

    public bool Busy { get; set; }

    bool ActiveActStarted { get; set; }

    /// <summary>
    /// Object that has the acts monobehaviours attached.
    /// </summary>
    [SerializeField]
    GameObject actContainer;

    [SerializeField]
    Act _activeAct;
    public Act ActiveAct {
        get => _activeAct;
        private set => _activeAct = value; 
    }

    List<Act> selectedActs = new List<Act>();

    void Awake()
    {
        Idle = actContainer.GetComponent<IdleAct>();
        staggered = actContainer.GetComponent<StaggeredAct>();
        acts = actContainer.GetComponents<Act>().ToList();
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
        if (ActiveAct != null)
        {
            ActiveAct.EndAct(agent);
            ActiveAct.ActEnded = true;
        }
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
        foreach(var act in acts)
        {
            act.ActEnded = false;
        }

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

        // Start act if not started yet (also handles starting of forced acts)
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
                ActiveAct.ActEnded = true;
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

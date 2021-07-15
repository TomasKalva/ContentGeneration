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

public class Acting : MonoBehaviour, IActing
{
    [SerializeField]
    public List<Act> acts;

    public bool Busy { get; set; }

    List<Act> selectedActs = new List<Act>();

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
        return selectedActs.MaxArg(act => act ? act.priority : -1000_000);
    }

    public bool CanAct()
    {
        return GetNextAct() != null;
    }


    public void Act(Agent agent)
    {
        var bestAct = GetNextAct();
        if (bestAct) {
            StartCoroutine(((IActing)this).ActSequence(agent, bestAct));
            selectedActs.Clear();
        }
    }
}

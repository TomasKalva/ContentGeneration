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

public class DetectorActing : MonoBehaviour, IActing
{
    [SerializeField]
    List<Act> acts;

    bool IActing.Busy { get; set; }

    private Act GetNextAct()
    {
        return acts.Where(act => act.CanBeUsed()).FirstOrDefault();
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
        }
    }
}

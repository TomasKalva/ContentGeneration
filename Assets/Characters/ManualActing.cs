using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ManualActing : MonoBehaviour, IActing
{
    [SerializeField]
    List<Act> acts;

    bool IActing.Busy { get; set; }

    private Act currentAct;

    public void DoAct(string actName)
    {
        currentAct = acts.Where(act => act.actName == actName).FirstOrDefault();
    }

    public void Act(Agent agent)
    {
        if (currentAct)
        {
            Debug.Log(currentAct.actName);
            StartCoroutine(((IActing)this).ActSequence(agent, currentAct));
            currentAct = null;
        }
    }
}
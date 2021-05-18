using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Acting : MonoBehaviour
{
    [SerializeField]
    List<Act> acts;

    public bool busy;

    private Act GetBestAct()
    {
        return acts.Where(act => act.CanBeUsed()).FirstOrDefault();
    }

    public bool CanAct()
    {
        return GetBestAct() != null;
    }

    public IEnumerator Act(Agent agent)
    {
        var bestAct = GetBestAct();
        if (bestAct) {
            busy = true;
            yield return bestAct.Perform(agent);
            busy = false;
        }
    }
}

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
        busy = true;
        yield return GetBestAct().Perform(agent);
        busy = false;
    }
}

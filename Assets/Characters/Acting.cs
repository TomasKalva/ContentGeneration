using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Acting : MonoBehaviour
{
    [SerializeField]
    Act attack;

    public bool busy;

    public bool CanAttack()
    {
        return attack.CanBeUsed();
    }

    public IEnumerator Act(Agent agent)
    {
        busy = true;
        yield return attack.Perform(agent);
        busy = false;
    }
}

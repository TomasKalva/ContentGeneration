using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighting : MonoBehaviour
{
    [SerializeField]
    Attack attack;

    public bool busy;

    public bool CanAttack()
    {
        return attack.CanBeUsed();
    }

    public IEnumerator Attack()
    {
        busy = true;
        Debug.Log(busy);
        yield return attack.Act();
        busy = false;
        Debug.Log(busy);
    }
}

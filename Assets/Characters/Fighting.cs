using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighting : MonoBehaviour
{
    [SerializeField]
    Attack attack;

    // Update is called once per frame
    void Update()
    {
        if (attack.CanBeUsed())
        {
            Debug.Log("detected player");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Act : MonoBehaviour
{
    [SerializeField]
    Detector detector;

    public bool CanBeUsed()
    {
        return detector.triggered;
    }

    public virtual IEnumerator Perform(Agent agent)
    {
        Debug.Log("Started attack");
        yield return new WaitForSeconds(5f);
        Debug.Log("Ended attack");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Detector : MonoBehaviour
{
    [SerializeField]
    LayerMask detectionMask = -1;

    public bool triggered;

    public Collider other;

    private void OnTriggerEnter(Collider other)
    {
        if (detectionMask == (detectionMask | (1 << other.gameObject.layer)))
        {
            triggered = true;
            this.other = other;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (detectionMask == (detectionMask | (1 << other.gameObject.layer)))
        {
            triggered = false;
            this.other = null;
        }
    }
}

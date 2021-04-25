using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField]
    Detector detector;

    public bool CanBeUsed()
    {
        return detector.triggered;
    }
}

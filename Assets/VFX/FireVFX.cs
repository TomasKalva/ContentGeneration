using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FireVFX : VFX
{
    [SerializeField]
    string gradientName;


    public Gradient Gradient
    {
        set
        {
            visualEffect.SetGradient(gradientName, value);
        }
    }
}

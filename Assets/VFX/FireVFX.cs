using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FireVFX : VFX, IDestroyable
{
    [SerializeField]
    string startColorName;

    [SerializeField]
    string endColorName;

    [SerializeField]
    string gradientName;

    public Color StartColor
    {
        set
        {
            visualEffect.SetVector4(startColorName, value);
        }
    }

    public Color EndColor
    {
        set
        {
            visualEffect.SetVector4(endColorName, value);
        }
    }

    public Gradient Gradient
    {
        set
        {
            visualEffect.SetGradient(gradientName, value);
        }
    }

    public void Destroy(float timeS)
    {
        Destroy(gameObject, timeS);
    }
}

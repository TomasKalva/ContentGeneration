using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FireVFX : MonoBehaviour
{
    [SerializeField]
    VisualEffect fireEffect;

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
            fireEffect.SetVector4(startColorName, value);
        }
    }

    public Color EndColor
    {
        set
        {
            fireEffect.SetVector4(endColorName, value);
        }
    }

    public Gradient Gradient
    {
        set
        {
            fireEffect.SetGradient(gradientName, value);
        }
    }
}

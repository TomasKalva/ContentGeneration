using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class MovingCloudVFX : VFX
{
    [SerializeField]
    string halfWidthName;

    public MovingCloudVFX SetHalfWidth(float halfWidth)
    {
        visualEffect.SetFloat(halfWidthName, halfWidth);
        return this;
    }
}

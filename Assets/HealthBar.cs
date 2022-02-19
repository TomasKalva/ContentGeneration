using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    Slider fill;

    public void SetValue(float val)
    {
        fill.value = val;
    }
}

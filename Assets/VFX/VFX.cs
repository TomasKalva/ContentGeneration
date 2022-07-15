using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFX : MonoBehaviour
{
    [SerializeField]
    protected VisualEffect visualEffect;

    [SerializeField]
    protected ColliderDetector colliderDetector;
    public ColliderDetector ColliderDetector => colliderDetector;

    public virtual void SetColor(Color color) { }
    public virtual void SetTexture() { }
}

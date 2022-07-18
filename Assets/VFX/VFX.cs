using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFX : MonoBehaviour
{
    [SerializeField]
    protected VisualEffect visualEffect;

    /// <summary>
    /// Disables its renderer on start so shouldn't be put on the same object as visual effect.
    /// </summary>
    [SerializeField]
    protected ColliderDetector colliderDetector;
    public ColliderDetector ColliderDetector => colliderDetector;

    public virtual void SetColor(Color color) { }
    public virtual void SetTexture() { }
}

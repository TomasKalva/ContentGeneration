using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometricPrimitive : MonoBehaviour
{
    /// <summary>
    /// Returns a copy of this object.
    /// </summary>
    public GeometricPrimitive New() => Instantiate(this);

}

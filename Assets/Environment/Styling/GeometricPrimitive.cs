using UnityEngine;

namespace OurFramework.Environment.StylingAreas
{
    public class GeometricPrimitive : MonoBehaviour
    {
        /// <summary>
        /// Returns a copy of this object.
        /// </summary>
        public GeometricPrimitive New() => Instantiate(this);

    }
}

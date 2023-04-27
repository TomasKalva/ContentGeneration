using UnityEngine;

namespace OurFramework.Environment.StylingAreas
{
    /// <summary>
    /// Real world geometry that can be placed to various parts of the grid.
    /// </summary>
    public class GeometricPrimitive : MonoBehaviour
    {
        /// <summary>
        /// Returns a copy of this object.
        /// </summary>
        public GeometricPrimitive New() => Instantiate(this);

    }
}

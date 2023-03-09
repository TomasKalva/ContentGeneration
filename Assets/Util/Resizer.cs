using OurFramework.Util;
using UnityEngine;

namespace OurFramework.Util
{
    public class Resizer : MonoBehaviour
    {
        public Vector3 newSize;

        public void Resize()
        {
            var collider = GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogError($"Not collider component exists on {gameObject.name}");
                return;
            }

            var localScale = transform.localScale.ComponentWise(Mathf.Abs);
            if (localScale.Any(a => a == 0f))
            {
                Debug.LogWarning($"Scale component is zero on {gameObject.name}");
            }

            var bounds = 2f * collider.bounds.extents;
            var newGlobalScale = newSize.ComponentWise(bounds, (a, b) => a / b).ComponentWise(localScale, (a, b) => a * b);
            transform.localScale = newGlobalScale;

        }
    }
}

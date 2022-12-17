using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
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
}

using UnityEngine;
using UnityEngine.VFX;

namespace OurFramework.Gameplay.RealWorld
{
    /// <summary>
    /// Kiln that can burst fire.
    /// </summary>
    public class Kiln : InteractiveObject
    {
        [SerializeField]
        VisualEffect fire;

        public void BurstFire()
        {
            fire.SendEvent("FireBurst");
            Debug.Log("FireBurst");
        }
    }
}

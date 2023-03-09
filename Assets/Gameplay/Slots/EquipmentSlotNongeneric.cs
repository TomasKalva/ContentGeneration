using OurFramework.UI;
using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
    public class EquipmentSlot : MonoBehaviour
    {
        /// <summary>
        /// To manage destruction of equipment correctly.
        /// </summary>
        public World World { protected get; set; }
    }
}

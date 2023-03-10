using OurFramework.Game;
using OurFramework.UI;
using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
    /// <summary>
    /// Slot for equipment.
    /// </summary>
    public class EquipmentSlot : MonoBehaviour
    {
        /// <summary>
        /// To manage destruction of equipment correctly.
        /// </summary>
        public World World { protected get; set; }
    }
}

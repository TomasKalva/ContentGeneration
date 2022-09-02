using ContentGeneration.Assets.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

public class EquipmentSlot : MonoBehaviour
{
    /// <summary>
    /// To manage destruction of equipment correctly.
    /// </summary>
    public World World { protected get; set; }
}

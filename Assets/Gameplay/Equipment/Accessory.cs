using OurFramework.Gameplay.State;

namespace OurFramework.Gameplay.RealWorld
{
    /// <summary>
    /// Accessory such as ring or hat.
    /// </summary>
    public class Accessory : Equipment
    {
        public AccessoryItem AccessoryItem { private get; set; }
    }
}
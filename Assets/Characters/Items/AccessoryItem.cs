using Assets.Characters.Items.ItemClasses;
using ShapeGrammar;

namespace Assets.Characters.Items
{
    public class AccessoryItem : EquipmentItem<Accessory>
    {
        public Accessory Accessory
        {
            get
            {
                if (_cachedEquipment == null)
                {
                    _cachedEquipment = EquipmentMaker();
                    _cachedEquipment.AccessoryItem = this;
                }
                return _cachedEquipment;
            }
        }

        public AccessoryItem(string name, string description, GeometryMaker<Accessory> accessoryMaker) : base(name, description, accessoryMaker)
        {
        }
    }
}

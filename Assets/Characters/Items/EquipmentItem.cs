using ContentGeneration.Assets.UI.Model;
using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Characters.Items.ItemClasses
{
    public class EquipmentItem<EquipmentT> : ItemState where EquipmentT : Equipment
    {
        protected GeometryMaker<EquipmentT> EquipmentMaker { get; }
        protected EquipmentT _cachedEquipment { get; set; }

        public EquipmentItem(string name, string description, GeometryMaker<EquipmentT> equipmentMaker)
        {
            Name = name;
            Description = description;
            EquipmentMaker = equipmentMaker;
        }
    }
}

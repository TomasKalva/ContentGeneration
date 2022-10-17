﻿using OurFramework.LevelDesignLanguage;
using ContentGeneration.Assets.UI.Model;

namespace OurFramework.Characters.Items.ItemClasses
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

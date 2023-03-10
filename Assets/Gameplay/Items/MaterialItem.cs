using UnityEngine;

namespace OurFramework.Gameplay.Data
{
    /// <summary>
    /// State of material item.
    /// </summary>
    public class MaterialItem : ItemState
    {
        public Material Material { get; }

        public MaterialItem(string name, string description, Material material)
        {
            Name = name;
            Description = description;
            Material = material;
        }
    }
}

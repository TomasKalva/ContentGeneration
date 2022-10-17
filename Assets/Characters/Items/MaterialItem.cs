using ContentGeneration.Assets.UI.Model;
using UnityEngine;

namespace Assets.Characters.Items
{
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

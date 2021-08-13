#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentGeneration.Assets.UI.Model
{
    public class ItemState
    {
        public string Name { get; private set; } = "Red Ichor Essence";
        public string Description { get; private set; } = "Red liquid";

        public virtual void Use()
        {
#if NOESIS
            Debug.Log($"Using {Name}");
#endif
        }

        public virtual void Drop()
        {
#if NOESIS
            Debug.Log($"Dropping {Name}");
#endif
        }
    }
}

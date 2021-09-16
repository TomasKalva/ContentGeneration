#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using ContentGeneration.Assets.UI.Util;

namespace ContentGeneration.Assets.UI.Model
{
#if NOESIS
    [Serializable]
#endif
    public class ItemState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

#if NOESIS
        [SerializeField]
#endif
        string _name;

        public string Name 
        { 
            get => _name;
            protected set { _name = value; PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
#endif
        string _description;

        public string Description
        {
            get => _description;
            protected set { _description = value; PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
        Transform realObject;

        public Transform RealObject
        {
            get => realObject;
            protected set { realObject = value; PropertyChanged.OnPropertyChanged(this); }
        }
#endif

        public virtual void OnUse(CharacterState character)
        {
            //Debug.Log($"Using {Name}");
        }

        public virtual void OnDrop(CharacterState character)
        {
            //Debug.Log($"Dropping {Name}");
        }

        public virtual void OnEquip(CharacterState character)
        {
            //Debug.Log($"Equipping {Name}");
        }

        public virtual void OnUnequip(CharacterState character)
        {
            //Debug.Log($"Unequipping {Name}");
        }

        public virtual void OnUpdate(CharacterState character)
        {
            //Debug.Log($"Updating {Name}");
        }
    }
}

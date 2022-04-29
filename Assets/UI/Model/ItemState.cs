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
            set { _name = value; PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
#endif
        string _description;

        public string Description
        {
            get => _description;
            set { _description = value; PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
        Transform realObject;


        public Transform RealObject
        {
            get => realObject;
            set { realObject = value; PropertyChanged.OnPropertyChanged(this); }
        }
#endif

        bool IsConsumable { get; set; }
        public ItemState SetConsumable()
        {
            IsConsumable = true;
            return this;
        }

        public ItemState()
        {
            Name = "";
            Description = "";
            OnUseDelegate = _ => { };
            OnDropDelegate = _ => { };
            OnEquipDelegate = _ => { };
            OnUnequipDelegate = _ => { };
            OnUpdateDelegate = _ => { };
        }

        public delegate void CharacterAction(CharacterState state);

        public CharacterAction OnUseDelegate { get; protected set; }
        public ItemState OnUse(CharacterAction characterAction)
        {
            OnUseDelegate = character =>
            {
                characterAction(character);
                if (IsConsumable)
                {
                    character.Inventory.RemoveItem(this);
                }
            };
            return this;
            //Debug.Log($"Using {Name}");
        }

        public CharacterAction OnDropDelegate { get; protected set; }
        public ItemState OnDrop(CharacterAction characterAction)
        {
            OnDropDelegate = characterAction;
            return this;
            //Debug.Log($"Dropping {Name}");
        }

        public CharacterAction OnEquipDelegate { get; protected set; }
        public ItemState OnEquip(CharacterAction characterAction)
        {
            OnEquipDelegate = characterAction;
            return this;
            //Debug.Log($"Equipping {Name}");
        }

        public CharacterAction OnUnequipDelegate { get; protected set; }
        public ItemState OnUnequip(CharacterAction characterAction)
        {
            OnUnequipDelegate = characterAction;
            return this;
            //Debug.Log($"Unequipping {Name}");
        }

        public CharacterAction OnUpdateDelegate { get; protected set; }
        public ItemState OnUpdate(CharacterAction characterAction)
        {
            OnUpdateDelegate = characterAction;
            return this;
            //Debug.Log($"Updating {Name}");
        }
    }
}

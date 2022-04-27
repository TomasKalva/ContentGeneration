#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
#endif
using ContentGeneration.Assets.UI.Util;
using System.ComponentModel;
using System;

namespace ContentGeneration.Assets.UI.Model
{
    [Serializable]
    public class PlayerCharacterState : CharacterState {

        private bool _interactingWithUI;
        public bool InteractingWithUI { 
            get => _interactingWithUI;
            set
            {
                _interactingWithUI = value;
                PlayerInventory.Active = value;
                OnPropertyChanged(this);
            } 
        }

#if NOESIS
        [SerializeField]
        private InteractiveObject _currentInteractiveObject;
        public InteractiveObject CurrentInteractiveObject
        {
            get { return _currentInteractiveObject; }
            set { 
                _currentInteractiveObject = value;
                CurrentInteractiveObjectState = value ? value.State : null;
                OnPropertyChanged(this);
            }
        }

#endif

#if NOESIS
        [SerializeField]
#endif
        private InteractiveObjectState _currentInteractiveObjectState;
        public InteractiveObjectState CurrentInteractiveObjectState
        {
            get { return _currentInteractiveObjectState; }
            set { _currentInteractiveObjectState = value; OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
#endif
        private CharacterState _targetedEnemy;
        public CharacterState TargetedEnemy
        {
            get { return _targetedEnemy; }
            set { _targetedEnemy = value; OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
#endif
        private CharacterProperties _properties;
        public CharacterProperties Properties
        {
            get { return _properties; }
            set { _properties = value; OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
#endif
        private float _spirit;
        public float Spirit
        {
            get { return _spirit; }
            set
            {
                _spirit = value;
                OnPropertyChanged(this);
            }
        }

        public PlayerInventory PlayerInventory => (PlayerInventory)Inventory;

        public PlayerCharacterState()
        {
            Inventory = new PlayerInventory(this);

#if NOESIS
            InteractingWithUI = false;
            //Properties.Character = this;
#else
            InteractingWithUI = true;
#endif
        }

        public override bool AddItem(ItemState item)
        {
            return PlayerInventory.AddItem(SlotType.Passive, item) != null;
        }
    }
}

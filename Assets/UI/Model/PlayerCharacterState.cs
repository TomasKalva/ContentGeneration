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
                Inventory.Active = value;
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
                CurrentInteractiveObjectState = value ? value.state : null;
                OnPropertyChanged(this); 
            }
        }

        [SerializeField]
        private Bonfire _spawnPoint;
        public Bonfire SpawnPoint
        {
            get { return _spawnPoint; }
            set { _spawnPoint = value; OnPropertyChanged(this); }
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
        private Inventory _inventory;
        public Inventory Inventory
        {
            get { return _inventory; }
            set { _inventory = value; OnPropertyChanged(this); }
        }

        public PlayerCharacterState()
        {
            Inventory = new Inventory();

#if NOESIS
            InteractingWithUI = false;
#else
            InteractingWithUI = true;
#endif
        }

        public override bool AddItem(ItemState item)
        {
            return Inventory.AddItem(item);
        }
    }
}

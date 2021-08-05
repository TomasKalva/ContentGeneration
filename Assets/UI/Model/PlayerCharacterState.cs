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
    }
}

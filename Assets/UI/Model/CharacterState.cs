#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
using UnityEditor;
#endif
using ContentGeneration.Assets.UI.Util;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;

namespace ContentGeneration.Assets.UI.Model
{
#if NOESIS
    [Serializable]
#endif
    public class CharacterState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

#if NOESIS
        [SerializeField]
#endif
        private FloatRange _health;
        public FloatRange Health
        {
            get { return _health; }
            set { _health = value; PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
#endif
        private FloatRange _will;
        public FloatRange Will
        {
            get { return _will; }
            set { _will = value; PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
#endif
        private FloatRange _posture;
        public FloatRange Posture
        {
            get { return _posture; }
            set { _posture = value; PropertyChanged.OnPropertyChanged(this); }
        }

        public bool PostureBroken { get; set; }

        public bool Dead => Health <= 0f;

        public Inventory Inventory { get; set; }

        public CharacterState()
        {
            Health = new FloatRange(100, 100);
            Will = new FloatRange(20, 20);
            Posture = 10f;
            Inventory = new EnemyInventory(this);
        }

        /// <summary>
        /// Resets the state of the character.
        /// </summary>
        public void Reset()
        {
            Health += Health.Maximum;
            Will += Will.Maximum;
        }

        /// <summary>
        /// Returns true if item was added successfully.
        /// </summary>
        public virtual bool AddItem(ItemState item)
        {
#if NOESIS
            Debug.Log($"Adding item: {item}");
#endif
            return SetItemToSlot(SlotType.Passive, item);
        }

#if NOESIS
        public void Update()
        {
            Inventory.Update();

            if (PostureBroken)
            {
                Posture += ExtensionMethods.PerFixedSecond(5f * Posture.Maximum);
            }

            if (Posture.Full())
            {
                PostureBroken = false;
            }
        }
#endif

#if NOESIS
        public void TakeDamage(DamageDealer damageDealer)
        {
            Health -= damageDealer.Damage;

            if (!PostureBroken)
            {
                Posture -= damageDealer.Damage;
            }
            if (Posture.Empty())
            {
                Agent.Stagger(damageDealer.PushForce(Agent.transform));
                PostureBroken = true;
            }
        }
#endif

        public bool SetItemToSlot(SlotType slotType, ItemState item)
        {
            var slot = Inventory.AddItem(slotType, item);
#if NOESIS
            if(Agent != null)
            {
                Agent.SynchronizeWithState(this);
            }
#endif
            return slot != null;
        }

        /// <summary>
        /// To be able to trigger property change from subclasses.
        /// </summary>
        protected void OnPropertyChanged(INotifyPropertyChanged thisInstance, [CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(thisInstance, new PropertyChangedEventArgs(name));
        }

#region Screen position of health bars

#if NOESIS
        private float _uiScreenPosX;
        public float UIScreenPosX
        {
            get { return _uiScreenPosX; }
            set { _uiScreenPosX = value; PropertyChanged.OnPropertyChanged(this); }
        }

        private float _uiScreenPosY;
        public float UIScreenPosY
        {
            get { return _uiScreenPosY; }
            set { _uiScreenPosY = viewCamera.scaledPixelHeight - value; PropertyChanged.OnPropertyChanged(this); }
        }


        private float _screenPosX;
        public float ScreenPosX
        {
            get { return _screenPosX; }
            set { _screenPosX = value; PropertyChanged.OnPropertyChanged(this); }
        }

        private float _screenPosY;
        public float ScreenPosY
        {
            get { return _screenPosY; }
            set { _screenPosY = viewCamera.scaledPixelHeight - value; PropertyChanged.OnPropertyChanged(this); }
        }

        public Vector2 ScreenPos => new Vector2(ScreenPosX, ScreenPosY);

        private bool _visibleOnCamera;
        public bool VisibleOnCamera
        {
            get { return _visibleOnCamera; }
            set { _visibleOnCamera = value; OnPropertyChanged(this); }
        }

        public Camera viewCamera;

        Agent _agent;

        public Agent Agent 
        { 
            get => _agent; 
            set
            {
                _agent = value;
                if(_agent != null)
                {
                    _agent.SynchronizeWithState(this);
                }
                
            }
        }

#else
        public float ScreenPosX => 0f;

        public float ScreenPosY => 0f;
#endif

#endregion
    }
}

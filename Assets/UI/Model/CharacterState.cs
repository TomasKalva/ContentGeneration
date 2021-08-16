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
        private FloatRange _stamina;
        public FloatRange Stamina
        {
            get { return _stamina; }
            set { _stamina = value; PropertyChanged.OnPropertyChanged(this); }
        }

        public bool Dead => Health <= 0f;

        public IInventory Inventory { get; set; }

        public CharacterState()
        {
            Health = new FloatRange(100, 42);
            Stamina = new FloatRange(100, 42);
            Inventory = new EnemyInventory(this);
        }

        /// <summary>
        /// Resets the state of the character.
        /// </summary>
        public void Reset()
        {
            Health += Health.Maximum;
            Stamina += Stamina.Maximum;
        }

        /// <summary>
        /// Returns true if item was added successfully.
        /// </summary>
        public virtual bool AddItem(ItemState item)
        {
#if NOESIS
            Debug.Log($"Adding item: {item}");
#endif
            Inventory.AddItem(item);
            return false;
        }

        public void Update()
        {
            Inventory.Update();
        }

#if NOESIS
        public void TakeDamage(DamageDealer damageDealer)
        {
            Health -= damageDealer.Damage;
            var pushForce = 1000 * (agent.transform.position - damageDealer.Owner.transform.position).normalized;
            agent.Stagger(pushForce);
        }
#endif

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

        public Camera viewCamera;

        public Agent agent;

#else
        public float ScreenPosX => 0f;

        public float ScreenPosY => 0f;
#endif

#endregion
    }
}

#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
using UnityEditor;
using ShapeGrammar;
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

        public World World { get; set; }

#if NOESIS
        public Behaviors Behaviors { get; set; }

        public GeometryMaker<Agent> GeometryMaker { get; set; }

        public Agent MakeGeometry()
        {
            var agent = GeometryMaker.CreateGeometry();
            agent.CharacterState = this;
            return agent;
        }
#endif

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

#if NOESIS
        [SerializeField]
#endif
        private FloatRange _posture;
        public FloatRange Posture
        {
            get { return _posture; }
            set { _posture = value; PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
#endif
        private CharacterProperties _properties;
        public CharacterProperties Prop
        {
            get { return _properties; }
            set { _properties = value; OnPropertyChanged(this); }
        }

        public Action OnUpdate { get; set; }

        public DamageTaken DamageTaken { get; }

        public bool PostureBroken { get; set; }

        public bool Dead => Health <= 0f;

        public Inventory Inventory { get; set; }

        public CharacterState()
        {
            Health = new FloatRange(100, 100);
            Stamina = new FloatRange(20, 20);
            Posture = 10f;
            Inventory = new EnemyInventory(this);
            DamageTaken = new DamageTaken(2f);
            Prop = new CharacterProperties();
            Prop.Character = this;
            OnUpdate = () => { };
#if NOESIS
            Behaviors = new Behaviors();
#endif
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

            DamageTaken.Update(Time.fixedDeltaTime);

            OnUpdate();

            UIUpdate();
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

            DamageTaken.AddDamage(damageDealer.Damage);
        }
#endif

        public bool SetItemToSlot(SlotType slotType, ItemState item)
        {
            var slot = slotType.IsWeapon() ?
                Inventory.EquipWeapon(slotType, item) :
                Inventory.AddItem(slotType, item);
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

        public virtual void UIUpdate()
        {
            var camera = viewCamera;

            var agentUiPos = camera.WorldToScreenPoint(Agent.transform.position + Agent.UIOffset * Vector3.up);
            UIScreenPosX = agentUiPos.x;
            UIScreenPosY = agentUiPos.y;

            var agentCenterPos = camera.WorldToScreenPoint(Agent.transform.position + Agent.CenterOffset * Vector3.up);
            ScreenPosX = agentCenterPos.x;
            ScreenPosY = agentCenterPos.y;

            VisibleOnCamera = ExtensionMethods.IsPointInDirection(camera.transform.position, camera.transform.forward, Agent.transform.position) &&
                                            (camera.transform.position - Agent.transform.position).magnitude < 25f;
        }
#else
        public float ScreenPosX => 0f;

        public float ScreenPosY => 0f;
#endif

#endregion
    }

    public class DamageTaken : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private float timeOutDuration;

        private float timeRemaining;

        private bool _timedOut;
        public bool TimedOut
        {
            get { return _timedOut; }
            set { _timedOut = value; PropertyChanged.OnPropertyChanged(this); }
        }

        private float _damage;

        public float Damage
        {
            get { return _damage; }
            set { _damage = value; PropertyChanged.OnPropertyChanged(this); }
        }

        public DamageTaken(float timeOutDuration)
        {
            this.timeOutDuration = timeOutDuration;
            Damage = 0f;
            TimedOut = true;
            timeRemaining = 0f;
        }

        public void AddDamage(float damage)
        {
            timeRemaining = timeOutDuration;
            Damage += damage;
            TimedOut = false;
        }

        public void Update(float deltaT)
        {
            timeRemaining -= deltaT;
            if (!TimedOut && timeRemaining < 0)
            {
                TimedOut = true;
                Damage = 0f;
            }
        }
    }
}

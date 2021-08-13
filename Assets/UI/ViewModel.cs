#if UNITY_5_3_OR_NEWER
#define NOESIS
using Noesis;
using UnityEngine;
using Color = Noesis.Color;
#else
using System;
using System.Windows.Input;
using System.Windows.Media;

#endif
using ContentGeneration.Assets.UI.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ContentGeneration.Assets.UI.Util;
using System.Linq;

namespace ContentGeneration.Assets.UI
{
    /// <summary>
    /// Logic for application ViewModel
    /// </summary>
    public partial class ViewModel :
#if NOESIS
        MonoBehaviour,
#endif
        INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Color TopColor { get; set; }
        public Color BottomColor { get; set; }
        public DelegateCommand ButtonClicked { get; set; }

        PlayerCharacterState _playerState;
        public PlayerCharacterState PlayerState 
        {
            get => _playerState; 
            set
            {
                _playerState = value;
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        ObservableCollection<CharacterState> enemies;
        public ObservableCollection<CharacterState> Enemies
        {
            get => enemies;
            private set
            {
                enemies = value;
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                MessageOpacity = 1f;
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        float _messageOpacity;

        public float MessageOpacity
        {
            get => _messageOpacity;
            set
            {
                _messageOpacity = value;
                PropertyChanged.OnPropertyChanged(this);
            }
        }


#if NOESIS
        private void Awake()
        {
            TopColor = Color.FromRgb(17, 102, 157);
            BottomColor = Color.FromRgb(18, 57, 87);

            ButtonClicked = new DelegateCommand((p) =>
            {
                UnityEngine.Debug.Log("Button clicked");
            });
        }

        void Start()
        {
            Enemies = new ObservableCollection<CharacterState>(Object.FindObjectsOfType<CharacterReference>().Select(r => r.CharacterState));
        }

        float inputDelay;

        bool CanTakeInput()
        {
            return inputDelay <= 0f;
        }

        void Update()
        {
            MessageOpacity *= 0.99f;

            // Let's handle input in update for now...
            inputDelay -= Time.deltaTime;
            var input = new Vector2(
                Input.GetAxis("Noesis_Horizontal"),
                Input.GetAxis("Noesis_Vertical")
                );
            if(input.magnitude >= 0.5 && CanTakeInput())
            {
                if (PlayerState.PlayerInventory.Active)
                {
                    PlayerState.PlayerInventory.MoveCursor(GetInputDirection(input.x), GetInputDirection(-input.y));
                    inputDelay = 0.15f;
                }
                else
                {
                    PlayerState.PlayerInventory.ChangeSelected(input.x > 0f);
                    inputDelay = 0.3f;
                }
            }

            if (Input.GetButtonDown("Noesis_Accept"))
            {
                PlayerState.PlayerInventory.HandleClick();
            }

            if (Input.GetButtonDown("Noesis_Menu"))
            {
                PlayerState.InteractingWithUI = !PlayerState.InteractingWithUI;
            }

            if (Input.GetButtonDown("DropItem"))
            {
                PlayerState.PlayerInventory.DropItem();
            }
        }

        int GetInputDirection(float i)
        {
            if (i > 0.1f)
                return 1;
            else if (i > -0.1f)
                return 0;
            else
                return -1;
        }
#else
        public ViewModel()
        {
            TopColor = Color.FromRgb(17, 102, 157);
            BottomColor = Color.FromRgb(18, 57, 87);

            PlayerState = new PlayerCharacterState();
            Enemies = new ObservableCollection<CharacterState>();

            Message = "Sample text";
            MessageOpacity = 0.5f;

            ButtonClicked = new DelegateCommand((p) =>
            {
                Console.WriteLine("Button clicked");
            });
        }
#endif
    }
}

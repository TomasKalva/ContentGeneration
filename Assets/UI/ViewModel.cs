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

        void Update()
        {
            MessageOpacity *= 0.99f;
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

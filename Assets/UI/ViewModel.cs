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
        public Color TopColor { get; set; }
        public Color BottomColor { get; set; }
        public DelegateCommand ButtonClicked { get; set; }

        CharacterState _playerState;
        public CharacterState PlayerState 
        {
            get => _playerState; 
            set
            {
                _playerState = value;
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        public ObservableCollection<CharacterState> Enemies { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

#if NOESIS
        private void Awake()
        {
            TopColor = Color.FromRgb(17, 102, 157);
            BottomColor = Color.FromRgb(18, 57, 87);

            Enemies = new ObservableCollection<CharacterState>(Object.FindObjectsOfType<CharacterState>());


            ButtonClicked = new DelegateCommand((p) =>
            {
                UnityEngine.Debug.Log("Button clicked");
            });
        }
#else
        public ViewModel()
        {
            TopColor = Color.FromRgb(17, 102, 157);
            BottomColor = Color.FromRgb(18, 57, 87);

            PlayerState = new CharacterState();
            Enemies = new ObservableCollection<CharacterState>();

            ButtonClicked = new DelegateCommand((p) =>
            {
                Console.WriteLine("Button clicked");
            });
        }
#endif
    }
}

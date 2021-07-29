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

namespace ContentGeneration.Assets.UI
{
    /// <summary>
    /// Logic for application ViewModel
    /// </summary>
    public partial class ViewModel
    {
        public Color TopColor { get; set; }
        public Color BottomColor { get; set; }
        public DelegateCommand ButtonClicked { get; }

        public CharacterState PlayerState { get; set; }

        public ObservableCollection<CharacterState> Enemies { get; }

        public ViewModel()
        {
            TopColor = Color.FromRgb(17, 102, 157);
            BottomColor = Color.FromRgb(18, 57, 87);


#if NOESIS
            PlayerState =  GameObject.Find("Player").GetComponent<CharacterState>();
            Enemies = new ObservableCollection<CharacterState>(Object.FindObjectsOfType<CharacterState>());
#else
            PlayerState = new CharacterState();
            Enemies = new ObservableCollection<CharacterState>();
#endif

            ButtonClicked = new DelegateCommand((p) =>
            {
#if NOESIS
                UnityEngine.Debug.Log("Button clicked");
#else
                Console.WriteLine("Button clicked");
#endif
            });
        }
    }
}

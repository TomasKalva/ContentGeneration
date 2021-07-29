﻿#if UNITY_5_3_OR_NEWER
#define NOESIS
using Noesis;
#else
using System;
using System.Windows.Input;
using System.Windows.Media;

#endif
using ContentGeneration.Assets.UI.Model;

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

        public CharacterState PlayerState { get; }

        public ViewModel()
        {
            TopColor = Color.FromRgb(17, 102, 157);
            BottomColor = Color.FromRgb(18, 57, 87);

            PlayerState = new CharacterState();

            ButtonClicked = new DelegateCommand((p) =>
            {
#if NOESIS
                UnityEngine.Debug.Log("Button clicked");
                TopColor = Color.FromRgb(0, 0, 0);
#else
                Console.WriteLine("Button clicked");
#endif
            });
        }
    }
}

#if UNITY_5_3_OR_NEWER
#define NOESIS
using Noesis;
using UnityEngine;
using Color = Noesis.Color;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ContentGeneration.Assets.UI.Util
{
    public static class PropertyChangedEventHandlerExtensions
    {
        public static void OnPropertyChanged(this PropertyChangedEventHandler propertyChanged, INotifyPropertyChanged thisInstance, [CallerMemberName] string name = null)
        {
            propertyChanged?.Invoke(thisInstance, new PropertyChangedEventArgs(name));
        }
    }
#if NOESIS
    public static class GameViewModel
    {
        public static ViewModel ViewModel;

        static GameViewModel(){
            var camera = GameObject.Find("Main Camera");
            ViewModel = camera.GetComponent<ViewModel>();
        }
    }
#endif
}

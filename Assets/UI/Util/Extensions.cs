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
    /// <summary>
    /// Has reference to the view model. Should be reset manually when view model changes.
    /// </summary>
    public static class GameViewModel
    {
        static ViewModel _viewModel;

        public static ViewModel ViewModel 
        {
            get
            {
                if(_viewModel == null)
                {
                    var camera = GameObject.Find("Main Camera");
                    _viewModel = camera.GetComponent<ViewModel>();
                }
                return _viewModel;
            } 
        }

        public static void Reset()
        {
            _viewModel = null;
        }
    }
#endif
}

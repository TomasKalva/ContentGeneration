#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
#endif
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OurFramework.Game;

namespace OurFramework.UI.Util
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

#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
#endif
using ContentGeneration.Assets.UI.Util;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ContentGeneration.Assets.UI.Model
{
    public class InteractiveObjectState :
#if NOESIS
        MonoBehaviour,
#endif
        INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

#if NOESIS
        [SerializeField]
#endif
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
#endif
        private string _messageOnInteract;
        public string MessageOnInteract
        {
            get { return _messageOnInteract; }
            set { _messageOnInteract = value; PropertyChanged.OnPropertyChanged(this); }
        }
    }
}

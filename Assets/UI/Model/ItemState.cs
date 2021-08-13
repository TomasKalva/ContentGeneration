#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using ContentGeneration.Assets.UI.Util;

namespace ContentGeneration.Assets.UI.Model
{
#if NOESIS
    [Serializable]
#endif
    public class ItemState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

#if NOESIS
        [SerializeField]
#endif
        string _name;

        public string Name 
        { 
            get => _name;
            private set { _name = value; PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
#endif
        string _description;

        public string Description
        {
            get => _description;
            private set { _description = value; PropertyChanged.OnPropertyChanged(this); }
        }

        public virtual void Use()
        {
#if NOESIS
            Debug.Log($"Using {Name}");
#endif
        }

        public virtual void Drop()
        {
#if NOESIS
            Debug.Log($"Dropping {Name}");
#endif
        }
    }
}

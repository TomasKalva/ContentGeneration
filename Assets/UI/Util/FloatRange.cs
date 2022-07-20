#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEditor;
using UnityEngine;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ContentGeneration.Assets.UI.Util
{

    /// <summary>
    /// (0, Maximum]
    /// </summary>
#if NOESIS
    [Serializable]
#endif
    public class FloatRange : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(INotifyPropertyChanged thisInstance, [CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(thisInstance, new PropertyChangedEventArgs(name));
        }

        public const float MAX_VALUE = 1_000_000f;

#if NOESIS
        [SerializeField]
#endif
        float _max;
        public float Maximum 
        { 
            get => _max; 
            set
            {
                _max = value;
                OnPropertyChanged(this);
            } 
        }

#if NOESIS
        [SerializeField]
#endif
        float _value;
        public float Value 
        {
            get => _value; 
            set 
            { 
                _value = Math.Max(0f, Math.Min(value, Maximum));
                OnPropertyChanged(this);
            } 
        }
        public bool Unbound => Maximum == MAX_VALUE;
        public bool Full() => Value == Maximum;
        public bool Empty() => Value == 0f;

        public FloatRange(float maximum, float value)
        {
            Maximum = maximum;
            this._value = 0f;
            Value = value;
        }

        public FloatRange()
        {
            Maximum = MAX_VALUE;
            _value = 0;
        }

        public static FloatRange operator -(FloatRange r1, float d) => new FloatRange(r1.Maximum, r1.Value - d);
        public static FloatRange operator +(FloatRange r1, float d) => new FloatRange(r1.Maximum, r1.Value + d);
        public static FloatRange operator *(FloatRange r1, float d) => new FloatRange(r1.Maximum, r1.Value * d);
        public static bool operator >=(FloatRange r1, float d) => r1.Value >= d;
        public static bool operator <=(FloatRange r1, float d) => r1.Value <= d;
        public static implicit operator FloatRange(float d) => new FloatRange(d, d);
        public static implicit operator float(FloatRange r) => r.Value;

    }
}

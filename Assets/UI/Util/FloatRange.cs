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

namespace ContentGeneration.Assets.UI.Util
{

    /// <summary>
    /// (0, Maximum]
    /// </summary>
#if NOESIS
    [Serializable]
#endif
    public class FloatRange
    {
        public const float MAX_VALUE = 1_000_000f;

#if NOESIS
        [SerializeField]
#endif
        private float max;
        public float Maximum { get => max; set => max = value; }

#if NOESIS
        [SerializeField]
#endif
        private float value;
        public float Value { get => value; set { this.value = (float)Math.Max(0d, Math.Min(value, Maximum)); } }
        public bool Unbound => Maximum == MAX_VALUE;
        public bool Full() => Value == Maximum;

        public FloatRange(float maximum, float value)
        {
            Maximum = maximum;
            this.value = 0f;
            Value = value;
        }

        public FloatRange()
        {
            Maximum = MAX_VALUE;
            value = 0;
        }

        public static FloatRange operator -(FloatRange r1, float d) => new FloatRange(r1.Maximum, r1.Value - d);
        public static FloatRange operator +(FloatRange r1, float d) => new FloatRange(r1.Maximum, r1.Value + d);
        public static FloatRange operator *(FloatRange r1, float d) => new FloatRange(r1.Maximum, r1.Value * d);
        public static bool operator >=(FloatRange r1, float d) => r1.Value >= d;
        public static bool operator <=(FloatRange r1, float d) => r1.Value <= d;
        public static implicit operator FloatRange(float d) => new FloatRange(MAX_VALUE, d);
        public static implicit operator float(FloatRange r) => r.Value;

    }
}

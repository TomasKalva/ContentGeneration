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
    public class FloatRange
    {
        public const float MAX_VALUE = 1_000_000f;

        public float Maximum { get; set; }
        private float _value;
        public float Value { get => _value; set { _value = (float)Math.Max(0d, Math.Min(value, Maximum)); } }
        public bool Unbound => Maximum == MAX_VALUE;
        public bool Full() => Value == Maximum;

        public FloatRange(float maximum, float value)
        {
            Maximum = maximum;
            _value = 0f;
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
        public static implicit operator FloatRange(float d) => new FloatRange(MAX_VALUE, d);
        public static implicit operator float(FloatRange r) => r.Value;

    }
}

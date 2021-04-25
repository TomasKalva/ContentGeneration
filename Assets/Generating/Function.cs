using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Region;

namespace Assets
{
    abstract class Function
    {
        /// <summary>
        /// Takes coordinates and its height.
        /// </summary>
        public abstract float Apply(Point p, float h);
    }

    class Constant : Function
    {
        float c;

        public Constant(float c)
        {
            this.c = c;
        }

        public override float Apply(Point p, float h)
        {
            return c;
        }
    }

    class Linear : Function
    {
        float a;

        public Linear(float a)
        {
            this.a = a;
        }

        public override float Apply(Point p, float h)
        {
            return a * h;
        }
    }

    class Parabola: Function
    {
        float a;
        float c;
        Vector2 b1;
        Vector2 b2;

        public Parabola(float height, float width, Vector2 b1, Vector2 b2)
        {
            this.c = height;
            this.a = -height / (width * width);
            this.b1 = b1;
            this.b2 = b2;
        }

        public override float Apply(Point p, float h)
        {
            var vec = p.GetVector(b1, b2);
            var v = a * (vec.x * vec.x + vec.y * vec.y) + c;
            return c > 0 ? Math.Max(0f, v) : Math.Min(0f, v);
        }
    }
}

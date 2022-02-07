using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public interface IIntDistr
    {
        public int Sample();
    }

    public class RandomIntDistr : IIntDistr
    {
        int min, max;

        public RandomIntDistr(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public int Sample()
        {
            return UnityEngine.Random.Range(min, max);
        }
    }

    public class IntDistr : IIntDistr
    {
        int start, step;
        int current;

        public IntDistr(int start = 0, int step = 1)
        {
            this.start = start;
            this.step = step;
            this.current = start - step;
        }

        public int Sample()
        {
            current += step;
            return current;
        }
    }

    public class IntervalDistr : IIntDistr
    {
        IIntDistr seq;
        int min, max;

        public IntervalDistr(IIntDistr seq, int min, int max)
        {
            this.seq = seq;
            this.min = min;
            this.max = max;

            Debug.Assert(max > min);
        }

        public int Sample()
        {
            var i = seq.Sample();
            return i.Mod(max - min) + min;
        }
    }

    /// <summary>
    /// Binomial distribution given by center and width.
    /// </summary>
    public class SlopeDistr : IIntDistr
    {
        float p;
        int center, width;
        int size;

        public SlopeDistr(int center = 0, int width = 1, float rightness = 0.5f)
        {
            this.center = center;
            this.width = width;
            this.p = rightness;
            size = 1 + 2 * width;
        }

        public int Sample()
        {
            var successes = 0;
            for (int i = 0; i < size; i++)
            {
                if (UnityEngine.Random.Range(0f, 1f) < p)
                {
                    successes++;
                }
            }
            return center + successes - width;
        }
    }
    public class UniformDistr : IIntDistr
    {
        int min, max;

        public UniformDistr(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public int Sample()
        {
            return UnityEngine.Random.Range(min, max);
        }
    }
}

using System.Linq;
using UnityEngine;

namespace OurFramework.Util
{
    /// <summary>
    /// Returns a number.
    /// </summary>
    public interface IDistribution<T>
    {
        public T Sample();
    }

    /// <summary>
    /// Returns numbers starting with start, incremented by step.
    /// </summary>
    public class IntSeqDistr : IDistribution<int>
    {
        int start, step;
        int current;

        public IntSeqDistr(int start = 0, int step = 1)
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

    /// <summary>
    /// Returns numbers of the given distribution clamped to the interval.
    /// </summary>
    public class IntervalDistr : IDistribution<int>
    {
        IDistribution<int> seq;
        int min, max;

        public IntervalDistr(IDistribution<int> seq, int min, int max)
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
    public class SlopeDistr : IDistribution<int>
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
                if (MyRandom.Range(0f, 1f) < p)
                {
                    successes++;
                }
            }
            return center + successes - width;
        }
    }

    /// <summary>
    /// Returns random number between min and max.
    /// </summary>
    public class UniformDistr : IDistribution<int>
    {
        int min, max;

        public UniformDistr(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public int Sample()
        {
            return MyRandom.Range(min, max);
        }
    }

    /// <summary>
    /// Returns the item with probability proportional to its weight. Weights can be
    /// arbitrary positive numbers.
    /// </summary>
    public class WeightedDistr<T> : IDistribution<T>
    {
        struct WeightItemPair
        {
            public float weight;
            public T value;

            public WeightItemPair(float weight, T value)
            {
                this.weight = weight;
                this.value = value;
            }
        }

        WeightItemPair[] Items { get; }

        public WeightedDistr(params (float, T)[] items)
        {
            Items = items.Select(item => new WeightItemPair(item.Item1, item.Item2)).ToArray();
        }

        public T Sample()
        {
            float getWeight(WeightItemPair wiPair) => wiPair.weight;
            return Items.GetRandom(getWeight).value;
        }
    }

    /// <summary>
    /// Returns the value.
    /// </summary>
    public class ConstDistrFloat : IDistribution<float>
    {
        float val;

        public ConstDistrFloat(float val)
        {
            this.val = val;
        }

        public float Sample()
        {
            return val;
        }
    }

    /// <summary>
    /// Returns the value.
    /// </summary>
    public class ConstDistr : IDistribution<int>
    {
        int val;

        public ConstDistr(int val)
        {
            this.val = val;
        }

        public int Sample()
        {
            return val;
        }
    }
}

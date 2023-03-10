using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurFramework.Util
{
    /// <summary>
    /// Thread safe random.
    /// </summary>
    static class MyRandom
    {
        static readonly System.Random GlobalRandom = new System.Random();

        [ThreadStatic] static System.Random _threadSafeRandom;

        static System.Random ThreadSafeRandom
        {
            get
            {
                if (_threadSafeRandom == null)
                {
                    int seed;
                    lock (GlobalRandom)
                    {
                        seed = GlobalRandom.Next();
                    }
                    _threadSafeRandom = new System.Random(seed);
                }
                return _threadSafeRandom;
            }
        }

        public static int Range(int min, int max) => ThreadSafeRandom.Next(min, max);
        public static float Range(float min, float max) => Value * (max - min) + min;
        public static float Value => (float)ThreadSafeRandom.NextDouble();
    }
}

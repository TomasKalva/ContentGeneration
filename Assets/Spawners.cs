using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assets.Region;
using UnityEngine;

namespace Assets
{
    abstract class Spawner
    {
        public bool Finished { get; private set; }
        IEnumerator progress;
        public void Step(Agents agents, TerrainMap map)
        {
            if (progress == null)
                progress = Spawn(agents, map);

            if (!progress.MoveNext())
            {
                Finished = true;
            }
        }
        protected abstract IEnumerator Spawn(Agents agents, TerrainMap map);
    }

    class RandomMapSpawner : Spawner
    {
        protected override IEnumerator Spawn(Agents agents, TerrainMap map)
        {
            int iterations = 10000;
            int c = 20;
            while (c-- > 0) {
                var averagerBrush = Brushes.SquareAverageBrush(2, 0.01f);
                var strongAveragerBrush = Brushes.SquareAverageBrush(2, 0.1f);
                var largeHillBrush = Brushes.HillBrush(0.004f, 5);
                var smallHillBrush = Brushes.HillBrush(0.03f, 3);
                var riverBrush = Brushes.HillBrush(-0.006f, 4);


                agents.AddAgent(new BasicAgent(largeHillBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                //agents.AddAgent(new BasicAgent(smallHillBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                //agents.AddAgent(new BasicAgent(smallHillBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(riverBrush).SetSteps(iterations).SetPosition(map.RandomPoint));

                agents.AddAgent(new BasicAgent(averagerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(averagerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(averagerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(averagerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(averagerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(averagerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(averagerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(strongAveragerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(strongAveragerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(strongAveragerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));

                for (int i = 0; i < iterations; i++)
                {
                    yield return null;
                }
            }
        }
    }

    class SmoothingAgentsSpawner : Spawner
    {
        protected override IEnumerator Spawn(Agents agents, TerrainMap map)
        {
            var averagerBrush = Brushes.SquareAverageBrush(1, 0.5f);
            for (int i = 0; i < map.Width; i++)
            {
                for (int j = 0; j < map.Height; j++)
                {
                    averagerBrush.Stroke(map, new Vector2(i, j));
                    /*var st = new Vector2(i / (float)map.Width, j / (float)map.Height) / 5f;
                    map[i, j] = 10f * myMap(st);*/
                }
            }
            yield return null;
            /*int iterations = 10000;
            int c = 20;
            while (c-- > 0)
            {
                var averagerBrush = Brushes.SquareAverageBrush(1, 0.01f);
                var strongAveragerBrush = Brushes.SquareAverageBrush(2, 0.1f);

                agents.AddAgent(new BasicAgent(averagerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(averagerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(averagerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(averagerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(averagerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(averagerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));
                agents.AddAgent(new BasicAgent(averagerBrush).SetSteps(iterations).SetPosition(map.RandomPoint));

                for (int i = 0; i < iterations; i++)
                {
                    yield return null;
                }
            }*/
        }
    }


    class VolcanoSpawner : Spawner
    {
        int radius;
        Point location;
        int holeSize;
        int raisingPhaseCount;
        int smoothingPhaseCount;
        int finishingPhaseCount;

        public VolcanoSpawner(int radius, Point location, int holeSize, int raisingPhaseCount, int smoothingPhaseCount, int finishingPhaseCount)
        {
            this.radius = radius;
            this.location = location;
            this.holeSize = holeSize;
            this.raisingPhaseCount = raisingPhaseCount;
            this.smoothingPhaseCount = smoothingPhaseCount;
            this.finishingPhaseCount = finishingPhaseCount;
        }

        protected override IEnumerator Spawn(Agents agents, TerrainMap map)
        {
            Area spawnArea = new Circle(8);
            int count = raisingPhaseCount;
            while (count-- > 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    var smallHillBrush = Brushes.HillBrush(0.01f, 4);
                    var smallerHillBrush = Brushes.HillBrush(0.01f, 2);

                    var pos = spawnArea.GetAbsolutePoints(location).GetRandom();
                    agents.AddAgent(new RepulsedAgent(smallHillBrush, location).SetPosition(pos).SetSteps(radius));
                    agents.AddAgent(new RepulsedAgent(smallerHillBrush, location).SetPosition(pos).SetSteps(radius));
                }
                foreach (var _ in agents.WaitUntilAgentsFinish())
                {
                    yield return null;
                }
            }

            count = smoothingPhaseCount;
            while (count-- > 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    var averagingBrush = Brushes.SquareAverageBrush(3, 0.01f);

                    var pos = spawnArea.GetAbsolutePoints(location).GetRandom();
                    agents.AddAgent(new RepulsedAgent(averagingBrush, location).SetPosition(pos).SetSteps(radius));
                }
                foreach (var _ in agents.WaitUntilAgentsFinish())
                {
                    yield return null;
                }
            }

            count = finishingPhaseCount;
            while (count-- > 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    var smallerHillBrush = Brushes.HillBrush(0.01f, 3);

                    var pos = spawnArea.GetAbsolutePoints(location).GetRandom();
                    agents.AddAgent(new DescendingAgent(smallerHillBrush).SetPosition(pos).SetSteps(radius));
                }
                foreach (var _ in agents.WaitUntilAgentsFinish())
                {
                    yield return null;
                }
            }


        }
    }

    class NoiseMapSpawner : Spawner
    {
        float fBM(Vector2 st, float lacunarity, float gain)
        {
            float val = 0.0f;
            float f = 1.0f;
            float a = 0.5f;
            for (int i = 0; i < 8; i++)
            {
                var stf = st * f;
                val += a * Mathf.PerlinNoise(stf.x, stf.y);
                f *= lacunarity;
                a *= gain;
            }
            return val;
        }

        float fract(float x) => x % 1.0f;

        float myMap(Vector2 st)
        {
            st *= 5.0f;

            float x = fBM(st, 1.788f, 0.212f);
            x *= x;
            x *= x;
            x *= x;

            float height = fBM(new Vector2(x, x), 2.740f, 0.740f);

            float low = 0.744f;
            float middle = 0.792f;
            float high = 0.892f;
            float super_high = 1.0f;

            var color = 0.0f;

            color += fract(Mathf.SmoothStep(low, middle, height)) ;
            color += fract(Mathf.SmoothStep(middle, high, height)) ;
            return height;
        }

        protected override IEnumerator Spawn(Agents agents, TerrainMap map)
        {
            for (int i = 0; i < map.Width; i++)
            {
                for (int j = 0; j < map.Height; j++)
                {
                    var st = new Vector2(i / (float)map.Width, j / (float)map.Height);
                    map[i, j] = myMap(st) * 10f;
                }
            }
            yield return null;
        }
    }
}

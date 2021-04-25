using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Region;

namespace Assets
{
    static class Brushes
    {
        public static Brush HillBrush(float height, int radius) => new AreaChangerBrush(new Circle(radius), new Parabola(height, radius * 0.1f, DrawingParameters.b1, DrawingParameters.b2));
        public static Brush SquareAverageBrush(int radius, float averagingSpeed) {
            int side = 1 + 2 * radius;
            return new AveragerBrush(new Rectangle(-radius, -radius, radius, radius), new Linear(1f / (side * side)), averagingSpeed);
        }
    }


    abstract class Brush
    {
        public abstract void Stroke(TerrainMap map, Vector2 position);
    }

    class AreaChangerBrush : Brush
    {
        private Area area;
        private Function function;

        public AreaChangerBrush(Area area, Function function)
        {
            this.area = area;
            this.function = function;
        }
        public override void Stroke(TerrainMap map, Vector2 position)
        {
            foreach (var p in area.GetPoints())
            {
                var coords = area.ToAbsolute((Point)position, p);
                map[coords] += function.Apply(p, map[coords]);
            }
        }
    }

    class AveragerBrush : Brush
    {
        Area area;
        Function function;
        float averagingSpeed;

        public AveragerBrush(Area area, Function function, float averagingSpeed)
        {
            this.area = area;
            this.function = function;
            this.averagingSpeed = averagingSpeed;
        }

        public override void Stroke(TerrainMap map, Vector2 position)
        {
            float totalChange = 0f;
            foreach (var p in area.GetPoints())
            {
                var coords = area.ToAbsolute((Point)position, p);
                totalChange += function.Apply(p, map[coords]);
            }
            foreach (var p in area.GetPoints())
            {
                var coords = area.ToAbsolute((Point)position, p);
                map[coords] = map[coords] * (1 - averagingSpeed) + totalChange * averagingSpeed;
            }
        }
    }

}

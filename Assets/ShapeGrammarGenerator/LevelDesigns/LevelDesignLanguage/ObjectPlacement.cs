using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeGrammar
{
    class ObjectPlacement<T>
    {
        Action<Area, T> placementOp;

        public ObjectPlacement(Action<Area, T> placementOp)
        {
            this.placementOp = placementOp;
        }

        public Placer<T> RandomPlacer(IDistribution<int> count, params (float, Func<T>)[] items)
        {
            return 
                new RandomPlacer<T>(
                    placementOp,
                    new WeightedDistribution<Func<T>>(
                        items
                    ),
                    count
                );
        }

        public Placer<T> EvenPlacer(IEnumerable<T> items)
        {
            return EvenPlacer(items.ToArray());
        }

        public Placer<T> EvenPlacer(params T[] items)
        {
            return
                new EvenPlacer<T>(
                    placementOp,
                    items
                );
        }
    }

    class LinearPath
    {
        public List<Area> Areas { get; }

        public LinearPath(List<Area> areas)
        {
            Areas = areas;
        }

        public Area LastArea() => Areas.LastOrDefault();
    }

    class Branching
    {
        public List<Area> Areas { get; }

        public Branching(List<Area> areas)
        {
            Areas = areas;
        }
    }

    abstract class Placer<T>
    {
        /// <summary>
        /// Puts T into the area.
        /// </summary>
        protected Action<Area, T> PlacementOp { get; }

        protected Placer(Action<Area, T> placementOp)
        {
            PlacementOp = placementOp;
        }

        public abstract void Place(IEnumerable<Area> areas);
    }

    class RandomPlacer<T> : Placer<T>
    {
        WeightedDistribution<Func<T>> ToPlaceF { get; }
        IDistribution<int> Count { get; }

        public RandomPlacer(Action<Area, T> placementOp, WeightedDistribution<Func<T>> toPlaceF, IDistribution<int> count) : base(placementOp)
        {
            ToPlaceF = toPlaceF;
            Count ??= new UniformDistr(1, 2);
        }

        public override void Place(IEnumerable<Area> areas)
        {
            areas.ForEach(area => Enumerable.Range(0, Count.Sample()).ForEach(_ => PlacementOp(area, ToPlaceF.Sample()())));
        }
    }

    class EvenPlacer<T> : Placer<T>
    {
        List<T> ToPlace { get; }

        public EvenPlacer(Action<Area, T> placementOp, IEnumerable<T> toPlace) : base(placementOp)
        {
            ToPlace = toPlace.ToList();
        }

        public override void Place(IEnumerable<Area> areas)
        {
            areas.Shuffle()
                .Zip(ToPlace, (area, toPlace) => new { area, toPlace})
                .ForEach(x => PlacementOp(x.area, x.toPlace));
        }
    }

    /*
    class Iterator<T>
    {
        Stack<T> items { get; }

        public Iterator(IEnumerable<T> items)
        {
            this.items = new Stack<T>(items);
        }

        public T Next() => items.Pop();
        public bool Any() => items.Any();
    }*/
}

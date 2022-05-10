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

        public Placer<Areas, T> RandomPlacer(IDistribution<int> count, params (float, Func<T>)[] items)
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

        public Placer<Areas, T> EvenPlacer(IEnumerable<T> items)
        {
            return EvenPlacer(items.ToArray());
        }

        public Placer<Areas, T> EvenPlacer(params T[] items)
        {
            return
                new EvenPlacer<T>(
                    placementOp,
                    items
                );
        }
    }

    class Areas
    {
        public List<Area> AreasList { get; }

        public Areas(List<Area> areasList)
        {
            AreasList = areasList;
        }

        public Areas Concat(Areas areas) => new Areas(AreasList.Concat(areas.AreasList).ToList());
    }

    class LinearPath : Areas
    {
        public LinearPath(List<Area> areas) : base(areas)
        {
        }

        public Area LastArea() => AreasList.LastOrDefault();
    }

    class Branching : Areas
    {
        public Branching(List<Area> areas) : base(areas)
        {
        }
    }

    abstract class Placer<AreasT, T> where AreasT : Areas
    {
        /// <summary>
        /// Puts T into the area.
        /// </summary>
        protected Action<Area, T> PlacementOp { get; }

        protected Placer(Action<Area, T> placementOp)
        {
            PlacementOp = placementOp;
        }

        public abstract void Place(AreasT areas);
    }

    class RandomPlacer<T> : Placer<Areas, T>
    {
        WeightedDistribution<Func<T>> ToPlaceF { get; }
        IDistribution<int> Count { get; }

        public RandomPlacer(Action<Area, T> placementOp, WeightedDistribution<Func<T>> toPlaceF, IDistribution<int> count) : base(placementOp)
        {
            ToPlaceF = toPlaceF;
            Count ??= new UniformDistr(1, 2);
        }

        public override void Place(Areas areas)
        {
            areas.AreasList.ForEach(area => Enumerable.Range(0, Count.Sample()).ForEach(_ => PlacementOp(area, ToPlaceF.Sample()())));
        }
    }

    class EvenPlacer<T> : Placer<Areas, T>
    {
        List<T> ToPlace { get; }

        public EvenPlacer(Action<Area, T> placementOp, IEnumerable<T> toPlace) : base(placementOp)
        {
            ToPlace = toPlace.ToList();
        }

        public override void Place(Areas areas)
        {
            areas.AreasList.Shuffle()
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

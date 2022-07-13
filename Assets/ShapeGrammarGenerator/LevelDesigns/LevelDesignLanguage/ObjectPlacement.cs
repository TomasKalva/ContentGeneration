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
                    count,
                    areas => areas.AreasList
                );
        }

        public Placer<Areas, T> EvenPlacer(IEnumerable<T> items)
        {
            return EvenPlacer(items.ToArray());
        }

        public Placer<Areas, T> EvenPlacer(params T[] items)
        {
            return
                new EvenPlacer<Areas, T>(
                    placementOp,
                    items,
                    areas => areas.AreasList.Shuffle()
                );
        }

        public Placer<Areas, T> DeadEndPlacer(IEnumerable<T> items)
        {
            return DeadEndPlacer(items.ToArray());
        }

        public Placer<Areas, T> DeadEndPlacer(params T[] items)
        {
            return
                new EvenPlacer<Areas, T>(
                    placementOp,
                    items,
                    areas => areas.AreasList.OrderBy(area => area.EdgesFrom.Count)
                );
        }

        public Placer<Areas, T> ProgressFunctionPlacer(ProgressFactory<T> progressFunc, IDistribution<int> count)
        {
            return
                new ProgressFunctionPlacer<Areas, T>(
                    placementOp,
                    count,
                    progressFunc
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

        //public IEnumerable<Area> DeadEnds() => AreasList.Where(area => !area.EdgesFrom.Any());
    }

    abstract class Placer<AreasT, T> where AreasT : Areas
    {
        /// <summary>
        /// Puts T into the area.
        /// </summary>
        protected Action<Area, T> PlacementOp { get; }
        public delegate IEnumerable<Area> AreasPrioritizer(AreasT areasT);
        protected AreasPrioritizer Prioritizer { get; }

        protected Placer(Action<Area, T> placementOp, AreasPrioritizer prioritizer)
        {
            PlacementOp = placementOp;
            Prioritizer = prioritizer;
        }

        public abstract void Place(AreasT areas);
    }

    class RandomPlacer<T> : Placer<Areas, T>
    {
        WeightedDistribution<Func<T>> ToPlaceF { get; }
        IDistribution<int> Count { get; }

        public RandomPlacer(Action<Area, T> placementOp, WeightedDistribution<Func<T>> toPlaceF, IDistribution<int> count, AreasPrioritizer prioritizer) : base(placementOp, prioritizer)
        {
            ToPlaceF = toPlaceF;
            Count ??= new UniformDistr(1, 2);
        }

        public override void Place(Areas areas)
        {
            Prioritizer(areas).ForEach(area => Enumerable.Range(0, Count.Sample()).ForEach(_ => PlacementOp(area, ToPlaceF.Sample()())));
        }
    }

    class EvenPlacer<AreasT, T> : Placer<AreasT, T> where AreasT : Areas
    {
        List<T> ToPlace { get; }

        public EvenPlacer(Action<Area, T> placementOp, IEnumerable<T> toPlace, AreasPrioritizer prioritizer) : base(placementOp, prioritizer)
        {
            ToPlace = toPlace.ToList();
        }

        public override void Place(AreasT areas)
        {
            Prioritizer(areas)
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

    public delegate T ProgressFactory<T>(float progress);

    class ProgressFunctionPlacer<AreasT, T> : Placer<AreasT, T> where AreasT : Areas
    {

        IDistribution<int> Count { get; }
        ProgressFactory<T> TFactory { get; }

        public ProgressFunctionPlacer(Action<Area, T> placementOp, IDistribution<int> count, ProgressFactory<T> progressFunction) : base(placementOp, areas => areas.AreasList)
        {
            Count = count;
            TFactory = progressFunction;
        }

        public override void Place(AreasT areas)
        {
            float totalAreas = areas.AreasList.Count;
            Prioritizer(areas).ForEach((area, i) =>
                Enumerable.Range(0, Count.Sample())
                    .ForEach(_ => PlacementOp(area, TFactory(i / (totalAreas - 1f)))));
        }
    }
}

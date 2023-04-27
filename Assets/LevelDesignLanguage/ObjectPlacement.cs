using System;
using System.Collections.Generic;
using System.Linq;
using OurFramework.Util;

namespace OurFramework.LevelDesignLanguage
{
    /// <summary>
    /// Declarations of placers.
    /// </summary>
    class ObjectPlacement<Object>
    {
        Action<Area, Object> placementOp;

        public ObjectPlacement(Action<Area, Object> placementOp)
        {
            this.placementOp = placementOp;
        }

        /// <summary>
        /// Places objects randomly.
        /// </summary>
        public Placer<Areas, Object> RandomAreaPlacer(IDistribution<int> count, params (float, Func<Object>)[] items)
        {
            return 
                new RandomAreaPlacer<Object>(
                    placementOp,
                    new WeightedDistr<Func<Object>>(
                        items
                    ),
                    count,
                    areas => areas.AreasList
                );
        }

        /// <summary>
        /// Places the given number of objects randomly across all areas.
        /// </summary>
        public Placer<Areas, Object> RandomAreasPlacer(IDistribution<int> count, params Func<Object>[] items)
        {
            return RandomAreasPlacer(count, items.Select(item => (1f, item)).ToArray());
        }

        /// <summary>
        /// Places the given number of objects randomly across all areas.
        /// </summary>
        public Placer<Areas, Object> RandomAreasPlacer(IDistribution<int> count, params (float, Func<Object>)[] items)
        {
            return
                new RandomAreasPlacer<Object>(
                    placementOp,
                    new WeightedDistr<Func<Object>>(
                        items
                    ),
                    count
                );
        }

        /// <summary>
        /// Places objects randomly.
        /// </summary>
        public Placer<Areas, Object> RandomAreaPlacer(IDistribution<int> count, params Func<Object>[] items)
        {
            return RandomAreaPlacer(count, items.Select(item => (1f, item)).ToArray());
        }

        /// <summary>
        /// Places objects evenly.
        /// </summary>
        public Placer<Areas, Object> EvenPlacer(IEnumerable<Object> items)
        {
            return EvenPlacer(items.ToArray());
        }

        /// <summary>
        /// Places objects evenly.
        /// </summary>
        public Placer<Areas, Object> EvenPlacer(params Object[] items)
        {
            return
                new EvenPlacer<Areas, Object>(
                    placementOp,
                    items,
                    areas => areas.AreasList.Shuffle()
                );
        }

        /// <summary>
        /// Places objects to dead ends first.
        /// </summary>
        public Placer<Areas, Object> DeadEndPlacer(IEnumerable<Object> items)
        {
            return DeadEndPlacer(items.ToArray());
        }

        /// <summary>
        /// Places objects to dead ends first.
        /// </summary>
        public Placer<Areas, Object> DeadEndPlacer(params Object[] items)
        {
            return
                new EvenPlacer<Areas, Object>(
                    placementOp,
                    items,
                    areas => areas.AreasList.OrderBy(area => area.EdgesFrom.Count)
                );
        }

        /// <summary>
        /// Places objects counts based on progress function.
        /// </summary>
        public Placer<Areas, Object> ProgressFunctionPlacer(ProgressFactory<Object> progressFunc, IDistribution<int> count)
        {
            return
                new ProgressFunctionPlacer<Areas, Object>(
                    placementOp,
                    count,
                    progressFunc
                );
        }
    }

    /// <summary>
    /// Define how to split objects between areas.
    /// </summary>
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

    /// <summary>
    /// Places the given number of objects randomly in each area.
    /// </summary>
    class RandomAreaPlacer<T> : Placer<Areas, T>
    {
        WeightedDistr<Func<T>> ToPlaceF { get; }
        IDistribution<int> Count { get; }

        public RandomAreaPlacer(Action<Area, T> placementOp, WeightedDistr<Func<T>> toPlaceF, IDistribution<int> count, AreasPrioritizer prioritizer) : base(placementOp, prioritizer)
        {
            ToPlaceF = toPlaceF;
            Count = count != null ? count : new UniformDistr(1, 2);
        }

        public override void Place(Areas areas)
        {
            Prioritizer(areas).ForEach(area => Enumerable.Range(0, Count.Sample()).ForEach(_ => PlacementOp(area, ToPlaceF.Sample()())));
        }
    }

    /// <summary>
    /// Places the given number of objects randomly across all areas.
    /// </summary>
    class RandomAreasPlacer<T> : Placer<Areas, T>
    {
        WeightedDistr<Func<T>> ToPlaceF { get; }
        IDistribution<int> Count { get; }

        public RandomAreasPlacer(Action<Area, T> placementOp, WeightedDistr<Func<T>> toPlaceF, IDistribution<int> count) : base(placementOp, null)
        {
            ToPlaceF = toPlaceF;
            Count = count != null ? count : new UniformDistr(1, 2);
        }

        public override void Place(Areas areas)
        {
            var areasList = areas.AreasList;
            Enumerable.Range(0, Count.Sample()).ForEach(_ => PlacementOp(areasList.GetRandom(), ToPlaceF.Sample()()));
        }
    }

    /// <summary>
    /// Places the objects evenly among areas.
    /// </summary>
    class EvenPlacer<AreasT, T> : Placer<AreasT, T> where AreasT : Areas
    {
        List<T> ToPlace { get; }

        public EvenPlacer(Action<Area, T> placementOp, IEnumerable<T> toPlace, AreasPrioritizer prioritizer) : base(placementOp, prioritizer)
        {
            ToPlace = toPlace.ToList();
        }

        public override void Place(AreasT areas)
        {
            Prioritizer(areas).RepeatInfinitely()
                .Zip(ToPlace, (area, toPlace) => new { area, toPlace})
                .ForEach(x => PlacementOp(x.area, x.toPlace));
        }
    }

    public delegate T ProgressFactory<T>(float progress);

    /// <summary>
    /// Picks counts of objects based on a function.
    /// </summary>
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

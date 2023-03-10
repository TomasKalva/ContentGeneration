using System.Collections.Generic;
using System.Linq;

namespace OurFramework.LevelDesignLanguage
{
    /// <summary>
    /// Areas created by the grammar.
    /// </summary>
    class Areas
    {
        public List<Area> AreasList { get; }

        public Areas(List<Area> areasList)
        {
            AreasList = areasList;
        }

        public Areas Concat(Areas areas) => new Areas(AreasList.Concat(areas.AreasList).ToList());
    }

    /// <summary>
    /// Consecutive areas one after another.
    /// </summary>
    class LinearPath : Areas
    {
        public LinearPath(List<Area> areas) : base(areas)
        {
        }

        public Area LastArea() => AreasList.LastOrDefault();
    }

    /// <summary>
    /// Randomly connected areas.
    /// </summary>
    class Branching : Areas
    {
        public Branching(List<Area> areas) : base(areas)
        {
        }
    }

    /// <summary>
    /// One area.
    /// </summary>
    class SingleArea : Areas
    {
        public SingleArea(Area area) : base(new List<Area>() { area })
        {
        }

        public Area Area => AreasList.First();
    }
}

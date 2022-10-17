using System.Collections.Generic;
using System.Linq;

namespace Assets.LevelDesignLanguage
{

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

    class SingleArea : Areas
    {
        public SingleArea(Area area) : base(new List<Area>() { area })
        {
        }

        public Area Get => AreasList.First();
    }
}

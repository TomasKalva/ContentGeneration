using Assets.ShapeGrammarGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeGrammar
{
    public class WorldState
    {
        public delegate WorldState ChangeWorld(WorldState state);

        public LevelGroupElement Added { get; }
        public LevelElement Last { get; }
        public Grid<Cube> Grid { get; }
        public delegate LevelElement TransformPushed(LevelElement levelElement);
        public TransformPushed AfterPushed { get; }

        public WorldState(LevelElement last, Grid<Cube> grid, TransformPushed afterPushed)
        {
            Grid = grid;
            Added = new LevelGroupElement(grid, AreaStyles.None());
            Last = last;
            AfterPushed = afterPushed;
        }

        public WorldState(LevelGroupElement added, LevelElement last, Grid<Cube> grid, TransformPushed afterPushed)
        {
            Grid = grid;
            Added = added;
            Last = last;
            AfterPushed = afterPushed;
        }

        public WorldState ChangeAll(IEnumerable<ChangeWorld> adders)
        {
            return adders.Aggregate(this,
            (addingState, adder) =>
            {
                var newState = adder(addingState);

                return newState.Last == null ? addingState : newState;
            });
        }

        public WorldState TryPush(LevelElement le)
        {
            if (le == null)
                return this;

            var newLe = AfterPushed(le);
            return new WorldState(Added.Merge(newLe), newLe, Grid, AfterPushed);
        }
    }
}

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
        public Grid Grid { get; }
        public delegate LevelElement TransformPushed(LevelElement levelElement);
        public TransformPushed AfterPushed { get; }

        public WorldState(LevelElement last, Grid grid, TransformPushed afterPushed)
        {
            Added = new LevelGroupElement(grid, AreaType.None);
            Last = last;
            AfterPushed = afterPushed;
        }

        public WorldState(LevelGroupElement added, LevelElement last, Grid grid, TransformPushed afterPushed)
        {
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

        public WorldState AddUntilCan(ChangeWorld adder, int maxIterations)
        {
            var newAddingState = this;
            int counter = 0;
            while (true)
            {
                if (counter++ > maxIterations)
                    return newAddingState;

                var newState = adder(newAddingState);

                if (newState == newAddingState)
                    return newAddingState;
                else
                    newAddingState = newState;
            }
        }

        public WorldState TryPush(LevelElement le)
        {
            if (le == null)
                return this;

            var newLe = AfterPushed(le);
            return new WorldState(Added.Merge(newLe), newLe, Grid, AfterPushed);
        }

        public WorldState TryPushIntersecting(LevelElement le)
        {
            if (le == null)
                return this;

            var newLe = AfterPushed(le);
            var added = Added.MinusInPlace(newLe);
            return new WorldState(added.Merge(newLe), newLe, Grid, AfterPushed);
        }

        public WorldState SetElement(LevelElement le)
        {
            return new WorldState(Added, le, Grid, AfterPushed);
        }

        public WorldState ChangeAdded(LevelGroupElement newAdded)
        {
            return new WorldState(newAdded, Last, Grid, AfterPushed);
        }
    }
}

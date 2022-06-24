using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeGrammar
{
    class LevelConstructor
    {
        List<LevelConstructionEvent> LevelConstructionEvents { get; }

        public LevelConstructor()
        {
            LevelConstructionEvents = new List<LevelConstructionEvent>();
        }

        public void AddEvent(int priority, LevelConstruction construction, Condition condition = null)
        {
            LevelConstructionEvents.Add(new LevelConstructionEvent(priority, construction, condition));
        }

        public void Construct()
        {
            var oldEvents = LevelConstructionEvents.ToList();
            LevelConstructionEvents.Clear();
            oldEvents.OrderBy(ev => -ev.Priority).ForEach(ev =>
            {
                //todo: check if the world is not too big yet

                if (!ev.Handle())
                {
                    LevelConstructionEvents.Add(ev);
                }
            });
        }

        public void Destroy()
        {

        }
    }

    /// <summary>
    /// Returns true if construction is finished.
    /// </summary>
    delegate bool LevelConstruction();
    class LevelConstructionEvent
    {
        public int Priority { get; }
        Condition Condition { get; }
        LevelConstruction Construction { get; }

        public LevelConstructionEvent(int priority, LevelConstruction construction, Condition condition = null)
        {
            Priority = priority;
            Construction = construction;
            Condition = condition == null ? () => true : condition;
        }

        public bool Handle()
        {
            if (Condition())
            {
                return Construction();
            }
            return false;
        }
    }

    delegate bool Condition();
}

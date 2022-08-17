using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class LevelConstructor
    {
        List<LevelConstructionEvent> LevelConstructionEvents { get; }

        public LevelConstructor()
        {
            LevelConstructionEvents = new List<LevelConstructionEvent>();
        }

        public void AddEvent(LevelConstructionEvent levelConstructionEvent)
        {
            LevelConstructionEvents.Add(levelConstructionEvent);
        }

        public void Construct()
        {
            var oldEvents = LevelConstructionEvents.ToList();
            LevelConstructionEvents.Clear();
            oldEvents.OrderBy(ev => -ev.Priority).ForEach(ev =>
            {
                //todo: check if the world is not too big yet
                Debug.Log($"Starting: {ev.Name}");
                if (!ev.Handle())
                {
                    LevelConstructionEvents.Add(ev);
                }
                Debug.Log($"Finished: {ev.Name}");
            });
        }

        public void Destroy()
        {

        }
    }

    /// <summary>
    /// Returns true if construction is finished.
    /// </summary>
    public delegate bool LevelConstruction();
    public class LevelConstructionEvent
    {
        public string Name { get; }
        public int Priority { get; }
        Condition Condition { get; }
        LevelConstruction Construction { get; }

        public LevelConstructionEvent(string name, int priority, LevelConstruction construction, Condition condition = null)
        {
            Name = name;
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

    public delegate bool Condition();
}

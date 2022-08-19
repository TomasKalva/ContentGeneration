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
        /// <summary>
        /// Events that are used to construct the current level.
        /// </summary>
        List<LevelConstructionEvent> LevelConstructionEvents { get; set; }
        /// <summary>
        /// Events that are set during the construction by modules.
        /// </summary>
        List<LevelConstructionEvent> NewEvents { get; set; }

        public LevelConstructor()
        {
            LevelConstructionEvents = new List<LevelConstructionEvent>();
            NewEvents = new List<LevelConstructionEvent>();
        }

        public void AddEvent(LevelConstructionEvent levelConstructionEvent)
        {
            NewEvents.Add(levelConstructionEvent);
        }

        /// <summary>
        /// Returns true iff construction was success.
        /// </summary>
        public bool TryConstruct()
        {
            LevelConstructionEvents.Clear();
            LevelConstructionEvents.AddRange(NewEvents);
            NewEvents.Clear();
            try
            {
                LevelConstructionEvents.OrderBy(ev => -ev.Priority).ForEach(ev =>
                {
                    Debug.Log($"Starting: {ev.Name}");
                    if (!ev.Handle())
                    {
                        NewEvents.Add(ev);
                    }
                    Debug.Log($"Finished: {ev.Name}");
                });
                return true;
            }
            catch(Exception ex)
            {
                // Keep the old events
                NewEvents.Clear();
                NewEvents.AddRange(LevelConstructionEvents);
                Debug.Log(ex.Message);
                return false;
            }
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

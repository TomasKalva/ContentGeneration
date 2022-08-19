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
        /// <summary>
        /// Index of successfull LevelConstructor iteration. Indexed from 0.
        /// </summary>
        int Level { get; set; }

        public LevelConstructor()
        {
            LevelConstructionEvents = new List<LevelConstructionEvent>();
            NewEvents = new List<LevelConstructionEvent>();
            Level = 0;
        }

        public void AddEvent(LevelConstructionEvent levelConstructionEvent)
        {
            NewEvents.Add(levelConstructionEvent);
        }

        public void AddEvent(string name, int priority, LevelConstruction construction, bool persistent = false)
        {
            NewEvents.Add(new LevelConstructionEvent(name, priority, construction, persistent));
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
                    ev.Handle(Level);
                    if (ev.Persistent)
                    {
                        NewEvents.Add(ev);
                    }
                    Debug.Log($"Finished: {ev.Name}");
                });
                Level++;
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
    public delegate void LevelConstruction(int level);
    public class LevelConstructionEvent
    {
        public string Name { get; }
        public int Priority { get; }
        LevelConstruction Construction { get; }
        public bool Persistent { get; }

        public LevelConstructionEvent(string name, int priority, LevelConstruction construction, bool persistent = false)
        {
            Name = name;
            Priority = priority;
            Construction = construction;
            Persistent = persistent;
        }

        public void Handle(int level)
        {
            Construction(level);
        }
    }

    public delegate bool Condition();
}

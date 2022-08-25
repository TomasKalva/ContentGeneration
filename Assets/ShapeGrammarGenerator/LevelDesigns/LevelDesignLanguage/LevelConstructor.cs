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
        List<SimpleLevelConstructionEvent> LevelConstructionEvents { get; set; }
        /// <summary>
        /// Events that are set during the construction by modules.
        /// </summary>
        List<SimpleLevelConstructionEvent> NewEvents { get; set; }
        /// <summary>
        /// Index of successfull LevelConstructor iteration. Indexed from 0.
        /// </summary>
        int Level { get; set; }

        public LevelConstructor()
        {
            LevelConstructionEvents = new List<SimpleLevelConstructionEvent>();
            NewEvents = new List<SimpleLevelConstructionEvent>();
            Level = 0;
        }

        public void AddEvent(SimpleLevelConstructionEvent levelConstructionEvent)
        {
            NewEvents.Add(levelConstructionEvent);
        }

        public void AddEvent(string name, int priority, LevelConstruction construction, bool persistent = false)
        {
            NewEvents.Add(new SimpleLevelConstructionEvent(name, priority, construction, persistent));
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
    }

    /// <summary>
    /// Returns true if construction is finished.
    /// </summary>
    public delegate void LevelConstruction(int level);
    public abstract class LevelConstructionEvent
    {
        public string Name { get; }
        public int Priority { get; }
        public bool Persistent { get; }

        public LevelConstructionEvent(string name, int priority, bool persistent = false)
        {
            Name = name;
            Priority = priority;
            Persistent = persistent;
        }

        public abstract void Handle(int level);
    }
    
    public class SimpleLevelConstructionEvent : LevelConstructionEvent
    {
        LevelConstruction Construction { get; }

        public SimpleLevelConstructionEvent(string name, int priority, LevelConstruction construction, bool persistent = false) : base(name, priority, persistent)
        {
            Construction = construction;
        }


        public override void Handle(int level)
        {
            Construction(level);
        }
    }

    public class PoolLevelConstructionEvent : LevelConstructionEvent
    {
        List<LevelConstructionEvent> ConstructionsPool { get; set; }
        int MaxNumberOfEvents { get; }

        public PoolLevelConstructionEvent(string name, int priority, LevelConstruction construction, bool persistent = false) : base(name, priority, persistent)
        {
            ConstructionsPool = new List<LevelConstructionEvent>();
        }

        public override void Handle(int level)
        {
            ConstructionsPool
                .OrderBy(ev => ev.Priority)
                .Take(MaxNumberOfEvents)
                .ForEach(ev => ev.Handle(level));
        }
    }
}

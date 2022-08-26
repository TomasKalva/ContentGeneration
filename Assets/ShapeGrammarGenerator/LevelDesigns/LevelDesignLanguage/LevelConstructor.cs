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
        /*
        List<LevelConstructionEvent> LevelConstructionEvents { get; set; }
        /// <summary>
        /// Events that are set during the construction by modules.
        /// </summary>
        List<LevelConstructionEvent> NewEvents { get; set; }*/
        /// <summary>
        /// Index of successfull LevelConstructor iteration. Indexed from 0.
        /// </summary>
        int Level { get; set; }

        public PriorityPoolLevelConstructionEvent NecessaryEvents { get; set; }

        public LevelConstructor()
        {
            NecessaryEvents = new PriorityPoolLevelConstructionEvent();
            /*LevelConstructionEvents = new List<LevelConstructionEvent>();
            NewEvents = new List<LevelConstructionEvent>();*/
            Level = 0;
        }

        public void AddNecessaryEvent(LevelConstructionEvent levelConstructionEvent)
        {
            NecessaryEvents.AddEvent(levelConstructionEvent);
        }

        public void AddNecessaryEvent(string name, int priority, LevelConstruction construction, bool persistent = false)
        {
            NecessaryEvents.AddEvent(new LevelConstructionEvent(name, priority, construction, persistent));
        }

        /// <summary>
        /// Returns true iff construction was success.
        /// </summary>
        public bool TryConstruct()
        {
            var oldNecessaryPool = NecessaryEvents;
            NecessaryEvents = new PriorityPoolLevelConstructionEvent();
            bool necessaryOk = oldNecessaryPool.TryConstruct(ev => AddNecessaryEvent(ev), Level);
            if (necessaryOk)
            {
                Level++;
            }
            else
            {
                NecessaryEvents = oldNecessaryPool;
            }
            return necessaryOk;
            /*LevelConstructionEvents.Clear();
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
            }*/
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
        public bool Persistent { get; }
        LevelConstruction Construction { get; }

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
    
    public abstract class LevelConstructionEventPool
    {
        public abstract bool TryConstruct(Action<LevelConstructionEvent> reAddPersistent, int level);
    }

    public class PriorityPoolLevelConstructionEvent : LevelConstructionEventPool
    {
        /// <summary>
        /// Events that are used to construct the current level.
        /// </summary>
        List<LevelConstructionEvent> LevelConstructionEvents { get; }
        /// <summary>
        /// Events that are set during the construction by modules.
        /// </summary>
        //List<LevelConstructionEvent> NewEvents { get; }

        public PriorityPoolLevelConstructionEvent()
        {
            LevelConstructionEvents = new List<LevelConstructionEvent>();
            //NewEvents = new List<LevelConstructionEvent>();
        }

        public void AddEvent(LevelConstructionEvent levelConstructionEvent)
        {
            LevelConstructionEvents.Add(levelConstructionEvent);
        }

        public override bool TryConstruct(Action<LevelConstructionEvent> reAddPersistent, int level)
        {
            /*LevelConstructionEvents.Clear();
            LevelConstructionEvents.AddRange(NewEvents);
            NewEvents.Clear();*/
            try
            {
                LevelConstructionEvents.OrderBy(ev => -ev.Priority).ForEach(ev =>
                {
                    Debug.Log($"Starting: {ev.Name}");
                    ev.Handle(level);
                    if (ev.Persistent)
                    {
                        reAddPersistent(ev);
                        
                    }
                    Debug.Log($"Finished: {ev.Name}");
                });
                level++;
                return true;
            }
            catch (Exception ex)
            {
                // Keep the old events
                //NewEvents.Clear();
                //NewEvents.AddRange(LevelConstructionEvents);
                Debug.Log(ex.Message);
                return false;
            }
        }
    }
}

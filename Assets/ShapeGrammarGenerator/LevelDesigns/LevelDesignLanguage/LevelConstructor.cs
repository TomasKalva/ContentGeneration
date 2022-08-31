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
        /// Index of successfull LevelConstructor iteration. Indexed from 0.
        /// </summary>
        int Level { get; set; }

        public PriorityPoolLevelConstructionEvent NecessaryEvents { get; set; }
        public RoundRobinPoolLevelConstructionEvent PossibleEvents { get; set; }

        public LevelConstructor()
        {
            NecessaryEvents = new PriorityPoolLevelConstructionEvent();
            PossibleEvents = new RoundRobinPoolLevelConstructionEvent(3);
            /*LevelConstructionEvents = new List<LevelConstructionEvent>();
            NewEvents = new List<LevelConstructionEvent>();*/
            Level = 0;
        }

        public void AddNecessaryEvent(LevelConstructionEvent levelConstructionEvent)
        {
            NecessaryEvents.AddEvent(levelConstructionEvent);
        }

        public void AddNecessaryEvent(string name, int priority, LevelConstruction construction, bool persistent = false, Func<bool> condition = null)
        {
            AddNecessaryEvent(new LevelConstructionEvent(name, priority, construction, persistent, condition));
        }

        public void AddPossibleEvent(LevelConstructionEvent levelConstructionEvent)
        {
            PossibleEvents.AddEvent(levelConstructionEvent);
        }
        public void AddPossibleEvent(string name, int priority, LevelConstruction construction, bool persistent = false, Func<bool> condition = null)
        {
            AddPossibleEvent(new LevelConstructionEvent(name, priority, construction, persistent, condition));
        }

        /// <summary>
        /// Returns true iff construction was success.
        /// </summary>
        public bool TryConstruct()
        {
            var oldNecessaryPool = NecessaryEvents;
            var oldPossiblePool = PossibleEvents;
            NecessaryEvents = new PriorityPoolLevelConstructionEvent();
            PossibleEvents = new RoundRobinPoolLevelConstructionEvent(3);
            bool necessaryOk = oldNecessaryPool.TryConstruct(ev => AddNecessaryEvent(ev), Level);
            bool possibleOk = oldPossiblePool.TryConstruct(ev => AddPossibleEvent(ev), Level);
            if (necessaryOk && possibleOk)
            {
                Level++;
            }
            else
            {
                NecessaryEvents = oldNecessaryPool;
                PossibleEvents = oldPossiblePool;
            }
            return necessaryOk;
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
        public Func<bool> Condition { get; }
        LevelConstruction Construction { get; }

        public LevelConstructionEvent(string name, int priority, LevelConstruction construction, bool persistent = false, Func<bool> condition = null)
        {
            Name = name;
            Priority = priority;
            Construction = construction;
            Persistent = persistent;
            Condition = condition != null ? condition : () => true;
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

        public PriorityPoolLevelConstructionEvent()
        {
            LevelConstructionEvents = new List<LevelConstructionEvent>();
        }

        public void AddEvent(LevelConstructionEvent levelConstructionEvent)
        {
            LevelConstructionEvents.Add(levelConstructionEvent);
        }

        public override bool TryConstruct(Action<LevelConstructionEvent> reAddPersistent, int level)
        {
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
                return true;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }
        }
    }

    public class RoundRobinPoolLevelConstructionEvent : LevelConstructionEventPool
    {
        /// <summary>
        /// Events that are used to construct the current level.
        /// </summary>
        List<LevelConstructionEvent> LevelConstructionEvents { get; }
        /// <summary>
        /// Maximum number of events that can be executed during one TryConstruct.
        /// </summary>
        int MaxEvents { get; }

        public RoundRobinPoolLevelConstructionEvent(int maxEvents)
        {
            LevelConstructionEvents = new List<LevelConstructionEvent>();
            MaxEvents = maxEvents;
        }

        public void AddEvent(LevelConstructionEvent levelConstructionEvent)
        {
            LevelConstructionEvents.Add(levelConstructionEvent);
        }

        public override bool TryConstruct(Action<LevelConstructionEvent> reAddPersistent, int level)
        {
            try
            {
                var toExecute = LevelConstructionEvents.Where(ev => ev.Condition()).Take(MaxEvents);
                var toKeep = LevelConstructionEvents.Except(toExecute);

                toKeep.ForEach(ev => reAddPersistent(ev));

                toExecute.ForEach(ev =>
                {
                    Debug.Log($"Starting: {ev.Name}");
                    ev.Handle(level);
                    if (ev.Persistent)
                    {
                        reAddPersistent(ev);

                    }
                    Debug.Log($"Finished: {ev.Name}");
                });
                return true;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }
        }
    }
}

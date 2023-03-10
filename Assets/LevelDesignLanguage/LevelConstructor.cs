using OurFramework.Environment.GridMembers;
using OurFramework.Environment.ShapeGrammar;
using OurFramework.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OurFramework.LevelDesignLanguage
{
    /// <summary>
    /// Creates the level.
    /// </summary>
    public class LevelConstructor
    {
        /// <summary>
        /// Index of successfull LevelConstructor iteration. Indexed from 0.
        /// </summary>
        int Level { get; set; }

        public AllEventPool NecessaryEvents { get; set; }
        public RoundRobinEventPool PossibleEvents { get; set; }

        public LevelConstructor()
        {
            NecessaryEvents = new AllEventPool();
            PossibleEvents = new RoundRobinEventPool(3);
            Level = 0;
        }

        public void AddNecessaryEvent(LevelConstructionEvent levelConstructionEvent)
        {
            NecessaryEvents.AddEvent(levelConstructionEvent);
        }

        public void AddNecessaryEvent(string name, int priority, LevelConstruction construction, bool persistent = false, LevelConstructionCondition condition = null)
        {
            AddNecessaryEvent(new LevelConstructionEvent(name, priority, construction, persistent, condition));
        }

        public void AddPossibleEvent(LevelConstructionEvent levelConstructionEvent)
        {
            PossibleEvents.AddEvent(levelConstructionEvent);
        }
        public void AddPossibleEvent(string name, int priority, LevelConstruction construction, bool persistent = false, LevelConstructionCondition condition = null)
        {
            AddPossibleEvent(new LevelConstructionEvent(name, priority, construction, persistent, condition));
        }

        /// <summary>
        /// Returns true iff construction was success.
        /// </summary>
        public void Construct()
        {
            var oldNecessaryPool = NecessaryEvents;
            var oldPossiblePool = PossibleEvents;
            NecessaryEvents = new AllEventPool();
            PossibleEvents = new RoundRobinEventPool(3);
            try
            {
                var neccesaryConstructions = oldNecessaryPool.GetEventConstuctions(NecessaryEvents, Level);
                var possibleConstructions = oldPossiblePool.GetEventConstuctions(PossibleEvents, Level);
                neccesaryConstructions.Concat(possibleConstructions)
                    .OrderBy(ev => -ev.Priority)
                    .ForEach(ev => ev.Handle(Level));
                Level++;
            }
            catch(Exception ex) when (
                ex is GridException || 
                ex is ShapeGrammarException || 
                ex is LevelDesignException)
            {
                NecessaryEvents = oldNecessaryPool;
                PossibleEvents = oldPossiblePool;
                throw ex;
            }
        }
    }

    /// <summary>
    /// Constructs part of a level.
    /// </summary>
    public delegate void LevelConstruction(int level);
    /// <summary>
    /// Returns true if construction can be done.
    /// </summary>
    public delegate bool LevelConstructionCondition(int level);
    public class LevelConstructionEvent
    {
        string Name { get; }
        public int Priority { get; }
        bool Persistent { get; }
        public LevelConstructionCondition Condition { get; }
        LevelConstruction Construction { get; }
        public LevelConstructionEventPool ToAddTo { private get; set; }

        public LevelConstructionEvent(string name, int priority, LevelConstruction construction, bool persistent = false, LevelConstructionCondition condition = null)
        {
            Name = name;
            Priority = priority;
            Construction = construction;
            Persistent = persistent;
            Condition = condition ?? (_ => true);
        }

        public void Handle(int level)
        {
            Debug.Log($"Starting: {Name}");
            Construction(level);
            if (Persistent)
            {
                ToAddTo.AddEvent(this);
            }
            Debug.Log($"Finished: {Name}");
        }
    }
    
    /// <summary>
    /// Contains events that are executed on start of level.
    /// </summary>
    public abstract class LevelConstructionEventPool
    {
        public abstract void AddEvent(LevelConstructionEvent levelConstructionEvent);

        public abstract IEnumerable<LevelConstructionEvent> GetEventConstuctions(LevelConstructionEventPool newPool, int level);
    }

    /// <summary>
    /// All specified events are executed.
    /// </summary>
    public class AllEventPool : LevelConstructionEventPool
    {
        /// <summary>
        /// Events that are used to construct the current level.
        /// </summary>
        List<LevelConstructionEvent> LevelConstructionEvents { get; }

        public AllEventPool()
        {
            LevelConstructionEvents = new List<LevelConstructionEvent>();
        }

        public override void AddEvent(LevelConstructionEvent levelConstructionEvent)
        {
            LevelConstructionEvents.Add(levelConstructionEvent);
        }

        public override IEnumerable<LevelConstructionEvent> GetEventConstuctions(LevelConstructionEventPool newPool, int level)
        {
            LevelConstructionEvents.ForEach(ev => ev.ToAddTo = newPool);
            return LevelConstructionEvents.Where(ev => ev.Condition(level));
        }
    }

    /// <summary>
    /// Only limited number of events is taken from a priority queue. Persistent
    /// effects are reinserted at the end.
    /// </summary>
    public class RoundRobinEventPool : LevelConstructionEventPool
    {
        /// <summary>
        /// Events that are used to construct the current level.
        /// </summary>
        List<LevelConstructionEvent> LevelConstructionEvents { get; }
        /// <summary>
        /// Maximum number of events that can be executed during one TryConstruct.
        /// </summary>
        int MaxEvents { get; }

        public RoundRobinEventPool(int maxEvents)
        {
            LevelConstructionEvents = new List<LevelConstructionEvent>();
            MaxEvents = maxEvents;
        }

        public override void AddEvent(LevelConstructionEvent levelConstructionEvent)
        {
            LevelConstructionEvents.Add(levelConstructionEvent);
        }

        public override IEnumerable<LevelConstructionEvent> GetEventConstuctions(LevelConstructionEventPool newPool, int level)
        {
            LevelConstructionEvents.ForEach(ev => ev.ToAddTo = newPool);

            var toExecute = LevelConstructionEvents.Where(ev => ev.Condition(level)).Take(MaxEvents);
            var toKeep = LevelConstructionEvents.Except(toExecute);

            toKeep.ForEach(ev => newPool.AddEvent(ev));
            return toExecute;
        }
    }
}

﻿using Assets.ShapeGrammarGenerator;
using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage;
using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Modules;
using Assets.ShapeGrammarGenerator.Primitives;
using Assets.ShapeGrammarGenerator.ShapeGrammar;
using ContentGeneration.Assets.UI;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ShapeGrammar.AsynchronousEvaluator;

namespace ShapeGrammar
{

    abstract class LDLanguage
    {
        public LanguageState State { get; }

        public Libraries Lib { get; }
        public Grammars Gr { get; }
        public Languages L { get; }
        public Environments Env { get; }
        public ObjectPlacement<CharacterState> PlC { get; }
        public ObjectPlacement<InteractiveObjectState> PlO { get; }
        //public MsgPrinter Msg { get; }

        public LDLanguage(LanguageParams languageParams)
        {
            Lib = languageParams.Lib;
            Gr = languageParams.Gr;
            
            State = languageParams.LanguageState;

            Env = new Environments(this);
            PlC = new ObjectPlacement<CharacterState>((area, enemy) => area.AddEnemy(enemy));
            PlO = new ObjectPlacement<InteractiveObjectState>((area, io) => area.AddInteractiveObject(io));
            //Msg = new MsgPrinter();

            L = languageParams.Languages;// Languages.Get(languageParams);
        }

        /// <summary>
        /// Returns true iff constructing was success.
        /// </summary>
        /*bool TryConstruct()
        {
            result = false;
            State.Restart();
            yield return TaskSteps.One();

            var task = Task<bool>.Factory.StartNew(() => State.LC.TryConstruct());
            while (!task.IsCompleted)
            {
                yield return TaskSteps.One();
            }
            result = task.Result;
        }*/

        public IEnumerable<TaskSteps> GenerateWorld()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            int constructionTries = 1;


            while (true)
            {
                State.Restart();
                yield return TaskSteps.One();

                var constructingTask = Task<bool>.Factory.StartNew(() =>
                {
                    try
                    {
                        State.LC.Construct();
                        State.CalculateObjectsPositions();
                        return true;
                    }
                    catch(Exception ex) when (
                        ex is GridException ||
                        ex is ShapeGrammarException ||
                        ex is LevelDesignException)
                    {
                        Debug.Log($"Construction failed: {ex.Message}");
                        return false;
                    }
                });
                while (!constructingTask.IsCompleted)
                {
                    yield return TaskSteps.One();
                }
                if (constructingTask.Result)
                {
                    break;
                }

                yield return TaskSteps.One();
                if(constructionTries++ >= 5)
                {
                    break;
                }
            }
            Debug.Log($"Number of construction tries: {constructionTries}");

            yield return TaskSteps.One();

            State.GrammarState.WorldState.Added.CreateGeometry(State.World);
            yield return TaskSteps.One();

            yield return TaskSteps.Multiple(State.GrammarState.WorldState.CreateGeometry(State.World));

            yield return TaskSteps.One();


            yield return TaskSteps.Multiple(State.InstantiateAreas());


            stopwatch.Stop();
            Debug.Log(stopwatch.ElapsedMilliseconds);
        }
    }

    class LanguageParams
    {
        public LanguageState LanguageState { get; }

        public Libraries Lib { get; }
        public Grammars Gr { get; }
        public Languages Languages { get; }

        public LanguageParams(Libraries lib, Grammars gr, LanguageState languageState, Languages languages)
        {
            Lib = lib;
            Gr = gr;
            LanguageState = languageState;
            Languages = languages;
        }
    }

    #region Primitives


    class SpacePartitioning
    {
        HashSet<Area> activeAreas;
        TraversabilityGraph traversabilityGraph;
        bool initialized = false; // initialize during first update so that UI can get reference to this enemy

        public SpacePartitioning(TraversabilityGraph traversabilityGraph)
        {
            this.traversabilityGraph = traversabilityGraph;
        }

        public void Initialize()
        {
            traversabilityGraph.Areas.ForEach(area => area.Disable());
            activeAreas = new HashSet<Area>();
        }

        public void Update(Node activeNode)
        {
            if (!initialized)
            {
                Initialize();
                initialized = true;
            }
            
            if(activeNode == null)
            {
                // Player is outside of all areas
                return;
            }

            var activeArea = traversabilityGraph.TryGetArea(activeNode);
            if(activeArea == null)
            {
                // Player is in a non-traversable area
                return;
            }

            var currentAreas = traversabilityGraph.Neighbors(activeArea).Append(activeArea);

            // remove no longer active
            var notActive = activeAreas.Where(area => traversabilityGraph.Distance(area, activeArea, int.MaxValue) > 1).ToList();
            notActive.ForEach(area => area.Disable());
            notActive.ForEach(area => activeAreas.Remove(area));

            // add newly active
            var newActive = currentAreas.Except(activeAreas);
            newActive.ForEach(area => area.Enable());
            newActive.ForEach(area => activeAreas.Add(area));
        }
    }


    class Item
    {

    }

    public delegate T GeometryMaker<out T>() where T : MonoBehaviour;
    /*
    public class GeometryMaker<T> where T : MonoBehaviour
    {
        Func<T> GeometryF { get; }

        public GeometryMaker(Func<T> geometryF)
        {
            GeometryF = geometryF;
        }

        public T CreateGeometry()
        {
            var geometry = GeometryF();
            return geometry;
        }
    }*/

    #endregion

    class Languages
    {
        public LevelLanguage LevelLanguage { get; private set; }
        public PatternLanguage PatternLanguage { get; private set; }
        public TestingLanguage TestingLanguage { get; private set; }
        public FactionsLanguage FactionsLanguage { get; private set; }
        public AscendingLanguage AscendingLanguage { get; private set; }
        public OutOfDepthEncountersLanguage OutOfDepthEncountersLanguage { get; private set; }
        public DetailsLanguage DetailsLanguage { get; private set; }
        public EnvironmentLanguage EnvironmentLanguage { get; private set; }
        public NpcLanguage NpcLanguage { get; private set; }
        public DeathLanguage DeathLanguage { get; private set; }


        public Languages()
        {
        }

        /// <summary>
        /// This class is referenced by LanguageParams and also requires them for initialization so
        /// languages can't be initialized in constructor.
        /// </summary>
        public void Initialize(LanguageParams languageParams)
        {
            LevelLanguage = new LevelLanguage(languageParams);
            PatternLanguage = new PatternLanguage(languageParams);
            TestingLanguage = new TestingLanguage(languageParams);
            FactionsLanguage = new FactionsLanguage(languageParams);
            AscendingLanguage = new AscendingLanguage(languageParams);
            OutOfDepthEncountersLanguage = new OutOfDepthEncountersLanguage(languageParams);
            DetailsLanguage = new DetailsLanguage(languageParams);
            EnvironmentLanguage = new EnvironmentLanguage(languageParams);
            NpcLanguage = new NpcLanguage(languageParams);
            DeathLanguage = new DeathLanguage(languageParams);
        }
    }

    #region Language tools

    class Environments
    {
        LDLanguage L { get; }
        LanguageState LanguageState { get; }
        Symbols Sym { get; }

        public Environments(LDLanguage language)
        {
            L = language;
            LanguageState = L.State;
            Sym = L.Gr.Sym;
        }

        IEnumerable<Area> GenerateAndTakeTraversable(Grammar grammar)
        {
            var newNodes = grammar.Evaluate(LanguageState.GrammarState);
            var traversable = newNodes
                                .Where(node => node.HasSymbols(Sym.FullFloorMarker))
                                .Select(node => new Area(node, L))
                                .ToList();
            LanguageState.AddAreas(traversable);
            var connections = newNodes
                                .Where(node => 
                                        node.HasSymbols(Sym.ConnectionMarker) && 
                                        node.LE.CG().Cubes.Any() // Connections with no cubes are ignored
                                        )
                                .ToList();
            LanguageState.AddConnections(connections);
            return traversable;
        }

        public void Execute(Grammar grammar)
        {
            var traversable = GenerateAndTakeTraversable(grammar);
        }

        public void Line(ProductionList productions, NodesQuery startNodesQuery, int count, out LinearPath linearPath)
        {
            var linearGrammar = new CustomGrammar(productions, count, startNodesQuery, state => state.LastCreated);
            var traversable = GenerateAndTakeTraversable(linearGrammar);
            linearPath = new LinearPath(traversable.ToList());
        }

        public void One(ProductionList productions, NodesQuery startNodesQuery, out SingleArea one)
        {
            var grammar = new CustomGrammar(productions, 1, startNodesQuery);
            one = new SingleArea(GenerateAndTakeTraversable(grammar).First());
        }

        public void ExtendRandomly(ProductionList productions, NodesQuery startNodesQuery, int count, out Branching branching)
        {
            var grammar = new CustomGrammar(productions, count, startNodesQuery, NodesQueries.Extend(NodesQueries.LastCreated));
            var traversableNodes = GenerateAndTakeTraversable(grammar);
            branching = new Branching(traversableNodes.ToList());
        }

        public void BranchRandomly(ProductionList productions, int count, out Branching branching)
        {
            var grammar = new CustomGrammar(productions, count);
            var traversableNodes = GenerateAndTakeTraversable(grammar);
            branching = new Branching(traversableNodes.ToList());
        }

        /// <summary>
        /// Path length will be at least 1.
        /// </summary>
        /// <param name="targetedProductions">Productions that can be guided.</param>
        public void MoveFromTo(Func<PathGuide, ProductionList> targetedProductions, ProductionList connectBackProductions, int pathLength, IEnumerable<Node> from, IEnumerable<Node> to, out LinearPath path)
        {
            // define path guide
            var guideBack = new PointPathGuide(LanguageState.GrammarState,
                state =>
                {
                    // todo: fix null reference exception
                    var returnToNodes = to; 
                    var currentNodesCenter = state.LastCreated.Select(node => node.LE).ToLevelGroupElement(LanguageState.Ldk.grid).CG().Center();
                    var targetPoint = returnToNodes.SelectMany(n => n.LE.Cubes()).ArgMin(cube => (cube.Position - currentNodesCenter).AbsSum()).Position;
                    return Vector3Int.RoundToInt(targetPoint);
                });


            // define a grammar that moves to the target
            pathLength = Math.Max(1, pathLength);
            var targetedGardenGrammar =
                new GrammarSequence()
                    .AppendLinear(
                        targetedProductions(guideBack),
                        pathLength - 1, NodesQueries.LastCreated
                    )
                    .AppendStartEnd(
                        L.Gr.Sym,
                        connectBackProductions,
                        state => state.LastCreated,
                        state => to
                    );

            // execute the grammar and get nodes
            var traversableNodes = GenerateAndTakeTraversable(targetedGardenGrammar);
            path = new LinearPath(traversableNodes.ToList());
        }
    }

    public static class Msg
    {
        public static void Show(string message)
        {
            GameViewModel.ViewModel.Message = message;
        }
    }

    class Grammars
    {
        public ProductionLists PrL { get; }
        public Productions Pr { get; }
        public Symbols Sym { get; }

        public Grammars(LevelDevelopmentKit ldk)
        {
            Sym = new Symbols();
            Pr = new Productions(ldk, Sym);
            PrL = new ProductionLists(ldk, Pr);
        }
    }

    #endregion
}

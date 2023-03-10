using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using OurFramework.Environment.ShapeGrammar;
using OurFramework.Environment.ShapeCreation;
using OurFramework.Environment.GridMembers;
using OurFramework.Gameplay.Data;
using OurFramework.Util;
using static OurFramework.Game.AsynchronousEvaluator;

namespace OurFramework.LevelDesignLanguage
{
    /// <summary>
    /// Used for defining entire levels and runs.
    /// </summary>
    abstract class LDLanguage
    {
        const int MAX_NUMBER_OF_CONSTRUCTION_TRIES = 5;
        public LanguageState State { get; }

        public Libraries.Libraries Lib { get; }
        public Grammars Gr { get; }
        public Modules M { get; }
        public Environments Env { get; }
        public ObjectPlacement<CharacterState> PlC { get; }
        public ObjectPlacement<InteractiveObjectState> PlO { get; }

        public LDLanguage(LanguageParams languageParams)
        {
            Lib = languageParams.Lib;
            Gr = languageParams.Gr;
            
            State = languageParams.LanguageState;

            Env = new Environments(this);
            PlC = new ObjectPlacement<CharacterState>((area, enemy) => area.AddEnemy(enemy));
            PlO = new ObjectPlacement<InteractiveObjectState>((area, io) => area.AddInteractiveObject(io));
            //Msg = new MsgPrinter();

            M = languageParams.Modules;// Languages.Get(languageParams);
        }

        /// <summary>
        /// Creates a new level.
        /// </summary>
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
                if(constructionTries++ >= MAX_NUMBER_OF_CONSTRUCTION_TRIES)
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
            Debug.Log($"Level generating took {stopwatch.ElapsedMilliseconds} ms");
        }
    }

    /// <summary>
    /// Parameters used when initializing languages.
    /// </summary>
    class LanguageParams
    {
        public LanguageState LanguageState { get; }

        public Libraries.Libraries Lib { get; }
        public Grammars Gr { get; }
        public Modules Modules { get; }

        public LanguageParams(Libraries.Libraries lib, Grammars gr, LanguageState languageState, Modules modules)
        {
            Lib = lib;
            Gr = gr;
            LanguageState = languageState;
            Modules = modules;
        }
    }

    #region Primitives
    
    public delegate T GeometryMaker<out T>() where T : MonoBehaviour;

    #endregion


    #region Language tools

    /// <summary>
    /// Used for defining environments.
    /// </summary>
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

        /// <summary>
        /// Creates a path between from and to. The path is one way and is separated by a jump at the end.
        /// Jump goes into a newly created walled area so that the player can't jump out of the map or to another
        /// possibly blocked area.
        /// </summary>
        public void Loopback(Func<PathGuide, ProductionList> targetedProductions, ProductionList connectBackProductions, int pathLength, IEnumerable<Node> from, IEnumerable<Node> to, out LinearPath path, out SingleArea connectTo)
        {
            One(L.Gr.PrL.WalledAround(), _ => to, out connectTo);
            MoveFromTo(targetedProductions, connectBackProductions, pathLength, from, connectTo.Area.Node.ToEnumerable(), out path);
        }
    }

    /// <summary>
    /// Definitions for grammars.
    /// </summary>
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

using OurFramework.Environment.GridMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static OurFramework.Environment.GridMembers.LevelElement;

namespace OurFramework.Environment.ShapeGrammar
{
    public class ProductionList
    {
        public Production[] Get { get; }

        public ProductionList(params Production[] productions)
        {
            Get = productions;
        }
    }

    public abstract class Grammar
    {
        protected Action<ShapeGrammarState> StartHandler { get; set; }
        protected Action<ShapeGrammarState> EndHandler { get; set; }
        protected List<Node> CreatedNodes { get; }

        public Grammar()
        {
            StartHandler = EndHandler = _ => { } ;
            CreatedNodes = new List<Node>();
        }

        /// <summary>
        /// Returns all created nodes.
        /// </summary>
        public abstract IEnumerable<Node> Evaluate(ShapeGrammarState shapeGrammarState);

        protected IEnumerable<Node> Produce(ShapeGrammarState state, IEnumerable<Production> applicableProductions)
        {
            var newNodes = applicableProductions.DoUntilSuccess(prod => state.ApplyProduction(prod), x => x != null);
            if (newNodes == null)
            {
                throw new NoApplicableProductionException("Can't apply any productions");
            }
            CreatedNodes.AddRange(newNodes);
            return newNodes;
        }

    }

    public class RandomGrammar : Grammar
    {
        ProductionList Productions { get; }
        int Count { get; }

        public RandomGrammar(ProductionList productions, int count)
        {
            Productions = productions;
            Count = count;
        }

        public override IEnumerable<Node> Evaluate(ShapeGrammarState shapeGrammarState)
        {
            for (int i = 0; i < Count; i++)
            {
                shapeGrammarState.ActiveNodes = shapeGrammarState.Root.AllDerived();
                var applicable = Productions.Get.Shuffle();
                Produce(shapeGrammarState, applicable);
            }
            return CreatedNodes;
        }
    }

    public class AllGrammar : Grammar
    {
        ProductionList Productions { get; }

        public AllGrammar(ProductionList productions)
        {
            Productions = productions;
        }

        public override IEnumerable<Node> Evaluate(ShapeGrammarState shapeGrammarState)
        {
            IEnumerable<Node> newNodes = null;
            do
            {
                shapeGrammarState.ActiveNodes = shapeGrammarState.Root.AllDerived();
                var applicable = Productions.Get.Shuffle();
                // Grammar evaluates until no production can be applied
                try
                {
                    newNodes = Produce(shapeGrammarState, applicable);
                }catch(NoApplicableProductionException ex) 
                {
                    Debug.Log("All grammar finished.");
                    break;
                }
            }
            while (true);
            return CreatedNodes;
        }
    }

    public delegate IEnumerable<Node> NodesQuery(ShapeGrammarState state);
    public static class NodesQueries
    {
        public static NodesQuery LastCreated { get; } = state => state.LastCreated;
        public static NodesQuery All { get; } = state => state.Root.AllDerived();
        public static NodesQuery Extend(NodesQuery start)
        {
            var toExtend = new List<Node>();
            return state =>
            {
                // add start to the nodes to be extended during the first step
                if (!toExtend.Any())
                {
                    toExtend.AddRange(start(state));
                }
                else
                {
                    toExtend.AddRange(LastCreated(state));
                }

                return toExtend;
            };
        }
    }

    public class CustomGrammar : Grammar
    {
        ProductionList Productions { get; }
        int Count { get; }
        NodesQuery StartNodesQuery { get; }
        NodesQuery NodesQuery { get; }

        public CustomGrammar(ProductionList productions, int count, NodesQuery startNodesQuery = null, NodesQuery nodesQuery = null)
        {
            Productions = productions;
            Count = count;
            StartNodesQuery = startNodesQuery ?? (state => state.Root.AllDerived());
            NodesQuery = nodesQuery ?? (state => state.Root.AllDerived());
        }

        public override IEnumerable<Node> Evaluate(ShapeGrammarState shapeGrammarState)
        {
            for (int i = 0; i < Count; i++)
            {
                shapeGrammarState.ActiveNodes = i == 0 ? StartNodesQuery(shapeGrammarState) : NodesQuery(shapeGrammarState);
                var applicable = Productions.Get.Shuffle();
                var newNodes = Produce(shapeGrammarState, applicable);
            }
            return CreatedNodes;
        }
    }

    /// <summary>
    /// Evaluates all productions with extra start and end symbols in the queried nodes.
    /// </summary>
    public class StartEndGrammar : Grammar
    {
        ProductionList Productions { get; }
        NodesQuery StartQuery { get; }
        NodesQuery EndQuery { get; }
        Symbols Sym { get; }

        public StartEndGrammar(Symbols symbols, ProductionList productions, NodesQuery startQuery, NodesQuery endQuery)
        {
            Sym = symbols;
            Productions = productions;
            StartQuery = startQuery;
            EndQuery = endQuery;
        }

        public override IEnumerable<Node> Evaluate(ShapeGrammarState shapeGrammarState)
        {
            shapeGrammarState.ActiveNodes = shapeGrammarState.Root.AllDerived();

            var startNodes = StartQuery(shapeGrammarState);
            var endNodes = EndQuery(shapeGrammarState);

            //startNodes = endNodes = shapeGrammarState.Root.AllDerived();

            startNodes.ForEach(node => node.AddSymbol(Sym.StartMarker));
            endNodes.ForEach(node => node.AddSymbol(Sym.EndMarker));

            var applicable = Productions.Get.Shuffle();
            var newNodes = Produce(shapeGrammarState, applicable);
            
            startNodes.ForEach(node => node.RemoveSymbolByName(Sym.StartMarker));
            endNodes.ForEach(node => node.RemoveSymbolByName(Sym.EndMarker));

            return CreatedNodes;
        }
    }

    public class GrammarSequence : Grammar
    {

        List<Grammar> EvaluatorSequence { get; }

        public GrammarSequence()
        {
            EvaluatorSequence = new List<Grammar>();
        }

        public GrammarSequence Add(Grammar evaluator)
        {
            EvaluatorSequence.Add(evaluator);
            return this;
        }
        public GrammarSequence AppendLinear(ProductionList productions, int count, NodesQuery startNodesQuery)
            => Add(new CustomGrammar(productions, count, startNodesQuery, state => state.LastCreated));
        public GrammarSequence AppendStartEnd(Symbols symbols, ProductionList productions, NodesQuery startQuery, NodesQuery endQuery)
            => Add(new StartEndGrammar(symbols, productions, startQuery, endQuery));
        public override IEnumerable<Node> Evaluate(ShapeGrammarState shapeGrammarState)
        {
            StartHandler(shapeGrammarState);
            var createdNodes = EvaluatorSequence.SelectMany(evaluator => evaluator.Evaluate(shapeGrammarState)).ToList();
            EndHandler(shapeGrammarState);
            return createdNodes;
        }

        public GrammarSequence SetStartHandler(Action<ShapeGrammarState> handler)
        {
            StartHandler = handler;
            return this;
        }

        public GrammarSequence SetEndHandler(Action<ShapeGrammarState> handler)
        {
            EndHandler = handler;
            return this;
        }
    }

    public abstract class PathGuide
    {
        public abstract LEMoves SelectMove(LEMoves moves);
        public abstract IEnumerable<Vector3Int> SelectDirections(LevelElement currentElement);
    }

    public class RandomPathGuide : PathGuide
    {
        public override LEMoves SelectMove(LEMoves moves)
        {
            return moves;
        }

        public override IEnumerable<Vector3Int> SelectDirections(LevelElement currentElement)
        {
            return ExtensionMethods.HorizontalDirections().Shuffle();
        }
    }

    public class PointPathGuide : PathGuide
    {
        ShapeGrammarState ShapeGrammarState { get; }
        Func<ShapeGrammarState, Vector3Int> TargetF { get; }

        public PointPathGuide(ShapeGrammarState shapeGrammarState, Func<ShapeGrammarState, Vector3Int> targetF)
        {
            ShapeGrammarState = shapeGrammarState;
            TargetF = targetF;
        }

        public override LEMoves SelectMove(LEMoves moves)
        {
            if (!moves.Ms.Any())
                return moves;

            var targetPoint = TargetF(ShapeGrammarState);
            var leCenter = Vector3Int.FloorToInt(moves.LE.CG().Center());
            var bestMove = moves.Ms.ArgMin(m => ((leCenter + m) - targetPoint).AbsSum());
            return new LEMoves(moves.LE, bestMove.ToEnumerable());
        }

        public override IEnumerable<Vector3Int> SelectDirections(LevelElement currentElement)
        {
            var target = TargetF(ShapeGrammarState);
            var currentCenter = currentElement.CG().Center();
            
            return ExtensionMethods.HorizontalDirections().OrderBy(dir => ((currentCenter + dir) - target).sqrMagnitude);
        }
    }
}

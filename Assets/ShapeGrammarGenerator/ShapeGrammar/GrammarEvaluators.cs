using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ShapeGrammar.LevelElement;

namespace ShapeGrammar
{
    public abstract class GrammarEvaluator
    {
        Action<ShapeGrammarState> StartHandler { get; set; }
        Action<ShapeGrammarState> EndHandler { get; set; }
        public abstract void Evaluate(ShapeGrammarState shapeGrammarState);

        protected IEnumerable<Node> Produce(ShapeGrammarState state, IEnumerable<Production> applicableProductions, string errorMsg = null)
        {
            var newNodes = applicableProductions.DoUntilSuccess(prod => state.ApplyProduction(prod), x => x != null);
            if (newNodes == null)
            {
                UnityEngine.Debug.Log(errorMsg ?? $"Can't apply any productions");
                return null;
            }
            return newNodes;
        }

        public GrammarEvaluator SetStartHandler(Action<ShapeGrammarState> handler)
        {
            StartHandler = handler;
            return this;
        }

        public GrammarEvaluator SetEndHandler(Action<ShapeGrammarState> handler)
        {
            EndHandler = handler;
            return this;
        }
    }

    public class RandomGrammarEvaluator : GrammarEvaluator
    {
        List<Production> Productions { get; }
        int Count { get; }

        public RandomGrammarEvaluator(List<Production> productions, int count)
        {
            Productions = productions;
            Count = count;
        }

        public override void Evaluate(ShapeGrammarState shapeGrammarState)
        {
            for (int i = 0; i < Count; i++)
            {
                shapeGrammarState.ActiveNodes = shapeGrammarState.Root.AllDerived();
                var applicable = Productions.Shuffle();
                Produce(shapeGrammarState, applicable);
            }
        }
    }

    public delegate IEnumerable<Node> NodesQuery(ShapeGrammarState state);

    public class LinearGrammarEvaluator : GrammarEvaluator
    {
        List<Production> Productions { get; }
        int Count { get; }
        Symbol StartSymbol { get; }
        NodesQuery NodesQuery { get; }

        public LinearGrammarEvaluator(List<Production> productions, int count, Symbol startSymbol, NodesQuery nodesQuery = null)
        {
            Productions = productions;
            Count = count;
            StartSymbol = startSymbol;
            NodesQuery = nodesQuery ?? (state => state.Root.AllDerived());
        }

        public override void Evaluate(ShapeGrammarState shapeGrammarState)
        {
            for (int i = 0; i < Count; i++)
            {
                shapeGrammarState.ActiveNodes = NodesQuery(shapeGrammarState);
                var applicable = Productions.Shuffle();
                var newNodes = Produce(shapeGrammarState, applicable);
            }
        }
    }

    public class StartEndGrammarEvaluator : GrammarEvaluator
    {
        List<Production> Productions { get; }
        NodesQuery StartQuery { get; }
        NodesQuery EndQuery { get; }
        Symbols Sym { get; }

        public StartEndGrammarEvaluator(Symbols symbols, List<Production> productions, NodesQuery startQuery, NodesQuery endQuery)
        {
            Sym = symbols;
            Productions = productions;
            StartQuery = startQuery;
            EndQuery = endQuery;
        }

        public override void Evaluate(ShapeGrammarState shapeGrammarState)
        {
            shapeGrammarState.ActiveNodes = shapeGrammarState.Root.AllDerived();

            var startNodes = StartQuery(shapeGrammarState);
            var endNodes = EndQuery(shapeGrammarState);

            startNodes.ForEach(node => node.AddSymbol(Sym.StartMarker));
            endNodes.ForEach(node => node.AddSymbol(Sym.EndMarker));

            var applicable = Productions.Shuffle();
            var newNodes = Produce(shapeGrammarState, applicable);
            
            startNodes.ForEach(node => node.RemoveSymbolByName(Sym.StartMarker));
            endNodes.ForEach(node => node.RemoveSymbolByName(Sym.EndMarker));
        }
    }

    public class GrammarEvaluatorSequence : GrammarEvaluator
    {

        IEnumerable<GrammarEvaluator> EvaluatorSequence { get; }

        private GrammarEvaluatorSequence(IEnumerable<GrammarEvaluator> evaluatorSequence)
        {
            EvaluatorSequence = evaluatorSequence;
        }

        public GrammarEvaluatorSequence()
        {
            EvaluatorSequence = new GrammarEvaluator[0] { };
        }

        public GrammarEvaluatorSequence Append(GrammarEvaluator evaluator) => new GrammarEvaluatorSequence(EvaluatorSequence.Append(evaluator));
        public GrammarEvaluatorSequence AppendLinear(List<Production> productions, int count, Symbol startSymbol, NodesQuery nodesQuery = null)
            => Append(new LinearGrammarEvaluator(productions, count, startSymbol, nodesQuery));
        public GrammarEvaluatorSequence AppendStartEnd(Symbols symbols, List<Production> productions, NodesQuery startQuery, NodesQuery endQuery)
            => Append(new StartEndGrammarEvaluator(symbols, productions, startQuery, endQuery));
        public override void Evaluate(ShapeGrammarState shapeGrammarState)
        {
            EvaluatorSequence.ForEach(evaluator => evaluator.Evaluate(shapeGrammarState));
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

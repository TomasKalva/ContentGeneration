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
                shapeGrammarState.ActiveNodes = shapeGrammarState.Root.AllNodes();
                var applicable = Productions.Shuffle();
                Produce(shapeGrammarState, applicable);
            }
        }
    }

    public delegate IEnumerable<Node> NodesQuery(ShapeGrammarState state);

    public class BranchGrammarEvaluator : GrammarEvaluator
    {
        List<Production> Productions { get; }
        int Count { get; }
        Symbol StartSymbol { get; }
        NodesQuery NodesQuery { get; }

        public BranchGrammarEvaluator(List<Production> productions, int count, Symbol startSymbol, NodesQuery nodesQuery = null)
        {
            Productions = productions;
            Count = count;
            StartSymbol = startSymbol;
            NodesQuery = nodesQuery ?? (state => state.Root.AllNodes());
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
        public GrammarEvaluatorSequence AppendBranch(List<Production> productions, int count, Symbol startSymbol, NodesQuery nodesQuery = null)
            => Append(new BranchGrammarEvaluator(productions, count, startSymbol, nodesQuery));
        public override void Evaluate(ShapeGrammarState shapeGrammarState)
        {
            EvaluatorSequence.ForEach(evaluator => evaluator.Evaluate(shapeGrammarState));
        }
    }

    public abstract class PathGuide
    {
        public abstract LEMoves SelectMove(ShapeGrammarState state, LEMoves moves);
    }

    public class RandomPathGuide : PathGuide
    {
        public override LEMoves SelectMove(ShapeGrammarState state, LEMoves moves)
        {
            return moves;
        }
    }

    public class PointPathGuide : PathGuide
    {
        Func<ShapeGrammarState, Vector3Int> TargetF { get; }

        public PointPathGuide(Func<ShapeGrammarState, Vector3Int> targetF)
        {
            TargetF = targetF;
        }

        public override LEMoves SelectMove(ShapeGrammarState state, LEMoves moves)
        {
            if (!moves.Ms.Any())
                return moves;

            var targetPoint = TargetF(state);
            var leCenter = Vector3Int.FloorToInt(moves.LE.CG().Center());
            var bestMove = moves.Ms.ArgMin(m => ((leCenter + m) - targetPoint).AbsSum());
            return new LEMoves(moves.LE, bestMove.ToEnumerable());
        }
    }
}

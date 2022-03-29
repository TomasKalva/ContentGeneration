using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

    public class PathGuide
    {
        Func<ShapeGrammarState, Vector3Int> TargetF;
        

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeGrammar
{
    public class ProdParamsManager
    {
        public delegate bool Condition(ShapeGrammarState shapeGrammarState, ProdParams pp);

        List<Symbol[]> ParametersSymbols { get; }
        public Condition CanBeApplied { get; private set; }
        public HashSet<ProdParams> Failed { get; }

        public ProdParamsManager()
        {
            ParametersSymbols = new List<Symbol[]>();
            Failed = new HashSet<ProdParams>(new ProdParamsEqualityComparer());
            CanBeApplied = (_1, _2) => true;
        }

        public IEnumerable<ProdParams> GetParams(ShapeGrammarState state)
        {
            var parameterNodes = ParametersSymbols.Select(symbol => state.WithActiveSymbols(symbol));
            var parameterNodesSequences = parameterNodes.CartesianProduct();
            var prodPars = parameterNodesSequences.Select(parSeq => new ProdParams(parSeq.ToArray()))
                .Where(prodPar => !Failed.Contains(prodPar))
                .Where(prodPar => CanBeApplied(state, prodPar));
            return prodPars;
        }

        public ProdParamsManager SetCondition(Condition condition)
        {
            CanBeApplied = condition;
            return this;
        }

        public ProdParamsManager AddNodeSymbols(params Symbol[] nodeSymbols)
        {
            ParametersSymbols.Add(nodeSymbols);
            return this;
        }
    }

    class ProdParamsEqualityComparer : IEqualityComparer<ProdParams>
    {
        public bool Equals(ProdParams x, ProdParams y)
        {
            return Enumerable.SequenceEqual(x.Parameters, y.Parameters);
        }

        public int GetHashCode(ProdParams obj)
        {
            return obj.Parameters.GetHashCode();
        }
    }

    public class ProdParams
    {
        public Node[] Parameters { get; }

        public ProdParams(Node[] parameters)
        {
            this.Parameters = parameters;
        }

        public Node Param => Parameters.First();
        public static void Deconstruct(out Node par1) { par1 = null; }
    }
}

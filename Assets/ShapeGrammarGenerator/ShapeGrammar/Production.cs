using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
            var parameterNodes = ParametersSymbols.Select(symbols => state.ActiveWithSymbols(symbols));
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
        public void Deconstruct(out Node par1, out Node par2) 
        {
            if (Parameters.Length < 2)
                throw new InvalidOperationException($"Not enough parameters, expected 2, actual {Parameters.Length}");

            par1 = Parameters[0];
            par2 = Parameters[1];
        }
    }

    public class Production
    {
        public delegate ProductionProgram Effect(ShapeGrammarState shapeGrammarState, ProdParams prodParams);

        public string Name { get; }
        Effect ExpandNewNodes { get; }

        public ProdParamsManager ProdParamsManager { get; }

        public Production(string name, ProdParamsManager ppm, Effect effect)
        {
            Name = name;
            ProdParamsManager = ppm;
            ExpandNewNodes = effect;
        }

        /// <summary>
        /// Tries to apply this production and returns nodes it creates. If can't be applied, returns null instead.
        /// </summary>
        public IEnumerable<Operation> TryApply(ShapeGrammarState shapeGrammarState, out int triedParameters)
        {
            var parameters = ProdParamsManager.GetParams(shapeGrammarState).Shuffle();
            triedParameters = 0;
            foreach (var pp in parameters)
            {
                triedParameters++;
                var programState = ExpandNewNodes(shapeGrammarState, pp);
                if (programState == null || programState.Failed)
                {
                    ProdParamsManager.Failed.Add(pp);
                }
                else
                {
                    return programState.AppliedOperations;
                }
            }
            return null;
        }
    }
}

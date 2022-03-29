using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public static class ShapeGrammarExtensions
    {
        public static Node GrammarNode(this LevelElement le, params Symbol[] symbols)
        {
            return new Node(le, symbols.ToList());
        }
    }

    public class Node : Printable
    {
        List<Symbol> Symbols { get; }
        HashSet<string> SymbolNames { get; }

        public LevelElement LE { get; set; }

        public List<Node> Derived { get; set; }

        public Node(LevelElement levelElement, List<Symbol> symbols)
        {
            Symbols = symbols;
            SymbolNames = symbols.Select(sym => sym.Name).ToHashSet();
            LE = levelElement;
            Derived = new List<Node>();
        }

        public IEnumerable<Node> AllNodes()
        {
            return Derived.Any() ?
                Derived.SelectMany(node => node.AllNodes()).Prepend(this).Distinct() :
                new[] { this };
        }

        public bool HasSymbols(params Symbol[] symbols)
        {
            return symbols.All(symbol => SymbolNames.Contains(symbol.Name));
        }

        public PrintingState Print(PrintingState state)
        {
            state.Print("{");
            Symbols.ForEach(symbol => symbol.Print(state));
            state.Print("}");
            return state;
        }

        public SymbolT GetSymbol<SymbolT>() where SymbolT : Symbol
        {
            return Symbols.SelectNN(symbol => symbol as SymbolT).FirstOrDefault();
        }
    }

    public class Production
    {
        public delegate IEnumerable<Operation> Effect(ShapeGrammarState shapeGrammarState, ProdParams prodParams);

        public string Name { get; }
        Effect ExpandNewNodes { get; }

        public ProdParamsManager ProdParamsManager { get; }

        public Production(string name, ProdParamsManager ppm, Effect effect)
        {
            Name = name;
            ProdParamsManager = ppm;
            ExpandNewNodes = effect;
        }

        public IEnumerable<Operation> TryApply(ShapeGrammarState shapeGrammarState, out int triedParameters)
        {
            var parameters = ProdParamsManager.GetParams(shapeGrammarState).Shuffle();
            triedParameters = 0;
            foreach(var pp in parameters)
            {
                triedParameters++;
                var ops = ExpandNewNodes(shapeGrammarState, pp);
                if(ops == null)
                {
                    ProdParamsManager.Failed.Add(pp);
                }
                else
                {
                    return ops;
                }
            }
            return null;
        }
    }

    public abstract class Operation : Printable
    {
        public IEnumerable<Node> From { get; set; }
        public IEnumerable<Node> To { get; set; }

        protected void AddIntoDag()
        {
            foreach(var from in From)
            {
                from.Derived.AddRange(To);
            }
        }

        public Operation SetFrom(params Node[] from)
        {
            From = from;
            return this;
        }

        public Operation SetTo(params Node[] to)
        {
            To = to;
            foreach(var node in to)
            {
                UnityEngine.Debug.Assert(node.LE != null, $"Level element of created node is null!");
            }
            return this;
        }

        public abstract IEnumerable<Node> ChangeState(ShapeGrammarState grammarState);

        protected void AddToFoundation(ShapeGrammarState grammarState, LevelElement le)
        {
            le.Cubes().ForEach(cube => grammarState.OffersFoundation[new Vector3Int(cube.Position.x, 0, cube.Position.z)] = false);
            grammarState.VerticallyTaken = grammarState.VerticallyTaken.Merge(le.ProjectToY(0));
        }

        public abstract PrintingState Print(PrintingState state);

        public PrintingState PrintNodes(PrintingState state)
        {
            state.Print("\tFrom: ");
            From.ForEach(from => from.Print(state));
            state.Print("\tTo: ");
            To.ForEach(to => to.Print(state));
            return state;
        }
    }

    public class AddNew : Operation
    {
        public override IEnumerable<Node> ChangeState(ShapeGrammarState grammarState)
        {
            AddIntoDag();
            var lge = To.Select(node => node.LE).ToLevelGroupElement(grammarState.WorldState.Grid);
            grammarState.WorldState = grammarState.WorldState.TryPush(lge);
            AddToFoundation(grammarState, lge);
            return To;
        }

        public override PrintingState Print(PrintingState state)
        {
            state.PrintIndent("Add");
            PrintNodes(state);
            return state;
        }
    }

    public class Replace : Operation
    {
        public override IEnumerable<Node> ChangeState(ShapeGrammarState grammarState)
        {
            AddIntoDag();
            From.ForEach(node => node.LE = LevelElement.Empty(grammarState.WorldState.Grid));
            var lge = To.Select(node => node.LE).ToLevelGroupElement(grammarState.WorldState.Grid);
            grammarState.WorldState = grammarState.WorldState.ChangeAll(To.Select<Node, WorldState.ChangeWorld>(gn => ws => ws.TryPush(gn.LE)));
            AddToFoundation(grammarState, lge);
            return To;
        }

        public override PrintingState Print(PrintingState state)
        {
            state.PrintLine("Replace");
            PrintNodes(state);
            return state;
        }
    }

    public class ShapeGrammarState : Printable
    {
        public Node Root { get; }

        /// <summary>
        /// For operations that require querying already created world.
        /// </summary>
        public WorldState WorldState { get; set; }

        public Grid<bool> OffersFoundation { get; }
        public LevelElement VerticallyTaken { get; set; }
        public IEnumerable<Node> ActiveNodes { get; set; }

        public class GrammarStats
        {
            public class ProductionInstance : Printable
            {
                public string Name { get; }
                public long TimeMs { get; }
                public List<Operation> Operations { get; }
                public int TriedParameters { get; }
                public bool Applied => Operations != null;

                public ProductionInstance(string name, List<Operation> operations, long timeMs, int triedParameters)
                {
                    Name = name;
                    TimeMs = timeMs;
                    Operations = operations;
                    TriedParameters = triedParameters;
                }

                public PrintingState Print(PrintingState state)
                {
                    state.PrintLine($"{(Applied  ? "Success:" : "Fail:\t")}\t{TimeMs}ms\t\t{TriedParameters} pars\t{Name}");
                    return state;
                }
            }


            public List<ProductionInstance> ProductionInstances { get; }
            public IEnumerable<ProductionInstance> AppliedProductions() => ProductionInstances.Where(production => production.Applied);

            public GrammarStats()
            {
                ProductionInstances = new List<ProductionInstance>();
                ProductionInstances = new List<ProductionInstance>();
            }

            public void AddApplied(string name, IEnumerable<Operation> operations, long timeMs, int triedParameters)
            {
                ProductionInstances.Add(new ProductionInstance(name, operations.ToList(), timeMs, triedParameters));
            }

            public void AddFailed(string name, long timeMs, int triedParameters)
            {
                ProductionInstances.Add(new ProductionInstance(name, null, timeMs, triedParameters));
            }

            public void Print()
            {
                var printingState = new PrintingState();
                ProductionInstances.ForEach(p => p.Print(printingState));
                printingState.Show();
            }
        }

        public GrammarStats Stats { get; }

        public ShapeGrammarState(LevelDevelopmentKit ldk)
        {
            var grid = ldk.grid;
            var empty = LevelElement.Empty(grid);
            Root = new Node(empty, new List<Symbol>());
            WorldState = new WorldState(empty, grid, le => le.ApplyGrammarStyleRules(ldk.houseStyleRules)).TryPush(empty);
            OffersFoundation = new Grid<bool>(new Vector3Int(10, 1, 10), (_1, _2) => true);
            VerticallyTaken = LevelElement.Empty(grid);
            Stats = new GrammarStats();
            ActiveNodes = Root.AllNodes();
        }

        public IEnumerable<Node> ApplyProduction(Production production)
        {
            var sw = new Stopwatch();
            sw.Start();
            var operations = production.TryApply(this, out int triedParameters);
            sw.Stop();
            var elapsedMs = sw.ElapsedMilliseconds;

            if (operations == null)
            {
                Stats.AddFailed(production.Name, elapsedMs, triedParameters);
                return null;
            }
            else
            {
                var newNodes = operations.SelectMany(operation => operation.ChangeState(this)).Evaluate();
                Stats.AddApplied(production.Name, operations.ToList(), elapsedMs, triedParameters);
                return newNodes;
            }
        }

        public IEnumerable<Node> ActiveWithSymbols(params Symbol[] symbols)
        {
            return ActiveNodes.Where(node => node.LE.Cubes().Any() && node.HasSymbols(symbols));
        }

        public bool CanBeFounded(LevelElement le)
        {
            return le.Cubes().All(cube => OffersFoundation[new Vector3Int(cube.Position.x, 0, cube.Position.z)]);
        }

        #region Operation factories
        public Operation Add(params Node[] from)
        {
            return new AddNew()
            {
                From = from,
            };
        }

        public Operation Replace(params Node[] from)
        {
            return new Replace()
            {
                From = from,
            };
        }

        #endregion

        public PrintingState Print(PrintingState state)
        {
            Stats.AppliedProductions().ForEach(appliedPr =>
            {
                state.PrintLine(appliedPr.Name).ChangeIndent(1);
                appliedPr.Operations.ForEach(op => op.Print(state).PrintLine());
                state.ChangeIndent(-1);
            });
            return state;
        }
    }

    public abstract class GrammarEvaluator
    {
        protected List<Production> Productions { get; }

        protected GrammarEvaluator(List<Production> productions)
        {
            Productions = productions;
        }

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

    public class ShapeGrammar : GrammarEvaluator
    {
        int Count { get; }

        public ShapeGrammar(List<Production> productions, int count) : base(productions)
        {
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
        int Count { get; }
        Symbol StartSymbol { get; }
        Production StartProduction { get; }
        Production EndProduction { get; }
        NodesQuery EndNodesQuery { get; }

        public BranchGrammarEvaluator(Production startProduction, List<Production> middleProductions, Production endProduction, NodesQuery endNodesQuery, int count, Symbol startSymbol) : base(middleProductions)
        {
            Count = count;
            StartSymbol = startSymbol;
            StartProduction = startProduction;
            EndProduction = endProduction;
            EndNodesQuery = endNodesQuery;
        }

        public override void Evaluate(ShapeGrammarState shapeGrammarState)
        {
            var createdByThis = new List<Node>();
            var endActive = EndNodesQuery(shapeGrammarState);

            var startNodes = shapeGrammarState.ApplyProduction(StartProduction);
            if (startNodes == null)
            {
                UnityEngine.Debug.Log($"Can't apply start production {StartProduction.Name}");
                return;
            }
            startNodes.ForEach(stNode => createdByThis.Add(stNode));

            for (int i = 0; i < Count; i++)
            {
                shapeGrammarState.ActiveNodes = createdByThis;
                var applicable = Productions.Shuffle();
                var newNodes = Produce(shapeGrammarState, applicable);
                createdByThis = newNodes.ToList();
                //newNodes.ForEach(newNode => createdByThis.Push(newNode));
            }


            shapeGrammarState.ActiveNodes = endActive;
            var endNodes = shapeGrammarState.ApplyProduction(EndProduction);
            if (endNodes == null)
            {
                UnityEngine.Debug.Log($"Can't apply start production {EndProduction.Name}");
                return;
            }
        }
    }
}

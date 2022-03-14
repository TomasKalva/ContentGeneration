using System;
using System.Collections.Generic;
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

        public LevelElement LevelElement { get; set; }

        public List<Node> Derived { get; set; }

        public Node(LevelElement levelElement, List<Symbol> symbols)
        {
            Symbols = symbols;
            SymbolNames = symbols.Select(sym => sym.Name).ToHashSet();
            LevelElement = levelElement;
            Derived = new List<Node>();
        }

        public IEnumerable<Node> AllNodes()
        {
            return Derived.Any() ?
                Derived.SelectMany(node => node.AllNodes()).Prepend(this) :
                new[] { this };
        }

        public bool HasActiveSymbols(params Symbol[] symbols)
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

        public IEnumerable<Operation> TryApply(ShapeGrammarState shapeGrammarState)
        {
            var parameters = ProdParamsManager.GetParams(shapeGrammarState).Shuffle();
            foreach(var pp in parameters)
            {
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
            return this;
        }

        public abstract void ChangeState(ShapeGrammarState grammarState);

        protected void AddToFoundation(ShapeGrammarState grammarState, LevelElement le)
        {
            le.Cubes().ForEach(cube => grammarState.OffersFoundation[new Vector3Int(cube.Position.x, 0, cube.Position.z)] = false);
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
        public override void ChangeState(ShapeGrammarState grammarState)
        {
            AddIntoDag();
            var lge = To.Select(node => node.LevelElement).ToLevelGroupElement(grammarState.WorldState.Grid);
            grammarState.WorldState = grammarState.WorldState.TryPush(lge);
            AddToFoundation(grammarState, lge);
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
        public override void ChangeState(ShapeGrammarState grammarState)
        {
            AddIntoDag();
            From.ForEach(node => node.LevelElement = LevelElement.Empty(grammarState.WorldState.Grid));
            var lge = To.Select(node => node.LevelElement).ToLevelGroupElement(grammarState.WorldState.Grid);
            grammarState.WorldState = grammarState.WorldState.ChangeAll(To.Select<Node, WorldState.ChangeWorld>(gn => ws => ws.TryPush(gn.LevelElement)));
            AddToFoundation(grammarState, lge);
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

        struct AppliedProduction
        {
            public string Name { get; }
            public List<Operation> Operations { get; }

            public AppliedProduction(string name, List<Operation> operations)
            {
                Name = name;
                Operations = operations;
            }
        }

        List<AppliedProduction> Applied { get; }

        public ShapeGrammarState(LevelDevelopmentKit ldk)
        {
            var grid = ldk.grid;
            var empty = LevelElement.Empty(grid); new LevelGeometryElement(grid, AreaType.None, new CubeGroup(grid, new List<Cube>()));
            Root = new Node(empty, new List<Symbol>());
            WorldState = new WorldState(empty, grid, le => le.ApplyGrammarStyleRules(ldk.houseStyleRules)).TryPush(empty);
            OffersFoundation = new Grid<bool>(new Vector3Int(10, 1, 10), (_1, _2) => true);
            Applied = new List<AppliedProduction>();
        }

        public bool ApplyProduction(Production production)
        {
            var operations = production.TryApply(this);
            if (operations == null)
                return false;

            operations.ForEach(operation => operation.ChangeState(this));
            Applied.Add(new AppliedProduction(production.Name, operations.ToList()));
            return true;
        }

        public IEnumerable<Node> WithActiveSymbols(params Symbol[] symbols)
        {
            return Root.AllNodes().Where(node => node.LevelElement.Cubes().Any() && node.HasActiveSymbols(symbols));
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
            Applied.ForEach(appliedPr =>
            {
                state.PrintLine(appliedPr.Name).ChangeIndent(1);
                appliedPr.Operations.ForEach(op => op.Print(state).PrintLine());
                state.ChangeIndent(-1);
            });
            return state;
        }
    }

    public class ShapeGrammar
    {
        public List<Production> Productions { get; }

        public ShapeGrammarState ShapeGrammarState { get; }

        public ShapeGrammar(List<Production> productions, LevelDevelopmentKit ldk)
        {
            Productions = productions;
            ShapeGrammarState = new ShapeGrammarState(ldk);
        }

        public void DoProductions(int count)
        {
            for(int i = 0; i < count; i++)
            {
                var applicable = Productions.Shuffle();//.Where(production => production.CanBeApplied(ShapeGrammarState)).Shuffle();
                var applied = applicable.DoUntilSuccess(prod => ShapeGrammarState.ApplyProduction(prod), x => x);
                if (!applied)
                {
                    Debug.Log($"Can't apply any productions {i}");
                }
            }
        }
    }
}

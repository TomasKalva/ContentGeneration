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
            return new Node(le, symbols.ToHashSet());
        }
    }

    public class Node
    {
        public HashSet<Symbol> Symbols { get; }

        public LevelElement LevelElement { get; set; }

        public List<Node> Derived { get; set; }

        public Node(LevelElement levelElement, HashSet<Symbol> symbols)
        {
            Symbols = symbols;
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
            return symbols.All(symbol => Symbols.Contains(symbol));
        }
    }

    public class Production
    {
        public delegate bool Condition(ShapeGrammarState shapeGrammarState);
        public delegate IEnumerable<Operation> Effect(ShapeGrammarState shapeGrammarState);

        public Condition CanBeApplied { get; }
        public Effect ExpandNewNodes { get; }

        public Production(Condition canBeApplied, Effect effect)
        {
            CanBeApplied = canBeApplied;
            ExpandNewNodes = effect;
        }
    }

    public class Productions
    {
        public LevelDevelopmentKit ldk { get; }
        public Symbols sym { get; } = new Symbols();

        public Productions(LevelDevelopmentKit ldk)
        {
            this.ldk = ldk;
        }

        public Production CreateNewHouse()
        {
            return new Production(
                state => true,
                state =>
                {
                    var root = state.Root;
                    var room = ldk.sgShapes.Room(new Box2Int(0, 0, 5, 5).InflateY(5, 10));
                    var movedRoom = ldk.pl.MoveToNotOverlap(state.WorldState.Added, room);
                    var foundation = ldk.sgShapes.Foundation(movedRoom);
                    return new[]
                    {
                        state.Add(root).SetTo(movedRoom.GrammarNode(sym.Room)),
                        state.Add(root).SetTo(foundation.GrammarNode(sym.Foundation))
                    };
                });
        }

        public Production ExtrudeTerrace()
        {
            return new Production(
                state => state.WithActiveSymbols(sym.Room) != null,
                state =>
                {
                    var room = state.WithActiveSymbols(sym.Room);
                    var terraces =
                    // for give parametrization
                        ExtensionMethods.HorizontalDirections().Shuffle()
                        .Select(dir => room.LevelElement.CubeGroup()
                    
                    // create a new object
                        .ExtrudeDir(dir, 2).LevelElement(AreaType.Colonnade))
                    
                    // fail if no such object exists
                        .Where(le => le.CubeGroup().NotTaken());
                    if (!terraces.Any())
                        return null;

                    // and modify the dag
                    var terrace = terraces.FirstOrDefault();
                    return new[]
                    {
                        state.Add(room).SetTo(terrace.GrammarNode(sym.Terrace)),
                    };
                });
        }
    }

    public abstract class Operation 
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
    }

    public class AddNew : Operation
    {
        public override void ChangeState(ShapeGrammarState grammarState)
        {
            AddIntoDag();
            var lge = To.Select(node => node.LevelElement).ToLevelGroupElement(grammarState.WorldState.Grid);
            grammarState.WorldState = grammarState.WorldState.TryPush(lge);
        }
    }

    public class Replace : Operation
    {
        public override void ChangeState(ShapeGrammarState grammarState)
        {
            AddIntoDag();
            From.ForEach(node => node.LevelElement = LevelElement.Empty(grammarState.WorldState.Grid));
            var lge = To.Select(node => node.LevelElement).ToLevelGroupElement(grammarState.WorldState.Grid);
            grammarState.WorldState = grammarState.WorldState.TryPush(lge);
        }
    }

    public enum Mode
    {
        Sequence, 
        Test
    }

    public class ShapeGrammarState
    {
        public Node Root { get; }

        /// <summary>
        /// For operations that require querying already created world.
        /// </summary>
        public WorldState WorldState { get; set; }


        public Mode Mode { get; set; }

        List<Operation> Applied { get; }

        public ShapeGrammarState(LevelDevelopmentKit ldk)
        {
            var grid = new Grid(new UnityEngine.Vector3Int(20, 20, 20));
            var empty = LevelElement.Empty(grid); new LevelGeometryElement(grid, AreaType.None, new CubeGroup(grid, new List<Cube>()));
            Root = new Node(empty, new HashSet<Symbol>());
            WorldState = new WorldState(empty, grid, le => le.ApplyGrammarStyleRules(ldk.houseStyleRules)).TryPush(empty);
            Applied = new List<Operation>();
        }

        public bool ApplyProduction(Production production)
        {
            var operations = production.ExpandNewNodes(this);
            if (operations == null)
                return false;

            operations.ForEach(operation => operation.ChangeState(this));
            Applied.AddRange(operations);
            return true;
        }

        public Node WithActiveSymbols(params Symbol[] symbols)
        {
            var nodes = Root.AllNodes().Where(node => node.HasActiveSymbols(symbols));
            return nodes.GetRandom();
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
            return new AddNew()
            {
                From = from,
            };
        }
        #endregion
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
                var applicable = Productions.Where(production => production.CanBeApplied(ShapeGrammarState));
                ShapeGrammarState.ApplyProduction(applicable.GetRandom());
            }
        }
    }
}

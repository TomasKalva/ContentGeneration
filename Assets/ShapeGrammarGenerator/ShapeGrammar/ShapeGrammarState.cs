using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }

    public abstract class Symbol { }

    public class BrokenFloor : Symbol
    {
    }

    public class ConnectTo : Symbol
    {
        public Node To { get; }
    }

    public class NotTaken : Symbol
    {
    }

    public class ExtrudeUp : Symbol
    {
    }

    public class CreateFrom : Symbol
    {
        public List<Node> From { get; }
    }

    public class FloorGiver : Symbol
    {
        public Node GiveTo { get; } 
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

        public Production CreateNewHouse()
        {
            return new Production(
                (state) => true,
                (state) =>
                {
                    var root = state.Root;
                    var room = ldk.sgShapes.Room(new Box2Int(0, 0, 5, 5).InflateY(5, 5));
                    var movedRoom = ldk.pl.MoveToNotOverlap(state.WorldState.Added, room);
                    var foundation = ldk.sgShapes.Foundation(movedRoom);
                    return new[] 
                    { 
                        state.Add(root).SetTo(room.GrammarNode()),
                        state.Add(root).SetTo(foundation.GrammarNode())
                    };
                });
        }

        /*
        public Production ExtrudeTerrace()
        {
            return new Production(
                (state) => true,
                (state) =>
                {
                    var house = state.
                    var room = ldk.sgShapes.Room(new Box2Int(0, 0, 5, 5).InflateY(5, 5));
                    var movedRoom = ldk.pl.MoveToNotOverlap(state.WorldState.Added, room);
                    var foundation = ldk.sgShapes.Foundation(movedRoom);
                    return new[] { room.GrammarNode(), foundation.GrammarNode() };
                });
        }
        */
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
            var lge = From.Select(node => node.LevelElement).ToLevelGroupElement(grammarState.WorldState.Grid);
            grammarState.WorldState = grammarState.WorldState.TryPush(lge);
        }
    }

    public class Replace : Operation
    {
        public override void ChangeState(ShapeGrammarState grammarState)
        {
            AddIntoDag();
            From.ForEach(node => node.LevelElement = LevelElement.Empty(grammarState.WorldState.Grid));
            var lge = From.Select(node => node.LevelElement).ToLevelGroupElement(grammarState.WorldState.Grid);
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

        public ShapeGrammarState()
        {
            var grid = new Grid(new UnityEngine.Vector3Int(20, 20, 20));
            var empty = new LevelGeometryElement(grid, AreaType.None, new CubeGroup(grid, new List<Cube>()));
            Root = new Node(empty, new HashSet<Symbol>());
            var worldState = new WorldState(empty, grid, le => le);
            Applied = new List<Operation>();
        }

        public void ApplyProduction(Production production)
        {
            var operations = production.ExpandNewNodes(this);
            operations.ForEach(operation => operation.ChangeState(this));
            Applied.AddRange(operations);
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

        public ShapeGrammar(List<Production> productions)
        {
            Productions = productions;
            ShapeGrammarState = new ShapeGrammarState();
        }

        public void DoProductions(int count)
        {
            for(int i = 0; i<count; i++)
            {
                var applicable = Productions.Where(production => production.CanBeApplied(ShapeGrammarState));
                ShapeGrammarState.ApplyProduction(applicable.GetRandom());
            }
        }
    }
}

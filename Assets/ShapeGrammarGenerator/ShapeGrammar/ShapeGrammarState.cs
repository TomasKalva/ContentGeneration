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
                    if (room.LevelElement.CubeGroup().LengthY() <= 1)
                        return null;

                    var terraces =
                    // for give parametrization
                        ExtensionMethods.HorizontalDirections().Shuffle()
                        .Select(dir =>

                        // access the object
                        room.LevelElement.CubeGroup()

                        // create a new object
                        .ExtrudeDir(dir, 2).LevelElement(AreaType.Colonnade))

                        // fail if no such object exists
                        .Where(le => le.CubeGroup().NotTaken());
                    if (!terraces.Any())
                        return null;

                    // and modify the dag
                    var terraceSpace = terraces.FirstOrDefault();
                    var lge = terraceSpace.Split(Vector3Int.down, AreaType.None, 1);
                    var terrace = lge.LevelElements[1].SetAreaType(AreaType.Colonnade).GrammarNode(sym.Terrace);
                    var roof = lge.LevelElements[0].SetAreaType(AreaType.Roof).GrammarNode(sym.Roof);
                    return new[]
                    {
                        state.Add(room).SetTo(terrace),
                        state.Add(terrace).SetTo(roof),
                    };
                });
        }

        public Production ExtrudeCourtyard()
        {
            return new Production(
                state => state.WithActiveSymbols(sym.Room) != null,
                state =>
                {
                    var room = state.WithActiveSymbols(sym.Room);
                    var roomCubeGroup = room.LevelElement.CubeGroup();

                    var courtyards =
                    // for give parametrization
                        ExtensionMethods.HorizontalDirections().Shuffle()
                        .Select(dir =>

                        // access the object
                        roomCubeGroup

                        // create a new object
                        .CubeGroupMaxLayer(dir)
                        .OpAdd()
                        .ExtrudeHor().ExtrudeHor().Minus(roomCubeGroup)

                        .LevelElement(AreaType.Yard))

                        // fail if no such object exists
                        .Where(le => le.CubeGroup().NotTaken() && state.CanBeFounded(le));
                    if (!courtyards.Any())
                        return null;

                    // and modify the dag
                    var courtyardSpace = courtyards.FirstOrDefault().GrammarNode(sym.Courtyard);
                    return new[]
                    {
                        state.Add(room).SetTo(courtyardSpace),
                    };
                });
        }

        /*
        public Production ExtrudeRoof()
        {
            return new Production(
                state => state.WithActiveSymbols(sym.Terrace) != null,
                state =>
                {
                    var terrace = state.WithActiveSymbols(sym.Terrace);
                    if (terrace.Derived.Where(derNode => derNode.HasActiveSymbols(sym.Roof)).Any())
                        return null;

                    var roof = terrace.LevelElement.CubeGroup().ExtrudeDir(Vector3Int.up).LevelElement(AreaType.Roof);
                    return new[]
                    {
                        state.Add(terrace).SetTo(roof.GrammarNode(sym.Roof)),
                    };
                }
                );
        }*/

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
            var grid = ldk.grid;
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

        public bool CanBeFounded(LevelElement le)
        {
            return true;
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

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

    public class ProdParamsManager
    {
        List<Symbol[]> ParametersSymbols { get; }
        HashSet<ProdParams> Failed { get; }
        
        public ProdParamsManager()
        {
            ParametersSymbols = new List<Symbol[]>();
        }

        public IEnumerable<ProdParams> GetParams(ShapeGrammarState state)
        {
            //todo: filter failed
            var parameterNodes = ParametersSymbols.Select(symbol => state.WithActiveSymbols(symbol));
            var parameterNodesSequences = parameterNodes.CartesianProduct();
            return parameterNodesSequences.Select(parSeq => new ProdParams(parSeq.ToArray()));
        }

        public ProdParamsManager AddNodeSymbols(params Symbol[] nodeSymbols)
        {
            ParametersSymbols.Add(nodeSymbols);
            return this;
        }
    }

    public class ProdParams
    {
        Node[] parameters;

        public ProdParams(Node[] parameters)
        {
            this.parameters = parameters;
        }

        public Node Param => parameters.First();
        public static void Deconstruct(out Node par1) { par1 = null; }
    }

    public class Node : Printable
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

        public PrintingState Print(PrintingState state)
        {
            state.Print("{");
            Symbols.ForEach(symbol => symbol.Print(state));
            state.Print("}");
            return state;
        }
    }

    public class Production
    {
        public delegate bool Condition(ShapeGrammarState shapeGrammarState);
        public delegate IEnumerable<Operation> Effect(ShapeGrammarState shapeGrammarState, ProdParams prodParams);

        public string Name { get; }
        public Condition CanBeApplied { get; }
        Effect ExpandNewNodes { get; }

        public ProdParamsManager ProdParamsManager { get; }

        public Production(string name, ProdParamsManager ppm, Condition canBeApplied, Effect effect)
        {
            Name = name;
            ProdParamsManager = ppm;
            CanBeApplied = canBeApplied;
            ExpandNewNodes = effect;
        }

        public IEnumerable<Operation> Apply(ShapeGrammarState shapeGrammarState)
        {
            var parameters = ProdParamsManager.GetParams(shapeGrammarState).Shuffle();
            return parameters.DoUntilSuccess(pp => ExpandNewNodes(shapeGrammarState, pp), result => result != null);
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
                "CreateNewHouse",
                new ProdParamsManager(),
                state => true,
                (state, pp) =>
                {
                    var root = state.Root;
                    var room = ldk.sgShapes.Room(new Box2Int(0, 0, 5, 5).InflateY(8, 10));
                    var movedRoom = ldk.pl.MoveToNotOverlap(state.WorldState.Added, room).GrammarNode(sym.Room);
                    var foundation = ldk.sgShapes.Foundation(movedRoom.LevelElement).GrammarNode(sym.Foundation);
                    return new[]
                    {
                        state.Add(root).SetTo(movedRoom),
                        state.Add(movedRoom).SetTo(foundation)
                    };
                });
        }

        public Production ExtrudeTerrace()
        {
            return new Production(
                "ExtrudeTerrace",
                new ProdParamsManager().AddNodeSymbols(sym.Room),
                state => state.WithActiveSymbols(sym.Room) != null,
                (state, pp) =>
                {
                    var room = pp.Param;
                    //var room = state.WithActiveSymbols(sym.Room);
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

        public Production CourtyardFromRoom()
        {
            return new Production(
                "CourtyardFromRoom",
                new ProdParamsManager().AddNodeSymbols(sym.Room),
                state => state.WithActiveSymbols(sym.Room) != null,
                (state, pp) =>
                {
                    var room = pp.Param;
                    //var room = state.WithActiveSymbols(sym.Room);
                    var roomCubeGroup = room.LevelElement.CubeGroup();

                    var courtyards =
                    // for give parametrization
                        ExtensionMethods.HorizontalDirections().Shuffle()
                        .Select(dir =>

                        // create a new object
                        roomCubeGroup
                        .CubeGroupMaxLayer(dir)
                        .OpAdd()
                        .ExtrudeHor().ExtrudeHor().Minus(roomCubeGroup)

                        .LevelElement(AreaType.Yard))

                        // fail if no such object exists
                        .Where(le => le.CubeGroup().NotTaken() && state.CanBeFounded(le));
                    if (!courtyards.Any())
                        return null;

                    var courtyard = courtyards.FirstOrDefault().GrammarNode(sym.Courtyard);

                    courtyard.LevelElement.ApplyGrammarStyleRules(ldk.houseStyleRules);
                    var door = ldk.con.ConnectByDoor(room.LevelElement, courtyard.LevelElement).GrammarNode();

                    // and modify the dag
                    var foundation = ldk.sgShapes.Foundation(courtyard.LevelElement).GrammarNode(sym.Foundation);
                    return new[]
                    {
                        state.Add(room).SetTo(courtyard),
                        state.Add(courtyard).SetTo(foundation),
                        state.Add(room, courtyard).SetTo(door),
                    };
                });
        }

        public Production CourtyardFromCourtyardCorner()
        {
            return new Production(
                "CourtyardFromCourtyardCorner",
                new ProdParamsManager().AddNodeSymbols(sym.Courtyard),
                state => state.WithActiveSymbols(sym.Courtyard) != null,
                (state, pp) =>
                {
                    var courtyard = pp.Param;
                    //var courtyard = state.WithActiveSymbols(sym.Courtyard);
                    var courtyardGroup = courtyard.LevelElement.CubeGroup();
                    var corners = courtyardGroup.AllSpecialCorners().CubeGroup().Where(cube => cube.Position.y >= 4);
                    if (!corners.Cubes.Any())
                        return null;

                    var newCourtyards = corners.CubeGroupMaxLayer(Vector3Int.down).Cubes
                        .Select(startCube =>
                        startCube.Group()
                        .OpAdd()
                            .ExtrudeHor().Minus(courtyardGroup)
                            .ExtrudeHor().Minus(courtyardGroup)
                            .ExtrudeVer(Vector3Int.up, 2)
                        .OpNew()
                            .MoveBy(2 * Vector3Int.down)
                        .LevelElement(AreaType.Yard))
                        .Where(le => le.CubeGroup().NotTaken() && state.CanBeFounded(le)); ;
                    if (!newCourtyards.Any())
                        return null;

                    var newCourtyardLe = newCourtyards.FirstOrDefault();
                    // floor doesn't exist yet...
                    newCourtyardLe.ApplyGrammarStyleRules(ldk.houseStyleRules);
                    
                    // connecting by elevator always succeeds
                    var path = ldk.con.ConnectByElevator(courtyard.LevelElement, newCourtyardLe);

                    // and modify the dag
                    var newCourtyardNode = newCourtyardLe.GrammarNode(sym.Courtyard);
                    var foundation = ldk.sgShapes.Foundation(newCourtyardNode.LevelElement).GrammarNode(sym.Foundation);
                    var pathNode = path.GrammarNode();
                    return new[]
                    {
                        state.Add(courtyard).SetTo(newCourtyardNode),
                        state.Add(newCourtyardNode).SetTo(foundation),
                        state.Add(courtyard, newCourtyardNode).SetTo(pathNode),
                    };
                });
        }

        public Production BridgeFromCourtyard()
        {
            return new Production(
                "BridgeFromCourtyard",
                new ProdParamsManager().AddNodeSymbols(sym.Courtyard),
                state => state.WithActiveSymbols(sym.Courtyard) != null,
                (state, pp) =>
                {
                    var courtyard = pp.Param;
                    //var courtyard = state.WithActiveSymbols(sym.Courtyard);
                    var courtyardCubeGroup = courtyard.LevelElement.CubeGroup();

                    var bridges =
                    // for give parametrization
                        ExtensionMethods.HorizontalDirections().Shuffle()
                        .Select(dir =>

                        // create a new object
                        courtyardCubeGroup
                        .ExtrudeDir(dir, 3)

                        .LevelElement(AreaType.Bridge))

                        // fail if no such object exists
                        .Where(le => le.CubeGroup().NotTaken() && state.CanBeFounded(le));
                    if (!bridges.Any())
                        return null;

                    var bridge = bridges.FirstOrDefault().GrammarNode(sym.Bridge);

                    bridge.LevelElement.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    // and modify the dag
                    var foundation = ldk.sgShapes.Foundation(bridge.LevelElement).GrammarNode(sym.Foundation);
                    return new[]
                    {
                        state.Add(courtyard).SetTo(bridge),
                        state.Add(bridge).SetTo(foundation),
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
            grammarState.WorldState = grammarState.WorldState.TryPush(lge);
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
            Root = new Node(empty, new HashSet<Symbol>());
            WorldState = new WorldState(empty, grid, le => le.ApplyGrammarStyleRules(ldk.houseStyleRules)).TryPush(empty);
            OffersFoundation = new Grid<bool>(new Vector3Int(10, 1, 10), (_1, _2) => true);
            Applied = new List<AppliedProduction>();
        }

        public bool ApplyProduction(Production production)
        {
            var operations = production.Apply(this);
            if (operations == null)
                return false;

            operations.ForEach(operation => operation.ChangeState(this));
            Applied.Add(new AppliedProduction(production.Name, operations.ToList()));
            return true;
        }

        public IEnumerable<Node> WithActiveSymbols(params Symbol[] symbols)
        {
            return Root.AllNodes().Where(node => node.HasActiveSymbols(symbols));
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
            return new AddNew()
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
                var applicable = Productions.Where(production => production.CanBeApplied(ShapeGrammarState)).Shuffle();
                var applied = applicable.DoUntilSuccess(prod => ShapeGrammarState.ApplyProduction(prod), x => x);
                if (!applied)
                {
                    Debug.Log($"Can't apply any productions {i}");
                }
            }
        }
    }
}

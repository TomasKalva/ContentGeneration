using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class ProductionProgram
    {
        public static LevelDevelopmentKit ldk { get; set; }
        public static Productions pr { get; set; }
        public static StyleRules StyleRules { get; set; }


        ShapeGrammarState State { get; }
        public bool Failed { get; private set; }

        /// <summary>
        /// Not IEnumerable to prevent multiple evaluations.
        /// </summary>
        List<Node> CurrentNodes { get; set; }
        public List<Operation> AppliedOperations { get; set; }

        public ProductionProgram(ShapeGrammarState state)
        {
            AppliedOperations = new List<Operation>();
            this.State = state;
        }

        ProductionProgram SetFailed(bool value)
        {
            Failed = value;
            return this;
        }

        public ProductionProgram SelectOne(ProductionProgram program, out Node result)
        {
            result = null;
            if (Failed)
                return this;

            if(!program.CurrentNodes.Any())
                return SetFailed(true);

            var node = program.CurrentNodes.GetRandom();
            CurrentNodes = node.ToEnumerable().ToList();
            result = node;
            return this;
        }

        public ProductionProgram FindPath(Func<Node> pathFinder, out Node path)
        {
            path = null;
            if (Failed) 
                return this;

            ApplyStyles();
            path = pathFinder();
            Debug.Assert(path != null);
            CurrentNodes = path.ToEnumerable().ToList();

            return this;
        }

        /// <summary>
        /// Used, because path finding requires information about floor...
        /// Maybe just assume that entire bottom layer is filled with floor to get rid of this call.
        /// </summary>
        ProductionProgram ApplyStyles()
        {
            if (Failed)
                return this;

            foreach(var node in AppliedOperations.SelectMany(op => op.To))
            {
                node.LE.ApplyGrammarStyleRules(StyleRules);
            }
            return this;
        }

        public ProductionProgram PlaceNodes(params Node[] from)
        {
            if (Failed)
                return this;

            if (!CurrentNodes.Any())
                return SetFailed(true);

            var op = State.Add(from).SetTo(CurrentNodes.ToArray());
            AppliedOperations.Add(op);
            return this;
        }

        public ProductionProgram ReplaceNodes(params Node[] from)
        {
            if (Failed)
                return this;

            if (!CurrentNodes.Any())
                return SetFailed(true);

            var op = State.Replace(from).SetTo(CurrentNodes.ToArray());
            AppliedOperations.Add(op);
            return this;
        }

        public ProductionProgram Found() => Found(out var _);

        /// <summary>
        /// Returned node is not in derivation. It is just a container of newly created level elements.
        /// </summary>
        public ProductionProgram Found(out Node foundation)
        {
            foundation = null;
            if (Failed)
                return this;

            CurrentNodes = CurrentNodes.Select(node => ldk.sgShapes.Foundation(node.LE).GN(pr.sym.Foundation)).ToList();
            foundation = CurrentNodes.Select(node => node.LE).ToLevelGroupElement(ldk.grid).GN();
            return this;
        }

        public ProductionProgram ReserveUpward(int height) => ReserveUpward(height, out var _);

        /// <summary>
        /// Returned node is not in derivation. It is just a container of newly created level elements.
        /// </summary>
        public ProductionProgram ReserveUpward(int height, out Node reservation)
        {
            reservation = null;
            if (Failed)
                return this;

            var reservations = CurrentNodes
                .Select(node =>
                    State.NewProgram()
                        .Set(() => node.LE.CG().ExtrudeVer(Vector3Int.up, height).LE(AreaType.Reservation).GN(pr.sym.UpwardReservation(node)))
                        .NotTaken()
                );
            if (reservations.Any(prog => prog.Failed))
                return SetFailed(true);

            CurrentNodes = reservations
                .Where(prog => !prog.Failed)
                .SelectMany(prog => prog.CurrentNodes).ToList();

            reservation = CurrentNodes.Select(node => node.LE).ToLevelGroupElement(ldk.grid).GN();
            return this;
            
        }

        public ProductionProgram Directional(IEnumerable<Vector3Int> directions, Func<Vector3Int, Node> nodeCreator)
        {
            if (Failed)
                return this;

            CurrentNodes = directions.Select(dir => nodeCreator(dir)).ToList();
            return this;
        }

        public ProductionProgram NotTaken()
        {
            if (Failed)
                return this;

            CurrentNodes = CurrentNodes.Where(node => node.LE.CG().AllAreNotTaken()).ToList();
            return this;
        }

        public ProductionProgram Where(Func<Node, bool> condition)
        {
            if (Failed)
                return this;

            CurrentNodes = CurrentNodes.Where(condition).ToList();
            return this;
        }

        public ProductionProgram CanBeFounded() => Where(node => State.CanBeFounded(node.LE));

        public ProductionProgram NonEmpty() => Where(node => node.LE.Cubes().Any());

        public ProductionProgram DontIntersectAdded() => Where(node => !node.LE.CG().Intersects(State.WorldState.Added.CG()));

        public ProductionProgram Change(Func<Node, Node> changer)
        {
            if (Failed)
                return this;

            CurrentNodes = CurrentNodes.SelectNN(changer).ToList();
            return this;
        }

        public ProductionProgram CurrentFirst(out Node first)
        {
            first = null;
            if (Failed)
                return this;

            first = CurrentNodes.FirstOrDefault();
            if (first == null)
                return SetFailed(true);

            return this;
        }

        public ProductionProgram Condition(Func<bool> condF)
        {
            if (Failed)
                return this;

            if (!condF())
                return SetFailed(true);

            return this;
        }

        public ProductionProgram ApplyOperationsIf(bool condition, Func<ProductionProgram> programF)
        {
            if (Failed)
                return this;

            if (condition)
            {
                var program = programF();
                AppliedOperations.AddRange(program.AppliedOperations);
                Failed = program.Failed;
            }
            return this;
        }

        public ProductionProgram EmptyPath()
        {
            return Set(() => new CubeGroup(ldk.grid, new List<Cube>()).LE().GN(pr.sym.ConnectionMarker));
        }

        public ProductionProgram Set(Func<Node> nodesF, out Node result)
        {
            result = null;
            if (Failed)
                return this;

            Set(() => nodesF().ToEnumerable());
            result = CurrentNodes.First();
            return this;
        }

        public ProductionProgram Set(Func<Node> nodesF)
        {
            if (Failed)
                return this;

            return Set(() => nodesF().ToEnumerable());
        }

        public ProductionProgram Set(Func<IEnumerable<Node>> nodesF)
        {
            if (Failed)
                return this;

            CurrentNodes = nodesF().ToList();
            return this;
        }

        public ProductionProgram MoveNearTo(Node nearWhat)
        {
            Change(node => ldk.pl.MoveNearXZ(
                                    nearWhat.LE.MoveBottomTo(0),
                                    node.LE.MoveBottomTo(0),
                                    State.VerticallyTaken)?.GN())
            .Change(validNewRoom => validNewRoom.LE.MoveBottomTo(nearWhat.LE.CG().LeftBottomBack().y).GN());
            return this;
        }
    }
}

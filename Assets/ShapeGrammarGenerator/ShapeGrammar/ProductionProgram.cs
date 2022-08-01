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
        /// The grammar currently operates on these nodes.
        /// 
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

        /// <summary>
        /// Keeps only one of the current nodes. Fails if no current nodes exist.
        /// </summary>
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

            path = pathFinder();
            Debug.Assert(path != null);
            CurrentNodes = path.ToEnumerable().ToList();

            return this;
        }

        /// <summary>
        /// Places the current nodes to the level. Each will be child of all nodes contained in from. 
        /// </summary>
        public ProductionProgram PlaceCurrentFrom(params Node[] from)
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
                    State.NewProgram(prog => prog
                        .Set(() => node.LE.CG().ExtrudeVer(Vector3Int.up, height).LE(AreaType.Reservation).GN(pr.sym.UpwardReservation(node)))
                        .NotTaken()
                    )
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

        /// <summary>
        /// Remove the current nodes which are overlapping already added nodes.
        /// </summary>
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

        /// <summary>
        /// Remove the current nodes for which foundation can't be created.
        /// </summary>
        public ProductionProgram CanBeFounded() => Where(node => State.CanBeFounded(node.LE));

        /// <summary>
        /// Remove the current nodes which contain no cubes.
        /// </summary>
        public ProductionProgram NonEmpty() => Where(node => node.LE.Cubes().Any());

        /// <summary>
        /// todo: what is difference from NotTaken?
        /// </summary>
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

        /// <summary>
        /// Runs the program with the current State and CurrentNodes and its state after being run is applied to this.
        /// programF takes new program with the same state as argument.
        /// </summary>
        public ProductionProgram RunIf(bool condition, Func<ProductionProgram, ProductionProgram> programF)
        {
            if (Failed)
                return this;

            if (condition)
            {
                var startingProgram = State.NewProgram(prog => prog);
                startingProgram.CurrentNodes = CurrentNodes;
                startingProgram.Failed = Failed;

                var program = programF(startingProgram);
                AppliedOperations.AddRange(program.AppliedOperations);

                CurrentNodes = program.CurrentNodes;
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

        /// <summary>
        /// Moves current node near to targetNode. None of the cubes of the moved node is vertically taken yet.
        /// Doesn't work correctly for multiple current nodes.
        /// </summary>
        public ProductionProgram MoveNearTo(Node targetNode)
        {
            Change(node => ldk.pl.MoveNearXZ(
                                    targetNode.LE.MoveBottomTo(0),
                                    node.LE.MoveBottomTo(0),
                                    State.VerticallyTaken)?.GN())
            .Change(validNewRoom => validNewRoom.LE.MoveBottomTo(targetNode.LE.CG().LeftBottomBack().y).GN());
            return this;
        }
    }
}

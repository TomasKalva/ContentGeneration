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

        IEnumerable<Node> CurrentNodes { get; set; }
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
            CurrentNodes = node.ToEnumerable();
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
            CurrentNodes = path.ToEnumerable();

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

        public ProductionProgram Found()
        {
            if (Failed)
                return this;

            CurrentNodes = CurrentNodes.Select(node => ldk.sgShapes.Foundation(node.LE).GN(pr.sym.Foundation));
            return this;
        }

        public ProductionProgram ReserveUpward(int height)
        {
            if (Failed)
                return this;

            CurrentNodes = CurrentNodes
                .Select(node =>
                    State.NewProgram()
                        .Set(() => node.LE.CG().ExtrudeVer(Vector3Int.up, height).LE(AreaType.RoomReservation).GN(pr.sym.RoomReservation(node)))
                        .NotTaken()
                )
                .Where(prog => !prog.Failed)
                .SelectMany(prog => prog.CurrentNodes);
            return this;
            
        }

        public ProductionProgram Directional(IEnumerable<Vector3Int> directions, Func<Vector3Int, Node> nodeCreator)
        {
            if (Failed)
                return this;

            CurrentNodes = directions.Select(dir => nodeCreator(dir));
            return this;
        }

        public ProductionProgram NotTaken()
        {
            if (Failed)
                return this;

            CurrentNodes = CurrentNodes.Where(node => node.LE.CG().AllAreNotTaken());
            return this;
        }

        public ProductionProgram CanBeFounded()
        {
            if (Failed)
                return this;

            CurrentNodes = CurrentNodes.Where(node => State.CanBeFounded(node.LE));
            return this;
        }

        public ProductionProgram NonEmpty()
        {
            if (Failed)
                return this;

            CurrentNodes = CurrentNodes.Where(node => node.LE.Cubes().Any());
            return this;
        }

        public ProductionProgram DontIntersectAdded()
        {
            if (Failed)
                return this;

            CurrentNodes = CurrentNodes.Where(node => !node.LE.CG().Intersects(State.WorldState.Added.CG()));
            return this;
        }

        public ProductionProgram Change(Func<Node, Node> changer)
        {
            if (Failed)
                return this;

            CurrentNodes = CurrentNodes.SelectNN(changer);
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

        public ProductionProgram Set(Func<Node> nodesF, out Node result)
        {
            Set(() => nodesF().ToEnumerable());
            result = CurrentNodes.First();
            return this;
        }

        public ProductionProgram Set(Func<Node> nodesF)
        {
            return Set(() => nodesF().ToEnumerable());
        }

        public ProductionProgram Set(Func<IEnumerable<Node>> nodesF)
        {
            if (Failed)
                return this;

            CurrentNodes = nodesF();
            return this;
        }
    }
}

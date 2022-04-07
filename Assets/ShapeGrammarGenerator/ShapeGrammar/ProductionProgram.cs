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
        bool Failed { get; set; }

        IEnumerable<Node> CurrentNodes { get; set; }
        public List<Operation> AppliedOperations { get; }

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

            path = pathFinder();
            Debug.Assert(path != null);
            CurrentNodes = path.ToEnumerable();

            return this;
        }

        /// <summary>
        /// Used, because path finding requires information about floor...
        /// Maybe just assume that entire bottom layer is filled with floor to get rid of this call.
        /// </summary>
        public ProductionProgram ApplyStyles(params Node[] from)
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

            var op = State.Add(from).SetTo(CurrentNodes.ToArray());
            AppliedOperations.Add(op);
            return this;
        }

        public ProductionProgram Found()
        {
            if (Failed)
                return this;

            CurrentNodes = CurrentNodes.Select(node => ldk.sgShapes.Foundation(node.LE).GrammarNode(pr.sym.Foundation));
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

        public ProductionProgram Set(params Node[] nodes)
        {
            if (Failed)
                return this;

            CurrentNodes = nodes;
            return this;
        }
    }
}

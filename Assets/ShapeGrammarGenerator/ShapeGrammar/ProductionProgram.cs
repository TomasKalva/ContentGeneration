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
        ShapeGrammarState State { get; }
        bool Failed { get; set; }

        ProductionProgram SetFailed(bool value)
        {
            Failed = value;
            return this;
        }

        IEnumerable<Node> CurrentNodes { get; set; }
        public List<Operation> AppliedOperations { get; }

        public ProductionProgram(ShapeGrammarState state)
        {
            AppliedOperations = new List<Operation>();
            this.State = state;
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
            return this;
        }

        public ProductionProgram PlaceNode(params Node[] from)
        {
            if (Failed)
                return this;

            var op = State.Add(from).SetTo(CurrentNodes.ToArray());
            AppliedOperations.Add(op);
            return this;
        }

        public ProductionProgram Found(Node what, out Node foundation)
        {
            foundation = null;
            if (Failed)
                return this;

            foundation = ldk.sgShapes.Foundation(what.LE).GrammarNode(pr.sym.Foundation);
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
        /*
        public ProductionProgram GetDirection(PathGuide pathGuide)
        {
            Directions = pathGuide.SelectDirections()
        }*/
    }
}

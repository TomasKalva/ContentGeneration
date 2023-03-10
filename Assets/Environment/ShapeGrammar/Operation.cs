using OurFramework.Environment.GridMembers;
using OurFramework.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OurFramework.Environment.ShapeGrammar
{
    /// <summary>
    /// Declares a relationship between derived symbols.
    /// After declaration it can be used to change ShapeGrammarState.
    /// </summary>
    public abstract class Operation : IPrintable
    {
        public IEnumerable<Node> From { get; set; }
        public IEnumerable<Node> To { get; set; }

        /// <summary>
        /// Creates connections between From and To.
        /// </summary>
        protected void AddIntoDag()
        {
            foreach (var from in From)
            {
                foreach (var to in To)
                {
                    from.AddDerived(to);
                }
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
            foreach (var node in to)
            {
                // Sanity check
                UnityEngine.Debug.Assert(node.LE != null, $"Level element of created node is null!");
            }
            return this;
        }

        /// <summary>
        /// Places the new nodes to the world.
        /// </summary>
        public abstract IEnumerable<Node> ChangeState(ShapeGrammarState grammarState);

        /// <summary>
        /// Adds le to the structure which keeps track of existing foundations cubes.
        /// </summary>
        protected void AddToFoundation(ShapeGrammarState grammarState, LevelElement le)
        {
            le.Cubes().ForEach(cube => grammarState.OffersFoundation[new Vector3Int(cube.Position.x, 0, cube.Position.z)] = false);
            grammarState.VerticallyTaken = grammarState.VerticallyTaken.Merge(le.ProjectToY(0));
        }

        /// <summary>
        /// Adds all nodes from To to structure mapping cubes to nodes.
        /// </summary>
        protected void AddToCubeToNodes(ShapeGrammarState grammarState)
        {
            var sym = new Symbols();
            To.Where(node => !node.HasSymbols(sym.ConnectionMarker))
                .ForEach(node =>
                {
                    node.LE.CG().Cubes.ForEach(cube => grammarState.CubeToNode[cube.Position] = node);
                });
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

    /// <summary>
    /// Adds the new nodes to the state.
    /// </summary>
    public class AddNew : Operation
    {
        public override IEnumerable<Node> ChangeState(ShapeGrammarState grammarState)
        {
            AddIntoDag();
            var lge = To.Select(node => node.LE).ToLevelGroupElement(grammarState.WorldState.Grid);
            /*grammarState.WorldState =*/ grammarState.WorldState.Add(lge);
            AddToCubeToNodes(grammarState);
            AddToFoundation(grammarState, lge);
            return To;
        }

        public override PrintingState Print(PrintingState state)
        {
            state.PrintIndent("Add");
            PrintNodes(state);
            return state;
        }
    }

    /// <summary>
    /// Replaces the old nodes with the new nodes.
    /// </summary>
    public class Replace : Operation
    {
        public override IEnumerable<Node> ChangeState(ShapeGrammarState grammarState)
        {
            AddIntoDag();
            From.ForEach(node =>
            {
                node.LE = LevelElement.Empty(grammarState.WorldState.Grid);
                node.Terminal = true;
            });
            var lge = To.Select(node => node.LE).ToLevelGroupElement(grammarState.WorldState.Grid);
            grammarState.WorldState.Add(lge);
            AddToCubeToNodes(grammarState);
            AddToFoundation(grammarState, lge);
            return To;
        }

        public override PrintingState Print(PrintingState state)
        {
            state.PrintIndent("Replace");
            PrintNodes(state);
            return state;
        }
    }
}

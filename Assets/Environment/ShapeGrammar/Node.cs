using OurFramework.Environment.GridMembers;
using OurFramework.Util;
using System.Collections.Generic;
using System.Linq;

namespace OurFramework.Environment.ShapeGrammar
{
    /// <summary>
    /// Element of shape grammar created by and used as a target for production rules.
    /// </summary>
    public class Node : IPrintable
    {
        List<Symbol> Symbols { get; }
        public IEnumerable<Symbol> GetSymbols => Symbols;
        HashSet<string> SymbolNames { get; }

        public LevelElement LE { get; set; }

        public List<Node> Derived { get; }
        public List<Node> DerivedFrom { get; }
        /// <summary>
        /// Nothing can be derived from terminal nodes.
        /// </summary>
        public bool Terminal { get; set; }

        public Node(LevelElement levelElement, List<Symbol> symbols)
        {
            Symbols = symbols;
            SymbolNames = symbols.Select(sym => sym.Name).ToHashSet();
            LE = levelElement;
            Derived = new List<Node>();
            DerivedFrom = new List<Node>();
            Terminal = false;
        }

        /// <summary>
        /// Returns all nodes that were derived from this node or a node derived from this node.
        /// </summary>
        public IEnumerable<Node> AllDerived()
        {
            var fringe = new Queue<Node>();
            var found = new HashSet<Node>();
            fringe.Enqueue(this);

            while (fringe.Any())
            {
                var node = fringe.Dequeue();
                if (found.Contains(node))
                    continue;

                found.Add(node);
                node.Derived.ForEach(child => fringe.Enqueue(child));
            }
            return found;
        }

        /// <summary>
        /// Returns true iff this contains all symbols.
        /// </summary>
        public bool HasSymbols(params Symbol[] symbols)
        {
            return symbols.All(symbol => SymbolNames.Contains(symbol.Name));
        }

        /// <summary>
        /// Prints symbols of this node.
        /// </summary>
        public PrintingState Print(PrintingState state)
        {
            state.Print("{ ");
            Symbols.ForEach(symbol => symbol.Print(state).Print(", "));
            state.Print("}");
            return state;
        }

        /// <summary>
        /// Returns symbol of the type with same name as symbolType.
        /// </summary>
        public SymbolT GetSymbol<SymbolT>(SymbolT symbolType) where SymbolT : Symbol
        {
            return Symbols.SelectNN(symbol => symbol as SymbolT).Where(symbol => symbol.Name == symbolType.Name).FirstOrDefault();
        }

        /// <summary>
        /// Adds a derived node.
        /// </summary>
        public void AddDerived(Node derived)
        {
            Derived.Add(derived);
            derived.DerivedFrom.Add(this);
        }

        /// <summary>
        /// Adds a symbol.
        /// </summary>
        public void AddSymbol(Symbol symbol)
        {
            Symbols.Add(symbol);
            SymbolNames.Add(symbol.Name);
        }

        /// <summary>
        /// Tries to remove the symbol with same name as argument.
        /// </summary>
        public void RemoveSymbolByName(Symbol symbol)
        {
            Symbols.RemoveAll(s => s.Name == symbol.Name);
            SymbolNames.Remove(symbol.Name);
        }
    }
}

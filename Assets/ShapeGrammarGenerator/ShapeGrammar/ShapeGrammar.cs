using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeGrammar
{
    public class Node
    {
        public HashSet<Symbol> Symbols { get; }

        public LevelElement LevelElement { get; }

        public List<Node> Derived { get; set; }
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

    public class PlayerState 
    {
        public WorldState WorldState { get; }
    }

    public class Production
    {
        Func<PlayerState, bool> CanBeApplied { get; }
        Action<PlayerState> Effect { get; }

        public Production(Func<PlayerState, bool> canBeApplied, Action<PlayerState> effect)
        {
            CanBeApplied = canBeApplied;
            Effect = effect;
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

                });
        }
    }

    public class ShapeGrammar
    {
        Node Root { get; }

        public List<Production> Productions { get; }


    }
}

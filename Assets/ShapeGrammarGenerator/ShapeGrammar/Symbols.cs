using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class Symbols
    {
        public Symbol BrokenFloor { get; } = new Symbol("BrokenFloor");
        public Symbol ConnectTo(Node to) => new ConnectTo("ConnectTo", to);
        public Symbol ExtrudeUp { get; } = new Symbol("ExtrudeUp");
        //public Symbol CreateFrom(params Node[] from) => new CreateFrom("CreateFrom", from.ToList());
        //public Symbol FloorGiver(Node giveTo) => new FloorGiver("FloorGiver", giveTo);
        public Symbol Room(bool plain = true, int floor = 0) => new Room("Room", plain, floor);
        public Symbol Terrace { get; } = new Symbol("Terrace");
        public Symbol Roof { get; } = new Symbol("Roof");
        public Symbol Courtyard { get; } = new Symbol("Courtyard");
        public Symbol Foundation { get; } = new Symbol("Foundation");
        public Symbol Bridge(Vector3Int direction = default) => new Bridge("Bridge", direction);
        public Symbol Garden { get; } = new Symbol("Garden");
    }

    public class Symbol : Printable
    {
        public string Name { get; }

        public Symbol(string name)
        {
            Name = name;
        }

        public PrintingState Print(PrintingState state)
        {
            return state.Print(Name);
        }
    }

    public class ConnectTo : Symbol
    {
        public Node To { get; }

        public ConnectTo(string name, Node to) : base(name)
        {
            To = to;
        }
    }

    public class CreateFrom : Symbol
    {
        public List<Node> From { get; }

        public CreateFrom(string name, List<Node> from) : base(name)
        {
            From = from;
        }
    }

    public class FloorGiver : Symbol
    {
        public Node GiveTo { get; }

        public FloorGiver(string name, Node giveTo) : base(name)
        {
            GiveTo = giveTo;
        }
    }

    public class Bridge : Symbol
    {
        public Vector3Int Direction { get; }

        public Bridge(string name, Vector3Int direction) : base(name)
        {
            Direction = direction;
        }
    }

    public class Room : Symbol
    {
        public bool Plain { get; set; }
        public int Floor { get; set; }

        public Room(string name, bool plain, int floor) : base(name)
        {
            Plain = plain;
            Floor = floor;
        }
    }
}

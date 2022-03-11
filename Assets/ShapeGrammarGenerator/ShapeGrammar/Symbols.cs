﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeGrammar
{
    public class Symbols
    {
        public Symbol BrokenFloor { get; } = new Symbol("BrokenFloor");
        public Symbol ConnectTo(Node to) => new ConnectTo("ConnectTo", to);
        public Symbol ExtrudeUp { get; } = new Symbol("ExtrudeUp");
        public Symbol CreateFrom(params Node[] from) => new CreateFrom("CreateFrom", from.ToList());
        public Symbol FloorGiver(Node giveTo) => new FloorGiver("FloorGiver", giveTo);
        public Symbol Room { get; } = new Symbol("Room");
        public Symbol Terrace { get; } = new Symbol("Terrace");
        public Symbol Roof { get; } = new Symbol("Roof");
        public Symbol Courtyard { get; } = new Symbol("Courtyard");
        public Symbol Foundation { get; } = new Symbol("Foundation");
        public Symbol Bridge { get; } = new Symbol("Bridge");
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
}

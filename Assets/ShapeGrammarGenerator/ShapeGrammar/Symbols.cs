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
        //public Symbol BrokenFloor { get; } = new Symbol("BrokenFloor");
        //public Symbol ConnectTo(Node to) => new ConnectTo("ConnectTo", to);
        //public Symbol ExtrudeUp { get; } = new Symbol("ExtrudeUp");
        //public Symbol CreateFrom(params Node[] from) => new CreateFrom("CreateFrom", from.ToList());
        //public Symbol FloorGiver(Node giveTo) => new FloorGiver("FloorGiver", giveTo);

        #region Common symbols
        public Symbol Foundation { get; } = new Symbol("Foundation");
        #endregion




        public Symbol Room { get; } = new Symbol("Room");
        public Symbol BrokenFloorRoom { get; } = new Symbol("BrokenFloorRoom");
        //public Symbol DirectedRoom(Vector3Int direction, int floor = 0) => new DirectedRoom("DirectedRoom", false, floor, direction);
        /// <summary>
        /// Serves as a space that can be turned into another part of a building or in a roof.
        /// </summary>
        public UpwardReservation UpwardReservation(Node roomBelow) => new UpwardReservation("RoomReservation", roomBelow);
        public Symbol Terrace(Vector3Int direction) => new DirectionalSymbol("Terrace", direction);
        public Symbol Roof { get; } = new Symbol("Roof");
        public Symbol Courtyard { get; } = new Symbol("Courtyard");
        public DirectionalSymbol Bridge(Vector3Int direction = default) => new DirectionalSymbol("Bridge", direction);
        public Symbol Garden { get; } = new Symbol("Garden");

        #region Graveyard
        public Symbol Park { get; } = new Symbol("Park");
        public Symbol ChapelEntrance { get; } = new Symbol("ChapelEntrance");
        public DirectionalSymbol ChapelHall(Vector3Int direction) => new DirectionalSymbol("ChapelHall", direction);
        public Symbol ChapelRoom { get; } = new Symbol("ChapelRoom");
        public Symbol ChapelTowerTop { get; } = new Symbol("ChapelTowerTop");
        public Symbol ChapelSide(Vector3Int direction) => new DirectionalSymbol("ChapelSide", direction);
        #endregion

        #region Castle
        public Symbol TowerBottom { get; } = new Symbol("TowerBottom");
        public Symbol TowerTop { get; } = new Symbol("TowerTop");
        public DirectionalSymbol WallTop(Vector3Int direction) => new DirectionalSymbol("WallTop", direction);
        public DirectionalSymbol SideWallTop(Vector3Int direction) => new DirectionalSymbol("SideWallTop", direction);
        #endregion

        public Symbol StartMarker { get; } = new Marker("Start");
        public Symbol EndMarker { get; } = new Marker("End");
        public Symbol ReturnToMarker { get; } = new Marker("ReturnTo");
        public Symbol FullFloorMarker { get; } = new Marker("FullFloor");
        public Symbol ConnectionMarker { get; } = new Marker("Connection");
        public Symbol LevelStartMarker { get; } = new Marker("LevelStart");
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



    public class DirectionalSymbol : Symbol
    {
        public Vector3Int Direction { get; }
        
        public DirectionalSymbol(string name, Vector3Int direction) : base(name)
        {
            Direction = direction;
        }
    }

    /*
    public class ChapelRoom : Symbol
    {
        public ChapelRoom(string name) : base(name)
        {
        }
    }*/

    public class Marker : Symbol
    {
        public Marker(string name) : base(name)
        {
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

    public class UpwardReservation : Symbol
    {
        public Node SomethingBelow { get; }

        public UpwardReservation(string name, Node roomBelow) : base(name)
        {
            SomethingBelow = roomBelow;
        }
    }
}

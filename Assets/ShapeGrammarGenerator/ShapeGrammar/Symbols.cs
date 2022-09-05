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
        #region Common symbols
        public Symbol Foundation { get; } = new Symbol("Foundation");
        /// <summary>
        /// Serves as a space that can be turned into another part of a building or in a roof.
        /// </summary>
        public ReferenceSymbol UpwardReservation(Node roomBelow) => new ReferenceSymbol("RoomReservation", roomBelow);
        #endregion

        public Symbol BrokenFloorRoom { get; } = new Symbol("BrokenFloorRoom");


        #region Town
        public Symbol Room { get; } = new Symbol("Room");
        public DirectionalSymbol Terrace(Vector3Int direction) => new DirectionalSymbol("Terrace", direction);
        public Symbol Roof { get; } = new Symbol("Roof");
        public Symbol Courtyard { get; } = new Symbol("Courtyard");
        public DirectionalSymbol Bridge(Vector3Int direction = default) => new DirectionalSymbol("Bridge", direction);
        public Symbol Garden { get; } = new Symbol("Garden");
        public DirectionalSymbol Balcony(Vector3Int direction) => new DirectionalSymbol("Balcony", direction);
        #endregion

        #region Graveyard
        public Symbol Park { get; } = new Symbol("Park");
        public Symbol ChapelEntrance { get; } = new Symbol("ChapelEntrance");
        public DirectionalSymbol ChapelHall(Vector3Int direction) => new DirectionalSymbol("ChapelHall", direction);
        public Symbol ChapelRoom { get; } = new Symbol("ChapelRoom");
        public Symbol ChapelTowerTop { get; } = new Symbol("ChapelTowerTop");
        public DirectionalSymbol ChapelSide(Vector3Int direction) => new DirectionalSymbol("ChapelSide", direction);
        #endregion

        #region Castle
        public Symbol TowerBottom { get; } = new Symbol("TowerBottom");
        public Symbol TowerTop { get; } = new Symbol("TowerTop");
        public DirectionalSymbol WallTop(Vector3Int direction) => new DirectionalSymbol("WallTop", direction);

        public DirectionalSymbol SideWallTop(Vector3Int direction) => new DirectionalSymbol("SideWallTop", direction);
        public Symbol WatchPost { get; } = new Symbol("WatchPost");
        #endregion

        #region Utility symbols
        public Symbol StartMarker { get; } = new Marker("Start");
        public Symbol EndMarker { get; } = new Marker("End");
        public Symbol ReturnToMarker { get; } = new Marker("ReturnTo");
        public Symbol FullFloorMarker { get; } = new Marker("FullFloor");
        public Symbol ConnectionMarker { get; } = new Marker("Connection");
        public Symbol LevelStartMarker { get; } = new Marker("LevelStart");
        #endregion
    }

    /// <summary>
    /// Used by production rules to decide to which nodes to apply.
    /// </summary>
    public class Symbol : IPrintable
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

    /// <summary>
    /// Holds a direction.
    /// </summary>
    public class DirectionalSymbol : Symbol
    {
        public Vector3Int Direction { get; }
        
        public DirectionalSymbol(string name, Vector3Int direction) : base(name)
        {
            Direction = direction;
        }
    }

    public class Marker : Symbol
    {
        public Marker(string name) : base(name)
        {
        }
    }

    /// <summary>
    /// Holds a reference to a node.
    /// </summary>
    public class ReferenceSymbol : Symbol
    {
        public Node NodeReference { get; }

        public ReferenceSymbol(string name, Node roomBelow) : base(name)
        {
            NodeReference = roomBelow;
        }
    }
}

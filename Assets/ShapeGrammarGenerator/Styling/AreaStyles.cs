using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.ShapeGrammarGenerator
{
    public static class AreaStyles
    {
        static GridPrimitives gm { get; set; }
        static GridPrimitivesPlacement gpp { get; set; }

        public static void Initialize(GridPrimitives gridPrimitives, GridPrimitivesPlacement gridPrimitivesPlacement)
        {
            gm = gridPrimitives;
            gpp = gridPrimitivesPlacement;
        }

        public static  AreaStyle Room() => new AreaStyle("Room", gm.DefaultStyle(), gpp.PlainRoomStyle);

        public static  AreaStyle Reservation() => new AreaStyle("Reservation", gm.DefaultStyle(), gpp.EmptyStyle);

        public static  AreaStyle OpenRoom() => new AreaStyle("OpenRoom", gm.DefaultStyle(), gpp.OpenRoomStyle);

        public static  AreaStyle FlatRoof() => new AreaStyle("FlatRoof", gm.DefaultStyle(), gpp.FlatRoofStyle);

        public static  AreaStyle GableRoof() => new AreaStyle("GableRoof", gm.DefaultStyle(), gpp.GableRoofStyle);

        public static  AreaStyle PointyRoof() => new AreaStyle("PointyRoof", gm.DefaultStyle(), gpp.PointyRoofStyle);

        public static  AreaStyle CrossRoof() => new AreaStyle("CrossRoof", gm.DefaultStyle(), gpp.CrossRoofStyle);

        public static AreaStyle DirectionalRoof(Vector3Int direction) => new AreaStyle("DirectionalRoof", gm.DefaultStyle(), (gpStyle, roofArea) => gpp.DirectionalRoofStyle(gpStyle, roofArea, direction));

        public static  AreaStyle Foundation() => new AreaStyle("Foundation", gm.DefaultStyle(), gpp.FoundationStyle);

        public static  AreaStyle Path() => new AreaStyle("Path", gm.DefaultStyle(), gpp.StairsPathStyle);

        public static  AreaStyle Garden() => new AreaStyle("Garden", gm.DefaultStyle(), gpp.GardenStyle);

        public static  AreaStyle Yard() => new AreaStyle("Yard", gm.DefaultStyle(), gpp.FlatRoofStyle);

        public static  AreaStyle WallTop() => new AreaStyle("WallTop", gm.DefaultStyle(), gpp.FlatRoofStyle);

        public static  AreaStyle Wall() => new AreaStyle("Wall", gm.DefaultStyle(), gpp.FoundationStyle);

        public static  AreaStyle CliffFoundation() => new AreaStyle("CliffFoundation", gm.DefaultStyle(), gpp.CliffFoundationStyle);

        public static  AreaStyle Empty() => new AreaStyle("Empty", gm.DefaultStyle(), gpp.EmptyStyle);

        public static AreaStyle None() => new AreaStyle("None", gm.DefaultStyle(), gpp.EmptyStyle);

        public static  AreaStyle Inside() => new AreaStyle("Inside", gm.DefaultStyle(), gpp.RoomStyle);

        public static  AreaStyle Platform() => new AreaStyle("Platform", gm.DefaultStyle(), gpp.PlatformStyle);

        public static  AreaStyle Debug() => new AreaStyle("Debug", gm.DefaultStyle(), gpp.RoomStyle);

        public static  AreaStyle Colonnade() => new AreaStyle("Colonnade", gm.DefaultStyle(), gpp.ColonnadeStyle);

        public static  AreaStyle Fall() => new AreaStyle("Fall", gm.DefaultStyle(), gpp.FallStyle);

        public static  AreaStyle Door() => new AreaStyle("Door", gm.DefaultStyle(), gpp.DoorStyle);

        public static  AreaStyle Bridge() => new AreaStyle("Bridge", gm.DefaultStyle(), gpp.FlatRoofStyle);
        public static AreaStyle BridgeTopFoundation(Vector3Int bridgeDirection) => new AreaStyle("BridgeTopFoundation", gm.DefaultStyle(), (gpStyle, roofArea) => gpp.PlaceInDirection(roofArea, gpStyle.BridgeTop(), bridgeDirection));
        public static AreaStyle BridgeBottomFoundation(Vector3Int bridgeDirection) => new AreaStyle("BridgeBottomFoundation", gm.DefaultStyle(), (gpStyle, roofArea) => gpp.PlaceInDirection(roofArea, gpStyle.BridgeBottom(), bridgeDirection));

        //public static  AreaStyle NoFloor() => new AreaStyle("NoFloor", gm.DefaultStyle(), gpp.NoFloor);
    }
}

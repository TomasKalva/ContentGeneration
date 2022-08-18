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
        static GridPrimitives gp { get; set; }
        static GridPrimitivesPlacement gpp { get; set; }
        public static GridPrimitivesStyle TownStyle => gp.TownStyle();
        public static GridPrimitivesStyle ChapelStyle => gp.ChapelStyle();
        public static GridPrimitivesStyle GardenStyle => gp.GardenStyle();
        public static GridPrimitivesStyle CastleStyle => gp.CastleStyle();

        public static void Initialize(GridPrimitives gridPrimitives, GridPrimitivesPlacement gridPrimitivesPlacement)
        {
            gp = gridPrimitives;
            gpp = gridPrimitivesPlacement;
        }

        static GridPrimitivesStyle GetStyle(GridPrimitivesStyle styleOrNull) => styleOrNull ?? TownStyle;

        public static  AreaStyle Room(GridPrimitivesStyle style = null) => new AreaStyle("Room", GetStyle(style), gpp.PlainRoomStyle);

        public static  AreaStyle Reservation() => new AreaStyle("Reservation", gp.TownStyle(), gpp.EmptyStyle);

        public static  AreaStyle OpenRoom() => new AreaStyle("OpenRoom", gp.TownStyle(), gpp.OpenRoomStyle);

        public static  AreaStyle FlatRoof() => new AreaStyle("FlatRoof", gp.TownStyle(), gpp.FlatRoofStyle);

        public static  AreaStyle GableRoof() => new AreaStyle("GableRoof", gp.TownStyle(), gpp.GableRoofStyle);

        public static  AreaStyle PointyRoof(GridPrimitivesStyle style = null) => new AreaStyle("PointyRoof", GetStyle(style), gpp.PointyRoofStyle);

        public static  AreaStyle CrossRoof() => new AreaStyle("CrossRoof", gp.TownStyle(), gpp.CrossRoofStyle);

        public static AreaStyle DirectionalRoof(Vector3Int direction) => new AreaStyle("DirectionalRoof", gp.TownStyle(), (gpStyle, roofArea) => gpp.DirectionalRoofStyle(gpStyle, roofArea, direction));

        public static  AreaStyle Foundation() => new AreaStyle("Foundation", gp.TownStyle(), gpp.FoundationStyle);

        public static  AreaStyle Path() => new AreaStyle("Path", gp.TownStyle(), gpp.StairsPathStyle);

        public static  AreaStyle Garden() => new AreaStyle("Garden", gp.GardenStyle(), gpp.GardenStyle);

        public static  AreaStyle Yard() => new AreaStyle("Yard", gp.TownStyle(), gpp.FlatRoofStyle);

        public static  AreaStyle WallTop() => new AreaStyle("WallTop", gp.TownStyle(), gpp.FlatRoofStyle);

        public static  AreaStyle Wall() => new AreaStyle("Wall", gp.TownStyle(), gpp.FoundationStyle);

        public static  AreaStyle CliffFoundation() => new AreaStyle("CliffFoundation", gp.TownStyle(), gpp.CliffFoundationStyle);

        public static  AreaStyle Empty() => new AreaStyle("Empty", gp.TownStyle(), gpp.EmptyStyle);

        public static AreaStyle None() => new AreaStyle("None", gp.TownStyle(), gpp.EmptyStyle);

        public static  AreaStyle Inside() => new AreaStyle("Inside", gp.TownStyle(), gpp.RoomStyle);

        public static  AreaStyle Platform() => new AreaStyle("Platform", gp.TownStyle(), gpp.PlatformStyle);

        public static  AreaStyle Debug() => new AreaStyle("Debug", gp.TownStyle(), gpp.RoomStyle);

        public static  AreaStyle Colonnade(GridPrimitivesStyle style = null) => new AreaStyle("Colonnade", GetStyle(style), gpp.ColonnadeStyle);


        public static  AreaStyle Fall() => new AreaStyle("Fall", gp.TownStyle(), gpp.FallStyle);

        public static  AreaStyle Connection() => new AreaStyle("Door", gp.TownStyle(), gpp.ConnectionStyle);

        public static AreaStyle BridgeTop(Vector3Int bridgeDirection) => new AreaStyle("BridgeTop", gp.TownStyle(), (gpStyle, bridgeArea) => gpp.BridgeStyle(gpStyle, bridgeArea, bridgeDirection));
        public static AreaStyle BridgeTopFoundation(Vector3Int bridgeDirection) => new AreaStyle("BridgeTopFoundation", gp.TownStyle(), (gpStyle, roofArea) => gpp.PlaceInDirection(roofArea, gpStyle.BridgeTop, bridgeDirection));
        public static AreaStyle BridgeBottomFoundation(Vector3Int bridgeDirection) => new AreaStyle("BridgeBottomFoundation", gp.TownStyle(), (gpStyle, roofArea) => gpp.PlaceInDirection(roofArea, gpStyle.BridgeBottom, bridgeDirection));

        //public static  AreaStyle NoFloor() => new AreaStyle("NoFloor", gm.DefaultStyle(), gpp.NoFloor);
    }
}

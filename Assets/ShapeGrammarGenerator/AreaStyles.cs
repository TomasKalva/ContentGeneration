using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator
{
    public static class AreaStyles
    {
        static GridPrimitives gm { get; set; }
        static ShapeGrammarStyles sgStyles { get; set; }

        public static void Initialize(GridPrimitives gridPrimitives, ShapeGrammarStyles shapeGrammarStyles)
        {
            gm = gridPrimitives;
            sgStyles = shapeGrammarStyles;
        }

        public static  AreaStyle Room() => new AreaStyle("Room", cg => sgStyles.PlainRoomStyle(cg));

        public static  AreaStyle Reservation() => new AreaStyle("Reservation", cg => sgStyles.EmptyStyle(cg));

        public static  AreaStyle OpenRoom() => new AreaStyle("OpenRoom", cg => sgStyles.OpenRoomStyle(cg));

        public static  AreaStyle FlatRoof() => new AreaStyle("FlatRoof", cg => sgStyles.FlatRoofStyle(cg));

        public static AreaStyle Balcony() => new AreaStyle("Balcony", cg => sgStyles.FlatRoofStyle(cg));

        public static  AreaStyle GableRoof() => new AreaStyle("FlatRoof", cg => sgStyles.FlatRoofStyle(cg));// new AreaStyle("GableRoof", cg => sgStyles.GableRoofStyle(cg));

        public static  AreaStyle PointyRoof() => new AreaStyle("FlatRoof", cg => sgStyles.FlatRoofStyle(cg));// new AreaStyle("PointyRoof", cg => area => (cg));

        public static  AreaStyle CrossRoof() => new AreaStyle("FlatRoof", cg => sgStyles.FlatRoofStyle(cg));// new AreaStyle("CrossRoof", cg => area => (cg));

        public static  AreaStyle Foundation() => new AreaStyle("Foundation", cg => sgStyles.FoundationStyle(cg));

        public static  AreaStyle Path() => new AreaStyle("Path", cg => sgStyles.StairsPathStyle(cg));

        public static  AreaStyle Garden() => new AreaStyle("Garden", cg => sgStyles.GardenStyle(cg));

        public static  AreaStyle Yard() => new AreaStyle("Yard", cg => sgStyles.FlatRoofStyle(cg));

        public static  AreaStyle WallTop() => new AreaStyle("WallTop", cg => sgStyles.FlatRoofStyle(cg));

        public static  AreaStyle Wall() => new AreaStyle("Wall", cg => sgStyles.FoundationStyle(cg));

        public static  AreaStyle CliffFoundation() => new AreaStyle("CliffFoundation", cg => sgStyles.CliffFoundationStyle(cg));

        public static  AreaStyle Empty() => new AreaStyle("Empty", cg => sgStyles.EmptyStyle(cg));

        public static AreaStyle None() => new AreaStyle("None", cg => sgStyles.EmptyStyle(cg));

        public static  AreaStyle Inside() => new AreaStyle("Inside", cg => sgStyles.RoomStyle(cg));

        public static  AreaStyle Platform() => new AreaStyle("Platform", cg => sgStyles.PlatformStyle(cg));

        public static  AreaStyle Debug() => new AreaStyle("Debug", cg => sgStyles.RoomStyle(cg));

        public static  AreaStyle Colonnade() => new AreaStyle("Colonnade", cg => sgStyles.ColonnadeStyle(cg));

        public static  AreaStyle Fall() => new AreaStyle("Fall", cg => sgStyles.FallStyle(cg));

        public static  AreaStyle Door() => new AreaStyle("Door", cg => sgStyles.DoorStyle(cg));

        public static  AreaStyle Bridge() => new AreaStyle("Bridge", cg => sgStyles.FlatRoofStyle(cg));

        public static  AreaStyle NoFloor() => new AreaStyle("NoFloor", cg => sgStyles.NoFloor(cg));
    }
}

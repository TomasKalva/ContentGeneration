using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator
{
    public class AreaStyles
    {
        GridPrimitives gm { get; }
        ShapeGrammarStyles sgStyles { get; }

        public AreaStyles(GridPrimitives gm, ShapeGrammarStyles sgStyles)
        {
            this.gm = gm;
            this.sgStyles = sgStyles;
        }

        public AreaStyle Room() => new AreaStyle("Room", cg => sgStyles.PlainRoomStyle(cg));

        public AreaStyle Reservation() => new AreaStyle("Reservation", cg => sgStyles.EmptyStyle(cg));

        public AreaStyle OpenRoom() => new AreaStyle("OpenRoom", cg => sgStyles.OpenRoomStyle(cg));

        public AreaStyle FlatRoof() => new AreaStyle("FlatRoof", cg => sgStyles.FlatRoofStyle(cg));

        public AreaStyle GableRoof() => new AreaStyle("FlatRoof", cg => sgStyles.FlatRoofStyle(cg));// new AreaStyle("GableRoof", cg => sgStyles.GableRoofStyle(cg));

        public AreaStyle PointyRoof() => new AreaStyle("FlatRoof", cg => sgStyles.FlatRoofStyle(cg));// new AreaStyle("PointyRoof", cg => area => (cg));

        public AreaStyle CrossRoof() => new AreaStyle("FlatRoof", cg => sgStyles.FlatRoofStyle(cg));// new AreaStyle("CrossRoof", cg => area => (cg));

        public AreaStyle Foundation() => new AreaStyle("Foundation", cg => sgStyles.FoundationStyle(cg));

        public AreaStyle Path() => new AreaStyle("Path", cg => sgStyles.StairsPathStyle(cg));

        public AreaStyle Garden() => new AreaStyle("Garden", cg => sgStyles.GardenStyle(cg));

        public AreaStyle Yard() => new AreaStyle("Yard", cg => sgStyles.FlatRoofStyle(cg));

        public AreaStyle WallTop() => new AreaStyle("WallTop", cg => sgStyles.FlatRoofStyle(cg));

        public AreaStyle Wall() => new AreaStyle("Wall", cg => sgStyles.FoundationStyle(cg));

        public AreaStyle CliffFoundation() => new AreaStyle("CliffFoundation", cg => sgStyles.CliffFoundationStyle(cg));

        public AreaStyle Empty() => new AreaStyle("Empty", cg => sgStyles.EmptyStyle(cg));

        public AreaStyle Inside() => new AreaStyle("Inside", cg => sgStyles.RoomStyle(cg));

        public AreaStyle Platform() => new AreaStyle("Platform", cg => sgStyles.PlatformStyle(cg));

        public AreaStyle Debug() => new AreaStyle("Debug", cg => sgStyles.RoomStyle(cg));

        public AreaStyle Colonnade() => new AreaStyle("Colonnade", cg => sgStyles.ColonnadeStyle(cg));

        public AreaStyle Fall() => new AreaStyle("Fall", cg => sgStyles.FallStyle(cg));

        public AreaStyle Door() => new AreaStyle("Door", cg => sgStyles.DoorStyle(cg));

        public AreaStyle Bridge() => new AreaStyle("Bridge", cg => sgStyles.FlatRoofStyle(cg));

        public AreaStyle NoFloor() => new AreaStyle("NoFloor", cg => sgStyles.NoFloor(cg));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeGrammar
{
    /*
    public class AreaStyle
    {
        public static AreaStyle WorldRoot { get; } = new AreaStyle("WorldRoot");
        public static AreaStyle None { get; } = new AreaStyle("None");
        public static AreaStyle Room { get; } = new AreaStyle("Room");
        public static AreaStyle Reservation { get; } = new AreaStyle("RoomReservation");
        public static AreaStyle OpenRoom { get; } = new AreaStyle("OpenRoom");
        public static AreaStyle FlatRoof { get; } = new AreaStyle("FlatRoof");
        public static AreaStyle GableRoof { get; } = new AreaStyle("GableRoof");
        public static AreaStyle PointyRoof { get; } = new AreaStyle("PointyRoof");
        public static AreaStyle CrossRoof { get; } = new AreaStyle("CrossRoof");
        public static AreaStyle Foundation { get; } = new AreaStyle("Foundation");
        public static AreaStyle CliffFoundation { get; } = new AreaStyle("CliffFoundation");
        public static AreaStyle Balcony { get; } = new AreaStyle("Balcony");
        public static AreaStyle House { get; } = new AreaStyle("House");
        public static AreaStyle WallTop { get; } = new AreaStyle("WallTop");
        public static AreaStyle Wall { get; } = new AreaStyle("Wall");
        public static AreaStyle Garden { get; } = new AreaStyle("Garden");
        public static AreaStyle Yard { get; } = new AreaStyle("Yard");
        public static AreaStyle Path { get; } = new AreaStyle("Path");
        public static AreaStyle Fall { get; } = new AreaStyle("Fall");
        public static AreaStyle Empty { get; } = new AreaStyle("Empty");
        public static AreaStyle Inside { get; } = new AreaStyle("Inside");
        public static AreaStyle Platform { get; } = new AreaStyle("Platform");
        public static AreaStyle Debug { get; } = new AreaStyle("Debug");
        public static AreaStyle Colonnade { get; } = new AreaStyle("Colonnade");
        public static AreaStyle Elevator { get; } = new AreaStyle("Elevator");
        public static AreaStyle Door { get; } = new AreaStyle("Door");
        public static AreaStyle Bridge { get; } = new AreaStyle("Bridge");
        public static AreaStyle NoFloor { get; } = new AreaStyle("NoFloor");

        public static HashSet<AreaStyle> ConnectableByStairs { get; }

        static AreaStyle()
        {
            ConnectableByStairs = new HashSet<AreaStyle>()
            {
                Room,
                OpenRoom,
                FlatRoof,
                WallTop,
                Garden,
                Platform
            };
        }

        public static bool CanBeConnectedByStairs(AreaStyle areaType) => ConnectableByStairs.Contains(areaType);

        public string Name { get; }

        private AreaStyle(string name)
        {
            Name = name;
        }
    }*/
    /*
    public delegate IEnumerable<LevelElement> StyleSelector(LevelElement cubeGroupGroup);

    public delegate CubeGroup ApplyStyle(CubeGroup cubeGroup);

    public class StyleRule
    {
        public StyleSelector Selector { get; }
        public ApplyStyle Setter { get; }

        public StyleRule(StyleSelector selector, ApplyStyle setter)
        {
            Selector = selector;
            Setter = setter;
        }
    }*/

    public class StyleApplier
    {
        /*
        StyleRule[] rules;

        public StyleRules(params StyleRule[] rules)
        {
            this.rules = rules;
        }*/

        public void Apply(LevelElement levelElement)
        {
            levelElement.Leafs().ForEach(le => le.ApplyStyle());
        }
    }
}

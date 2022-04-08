using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeGrammar
{
    public class AreaType
    {
        public static AreaType WorldRoot { get; } = new AreaType("WorldRoot");
        public static AreaType None { get; } = new AreaType("None");
        public static AreaType Room { get; } = new AreaType("Room");
        public static AreaType Reservation { get; } = new AreaType("RoomReservation");
        public static AreaType OpenRoom { get; } = new AreaType("OpenRoom");
        public static AreaType FlatRoof { get; } = new AreaType("FlatRoof");
        public static AreaType Roof { get; } = new AreaType("Roof");
        public static AreaType Foundation { get; } = new AreaType("Foundation");
        public static AreaType CliffFoundation { get; } = new AreaType("CliffFoundation");
        public static AreaType Balcony { get; } = new AreaType("Balcony");
        public static AreaType House { get; } = new AreaType("House");
        public static AreaType WallTop { get; } = new AreaType("WallTop");
        public static AreaType Wall { get; } = new AreaType("Wall");
        public static AreaType Garden { get; } = new AreaType("Garden");
        public static AreaType Yard { get; } = new AreaType("Yard");
        public static AreaType Path { get; } = new AreaType("Path");
        public static AreaType Fall { get; } = new AreaType("Fall");
        public static AreaType Empty { get; } = new AreaType("Empty");
        public static AreaType Inside { get; } = new AreaType("Inside");
        public static AreaType Platform { get; } = new AreaType("Platform");
        public static AreaType Debug { get; } = new AreaType("Debug");
        public static AreaType Colonnade { get; } = new AreaType("Colonnade");
        public static AreaType Elevator { get; } = new AreaType("Elevator");
        public static AreaType Door { get; } = new AreaType("Door");
        public static AreaType Bridge { get; } = new AreaType("Bridge");
        public static AreaType NoFloor { get; } = new AreaType("NoFloor");

        public static HashSet<AreaType> ConnectableByStairs { get; }

        static AreaType()
        {
            ConnectableByStairs = new HashSet<AreaType>()
            {
                Room,
                OpenRoom,
                FlatRoof,
                WallTop,
                Garden,
                Platform
            };
        }

        public static bool CanBeConnectedByStairs(AreaType areaType) => ConnectableByStairs.Contains(areaType);

        public string Name { get; }

        private AreaType(string name)
        {
            Name = name;
        }
    }

    public delegate IEnumerable<LevelElement> StyleSelector(LevelElement cubeGroupGroup);

    public delegate CubeGroup StyleSetter(CubeGroup cubeGroup);

    public class StyleRule
    {
        public StyleSelector Selector { get; }
        public StyleSetter Setter { get; }

        public StyleRule(StyleSelector selector, StyleSetter setter)
        {
            Selector = selector;
            Setter = setter;
        }
    }

    public class StyleRules
    {
        StyleRule[] rules;

        public StyleRules(params StyleRule[] rules)
        {
            this.rules = rules;
        }

        public void Apply(LevelElement levelElement)
        {
            rules.ForEach(rule => rule.Selector(levelElement).ForEach(le => le.SetGrammarStyle(rule.Setter)));
        }
    }
}

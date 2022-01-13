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
        public static AreaType Roof { get; } = new AreaType("Roof");
        public static AreaType Foundation { get; } = new AreaType("Foundation");
        public static AreaType Balcony { get; } = new AreaType("Balcony");
        public static AreaType House { get; } = new AreaType("House");
        public static AreaType WallTop { get; } = new AreaType("WallTop");
        public static AreaType Wall { get; } = new AreaType("Wall");
        public static AreaType Garden { get; } = new AreaType("Garden");

        public string Name { get; }

        private AreaType(string name)
        {
            Name = name;
        }
    }

    public delegate CubeGroupGroup StyleSelector(CubeGroupGroup cubeGroupGroup);

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

        public void Apply(CubeGroupGroup cubeGroupGroup)
        {
            rules.ForEach(rule => rule.Selector(cubeGroupGroup).SetGrammarStyle(rule.Setter));
        }
    }
}

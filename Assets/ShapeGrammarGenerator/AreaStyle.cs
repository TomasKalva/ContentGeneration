using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator
{
    public delegate CubeGroup ApplyStyle(CubeGroup target);

    public class AreaStyle
    {
        public string Name { get; }
        public ApplyStyle ApplyStyle { get; }

        public AreaStyle(string name, ApplyStyle applyStyle)
        {
            Name = name;
            ApplyStyle = applyStyle;
        }
    }
}

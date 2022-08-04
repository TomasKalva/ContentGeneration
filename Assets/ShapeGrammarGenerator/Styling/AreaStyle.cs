using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator
{
    public delegate CubeGroup ApplyStyle(GridPrimitivesStyle gridPrimitivesStyle, CubeGroup target);

    public class AreaStyle
    {
        public string Name { get; }
        ApplyStyle ApplyStyleF { get; }
        GridPrimitivesStyle GridPrimitivesStyle { get; }

        public AreaStyle(string name, GridPrimitivesStyle gridPrimitiveStyle, ApplyStyle applyStyle)
        {
            Name = name;
            ApplyStyleF = applyStyle;
            GridPrimitivesStyle = gridPrimitiveStyle;
        }

        public CubeGroup ApplyStyle(CubeGroup cg) => ApplyStyleF(GridPrimitivesStyle, cg);
    }
}

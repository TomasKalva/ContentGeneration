﻿using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator
{
    public delegate CubeGroup PlacePrimitives(GridPrimitivesStyle gridPrimitivesStyle, CubeGroup target);

    public class AreaStyle
    {
        public string Name { get; }
        PlacePrimitives PlacePrimitivesF { get; }
        GridPrimitivesStyle GridPrimitivesStyle { get; }

        public AreaStyle(string name, GridPrimitivesStyle gridPrimitiveStyle, PlacePrimitives applyStyle)
        {
            Name = name;
            PlacePrimitivesF = applyStyle;
            GridPrimitivesStyle = gridPrimitiveStyle;
        }

        public CubeGroup ApplyStyle(CubeGroup cg) => PlacePrimitivesF(GridPrimitivesStyle, cg);
    }
}

using OurFramework.Environment.GridMembers;

namespace OurFramework.Environment.StylingAreas
{
    public delegate CubeGroup PlacePrimitives(GridPrimitivesStyle gridPrimitivesStyle, CubeGroup target);

    /// <summary>
    /// Style of level element. Can be applied to the underlying cube group.
    /// </summary>
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

using OurFramework.Environment.GridMembers;
using OurFramework.Environment.StylingAreas;
using UnityEngine;

namespace OurFramework.Environment.ShapeCreation
{
    /// <summary>
    /// Tools used when creating geometry of levels.
    /// </summary>
    public class LevelDevelopmentKit
    {
        public Grid<Cube> grid { get; }
        public CGShapes cgs { get; }
        public LEShapes les { get; }
        public Connections con { get; }

        public LevelDevelopmentKit(GeometricPrimitives gp)
        {
            grid = new Grid<Cube>(new Vector3Int(20, 10, 20), (grid, pos) => new Cube(grid, pos));
            cgs = new CGShapes(grid);
            les = new LEShapes(grid);
            con = new Connections();
            AreaStyles.Initialize(new GridPrimitives(gp), new GridPrimitivesPlacement(grid));
        }
    }
}

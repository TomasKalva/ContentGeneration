using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator
{
    public class GridPrimitives
    {
        GeometricPrimitives gp;

        public GridPrimitives(GeometricPrimitives gp)
        {
            this.gp = gp;
        }

        public WallPrimitive HouseWall()
            => new WallPrimitive(gp.BrickWall, gp.BrickWall);// todo: replace two sided walls with only one sided ones

        public HorFaceExclusivePrimitive Door()
            => new HorFaceExclusivePrimitive(gp.WallDoor);//todo: replace with actual door primitive

        public HorFaceExclusivePrimitive Railing()
            => new HorFaceExclusivePrimitive(gp.Railing);


        public CornerFaceExclusivePrimitive RailingPillar()
            => new CornerFaceExclusivePrimitive(gp.RailingPillar);

        public BeamPrimitive Beam()
            => new BeamPrimitive(gp.BeamBottom, gp.BeamMiddle, gp.BeamTop);


        public FloorPrimitive Floor()
            => new FloorPrimitive(gp.CobblestoneFloor, gp.CobblestoneFloor);// todo: make floor one sided and add ceiling


        public CubeExclusivePrimitive StairPrimitive()
            => new CubeExclusivePrimitive(gp.Stairs);


        public GridPrimitivesStyle DefaultStyle() => new GridPrimitivesStyle()
        {
            Door = Door(),
            Wall = HouseWall(),
            Railing = Railing(),
            RailingPillar = RailingPillar(),
            Beam = Beam(),
            Floor = Floor(),
            Stairs = StairPrimitive(),
        };
    }

    public class GridPrimitivesStyle
    {
        public HorFacePrimitive Door { get; set; }
        public HorFacePrimitive Wall { get; set; }
        public HorFacePrimitive Railing { get; set; }
        public CornerFacePrimitive RailingPillar { get; set; }
        public CornerFacePrimitive Beam { get; set; }
        public VerFacePrimitive Floor { get; set; }
        public CubePrimitive Stairs { get; set; }
    }


}

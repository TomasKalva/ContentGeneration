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

        public NoFloorPrimitive NoFloor()
            => new NoFloorPrimitive();// todo: make floor one sided and add ceiling


        public CubeExclusivePrimitive StairPrimitive()
            => new CubeExclusivePrimitive(gp.Stairs);


        public GridPrimitivesStyle DefaultStyle() => new GridPrimitivesStyle()
        {
            Door = Door,
            Wall = HouseWall,
            Railing = Railing,
            RailingPillar = RailingPillar,
            Beam = Beam,
            Floor = Floor,
            NoFloor = NoFloor,
            Stairs = StairPrimitive,
        };
    }

    public class GridPrimitivesStyle
    {
        public Func<HorFacePrimitive> Door { get; set; }
        public Func<HorFacePrimitive> Wall { get; set; }
        public Func<HorFacePrimitive> Railing { get; set; }
        public Func<CornerFacePrimitive> RailingPillar { get; set; }
        public Func<CornerFacePrimitive> Beam { get; set; }
        public Func<VerFacePrimitive> Floor { get; set; }
        public Func<VerFacePrimitive> NoFloor { get; set; }
        public Func<CubePrimitive> Stairs { get; set; }
    }


}

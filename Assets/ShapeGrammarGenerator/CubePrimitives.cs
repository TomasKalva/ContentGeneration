using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator
{
    public class CubePrimitives
    {
        GeometricPrimitives gp;

        public CubePrimitives(GeometricPrimitives gp)
        {
            this.gp = gp;
        }

        public WallPrimitive HouseWall()
            => new WallPrimitive(gp.BrickWall, gp.BrickWall);// todo: replace two sided walls with only one sided ones

        public HorExclusivePrimitive Door()
            => new HorExclusivePrimitive(gp.WallDoor);//todo: replace with actual door primitive

        public HorExclusivePrimitive Railing()
            => new HorExclusivePrimitive(gp.Railing);


        public CornerExclusivePrimitive RailingPillar()
            => new CornerExclusivePrimitive(gp.RailingPillar);

        public BeamPrimitive Beam()
            => new BeamPrimitive(gp.BeamBottom, gp.BeamMiddle, gp.BeamTop);


        public FloorPrimitive Floor()
            => new FloorPrimitive(gp.CobblestoneFloor, gp.CobblestoneFloor);// todo: make floor one sided and add ceiling
    }

    
}

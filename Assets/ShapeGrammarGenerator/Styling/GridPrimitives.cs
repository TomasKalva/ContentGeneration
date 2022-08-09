using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
            => new WallPrimitive(gp.woodenWall, gp.brickWall);
        public WallPrimitive FoundationWall()
            => new WallPrimitive(gp.empty, gp.stoneWall);
        public CladdedWallPrimitive CladdedWall()
            => new CladdedWallPrimitive(gp.woodenWall, gp.brickWall, gp.cladding);

        public HorFaceExclusivePrimitive Door()
            => new HorFaceExclusivePrimitive(gp.wallDoor, FACE_HOR.Door, 3);//todo: replace with actual door primitive
        public HorFaceExclusivePrimitive RailingDoor()
            => new HorFaceExclusivePrimitive(gp.railingDoor, FACE_HOR.Door, 3);

        public HorFaceExclusivePrimitive Railing()
            => new HorFaceExclusivePrimitive(gp.railing, FACE_HOR.Railing, 1);
        public HorFaceExclusivePrimitive Cladding()
            => new HorFaceExclusivePrimitive(gp.cladding, FACE_HOR.Railing, 0.5f);

        public NoWallPrimitive NoWall()
            => new NoWallPrimitive();


        public CornerFaceExclusivePrimitive RailingPillar()
            => new CornerFaceExclusivePrimitive(gp.railingPillar, CORNER.RailingPillar);

        public BeamPrimitive Beam()
            => new BeamPrimitive(gp.beamBottom, gp.beamMiddle, gp.beamTop);


        public FloorPrimitive Floor()
            => new FloorPrimitive(gp.oneSidedFloor, gp.oneSidedCeiling);

        public FloorPrimitive PathFullFloor()
            => new FloorPrimitive(gp.woodenFullFloor, gp.empty);

        public NoFloorPrimitive NoFloor()
            => new NoFloorPrimitive();// todo: make floor one sided and add ceiling


        public CubeExclusivePrimitive StairPrimitive(Vector3Int direction)
            => new CubeExclusivePrimitive(gp.stairs, direction);

        public GridPrimitivesStyle DefaultStyle() => new GridPrimitivesStyle()
        {
            Door = Door,
            RailingDoor = RailingDoor,

            Wall = HouseWall,
            FoundationWall = FoundationWall,
            CladdedWall = CladdedWall,
            NoWall = NoWall,

            Railing = Railing,
            Cladding = Cladding,

            RailingPillar = RailingPillar,
            Beam = Beam,

            Floor = Floor,
            PathFullFloor = PathFullFloor,
            NoFloor = NoFloor,

            Stairs = StairPrimitive,

            DirectionalRoof = () => gp.oneDirectionRoof,
            CrossRoof = () => gp.crossRoof,
            PointyRoof = () => gp.pointyRoof,
            GableRoof = () => gp.gableRoof,

            BridgeTop = () => gp.bridgeTop,
            BridgeBottom = () => gp.bridgeBottom,
        };
    }

    /// <summary>
    /// Factories for the primitives, because each primitive contains a flag if it was resolved already.
    /// </summary>
    public class GridPrimitivesStyle
    {
        public Func<HorFacePrimitive> Door { get; set; }
        public Func<HorFacePrimitive> RailingDoor { get; set; }
        public Func<HorFacePrimitive> Wall { get; set; }
        public Func<HorFacePrimitive> FoundationWall { get; set; }
        public Func<HorFacePrimitive> CladdedWall { get; set; }
        public Func<HorFacePrimitive> Railing { get; set; }
        public Func<HorFacePrimitive> Cladding { get; set; }
        public Func<HorFacePrimitive> NoWall { get; set; }
        public Func<CornerFacetPrimitive> RailingPillar { get; set; }
        public Func<CornerFacetPrimitive> Beam { get; set; }
        public Func<VerFacePrimitive> Floor { get; set; }
        public Func<VerFacePrimitive> PathFullFloor { get; set; }
        public Func<VerFacePrimitive> NoFloor { get; set; }
        public Func<Vector3Int, CubePrimitive> Stairs { get; set; }
        public Func<GeometricPrimitive> DirectionalRoof { get; set; }
        public Func<GeometricPrimitive> CrossRoof { get; set; }
        public Func<GeometricPrimitive> PointyRoof { get; set; }
        public Func<GeometricPrimitive> GableRoof { get; set; }
        public Func<GeometricPrimitive> BridgeTop { get; set; }
        public Func<GeometricPrimitive> BridgeBottom { get; set; }
    }


}

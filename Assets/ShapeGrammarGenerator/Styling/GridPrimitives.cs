using ShapeGrammar;
using System;
using UnityEngine;

namespace Assets.ShapeGrammarGenerator
{
    public class GridPrimitives
    {
        public GeometricPrimitives GP { get; }

        public GridPrimitives(GeometricPrimitives gp)
        {
            this.GP = gp;
        }

        public WallPrimitive HouseWall()
            => new WallPrimitive(GP.tiledWall, GP.brickWall);
        public WallPrimitive Wall(GeometricPrimitive insideWall, GeometricPrimitive outsideWall)
            => new WallPrimitive(insideWall, outsideWall);
        public WallPrimitive FoundationWall(GeometricPrimitive wall)
            => new WallPrimitive(GP.empty, wall);
        public CladdedWallPrimitive CladdedWall(GeometricPrimitive insideWall, GeometricPrimitive outsideWall)
            => new CladdedWallPrimitive(insideWall, outsideWall, GP.cladding);
        public CladdedWallPrimitive CladdedWall()
            => new CladdedWallPrimitive(GP.tiledWall, GP.brickWall, GP.cladding);

        public HorFaceExclusivePrimitive Door()
            => new HorFaceExclusivePrimitive(GP.wallDoor, FACE_HOR.Door, 3);
        public HorFaceExclusivePrimitive RailingDoor()
            => new HorFaceExclusivePrimitive(GP.railingDoor, FACE_HOR.Door, 3);

        public HorFaceExclusivePrimitive Railing()
            => new HorFaceExclusivePrimitive(GP.railing, FACE_HOR.Railing, 1);
        public HorFaceExclusivePrimitive Cladding()
            => new HorFaceExclusivePrimitive(GP.cladding, FACE_HOR.Railing, 0.5f);

        public HorFaceExclusivePrimitive WallHole()
            => new HorFaceExclusivePrimitive(GP.wallHole, FACE_HOR.Door, 3);

        public NoWallPrimitive NoWall()
            => new NoWallPrimitive();


        public CornerFaceExclusivePrimitive RailingPillar()
            => new CornerFaceExclusivePrimitive(GP.railingPillar, CORNER.RailingPillar, 1f);

        public BeamPrimitive Beam()
            => new BeamPrimitive(GP.beamBottom, GP.beamMiddle, GP.beamTop, GP.beamBottomTop);

        public CornerFaceExclusivePrimitive NoPillar()
            => new CornerFaceExclusivePrimitive(GP.empty, CORNER.Nothing, 3f);


        public FloorPrimitive Floor(GeometricPrimitive floor, GeometricPrimitive ceiling)
            => new FloorPrimitive(floor, ceiling);
        public FloorPrimitive Floor()
            => new FloorPrimitive(GP.stoneTiledFloor, GP.oneSidedCeiling);

        public FloorPrimitive PathFullFloor()
            => new FloorPrimitive(GP.woodenFullFloor, GP.empty);

        public NoFloorPrimitive NoFloor()
            => new NoFloorPrimitive();// todo: make floor one sided and add ceiling


        public CubeExclusivePrimitive StairPrimitive(Vector3Int direction)
            => new CubeExclusivePrimitive(GP.stairs, direction);

        public GridPrimitivesStyle TownStyle() => new GridPrimitivesStyle()
        {
            Door = Door,
            RailingDoor = RailingDoor,

            Wall = HouseWall,
            FoundationWall = () => FoundationWall(GP.stoneWall),
            CladdedWall = CladdedWall,
            WallHole = WallHole,
            NoWall = NoWall,

            Railing = Railing,
            Cladding = Cladding,

            RailingPillar = RailingPillar,
            Beam = Beam,
            NoPillar = NoPillar,

            Floor = () => Floor(GP.evenTiledFloor, GP.oneSidedCeiling),
            PathFullFloor = PathFullFloor,
            NoFloor = NoFloor,

            Stairs = StairPrimitive,

            DirectionalRoof = GP.oneDirectionRoof,
            CrossRoof = GP.crossRoof,
            PointyRoof = GP.pointyRoof,
            GableRoof = GP.gableRoof,

            BridgeTop = GP.bridgeTop,
            BridgeBottom = GP.bridgeBottom,
        };

        public GridPrimitivesStyle YardStyle() => TownStyle()
            .SetFloor(() => Floor(GP.cobblestoneFloor, GP.oneSidedCeiling));

        public GridPrimitivesStyle ChapelStyle() => TownStyle()
            .SetFloor(() => Floor(GP.ornamentedFloor, GP.oneSidedCeiling))
            .SetWall(() => Wall(GP.pipedWall, GP.bigBrickWall))
            .SetCladdedWall(() => CladdedWall(GP.pipedWall, GP.bigBrickWall))
            .SetPointyRoof(GP.curvedPointyRoof)
            .SetGableRoof(GP.curvedGableRoof)
            .SetDirectionalRoof(GP.curvedOneDirectionRoof)
            .SetCrossRoof(GP.curvedCrossRoof);

        public GridPrimitivesStyle GardenStyle() => TownStyle()
            .SetFoundationWall(() => FoundationWall(GP.barkWall))
            .SetFloor(() => Floor(GP.grassFloor, GP.oneSidedCeiling));

        public GridPrimitivesStyle CastleStyle() => TownStyle()
            .SetFloor(() => Floor(GP.marbleFloor, GP.oneSidedCeiling))
            .SetWall(() => Wall(GP.cementedWall, GP.cementedWall))
            .SetCladdedWall(() => CladdedWall(GP.cementedWall, GP.cementedWall));
    }

    /// <summary>
    /// Factories for the primitives, because each primitive contains a flag if it was resolved already.
    /// </summary>
    public class GridPrimitivesStyle
    {
        public Func<HorFacePrimitive> Door { get; set; }
        public Func<HorFacePrimitive> RailingDoor { get; set; }
        public Func<HorFacePrimitive> Wall { get; set; }
        public GridPrimitivesStyle SetWall(Func<HorFacePrimitive> wall)
        {
            Wall = wall;
            return this;
        }

        public Func<HorFacePrimitive> FoundationWall { get; set; }
        public GridPrimitivesStyle SetFoundationWall(Func<HorFacePrimitive> foundationWall)
        {
            FoundationWall = foundationWall;
            return this;
        }

        public Func<HorFacePrimitive> CladdedWall { get; set; }
        public GridPrimitivesStyle SetCladdedWall(Func<HorFacePrimitive> claddedWall)
        {
            CladdedWall = claddedWall;
            return this;
        }
        public Func<HorFacePrimitive> Railing { get; set; }
        public Func<HorFacePrimitive> Cladding { get; set; }
        public Func<HorFaceExclusivePrimitive> WallHole { get; set; }
        public Func<HorFacePrimitive> NoWall { get; set; }
        public Func<CornerFacetPrimitive> RailingPillar { get; set; }
        public Func<CornerFacetPrimitive> Beam { get; set; }
        public Func<CornerFacetPrimitive> NoPillar { get; set; }
        public Func<VerFacePrimitive> Floor { get; set; }
        public GridPrimitivesStyle SetFloor(Func<VerFacePrimitive> floor)
        {
            Floor = floor;
            return this;
        }

        public Func<VerFacePrimitive> PathFullFloor { get; set; }
        public Func<VerFacePrimitive> NoFloor { get; set; }
        public Func<Vector3Int, CubePrimitive> Stairs { get; set; }
        public GeometricPrimitive DirectionalRoof { get; set; }
        public GridPrimitivesStyle SetDirectionalRoof(GeometricPrimitive roof)
        {
            DirectionalRoof = roof;
            return this;
        }
        public GeometricPrimitive CrossRoof { get; set; }
        public GridPrimitivesStyle SetCrossRoof(GeometricPrimitive roof)
        {
            CrossRoof = roof;
            return this;
        }
        public GeometricPrimitive PointyRoof { get; set; }
        public GridPrimitivesStyle SetPointyRoof(GeometricPrimitive roof)
        {
            PointyRoof = roof;
            return this;
        }
        public GeometricPrimitive GableRoof { get; set; }
        public GridPrimitivesStyle SetGableRoof(GeometricPrimitive roof)
        {
            GableRoof = roof;
            return this;
        }
        public GeometricPrimitive BridgeTop { get; set; }
        public GridPrimitivesStyle SetBridgeTop(GeometricPrimitive bridgeTop)
        {
            BridgeTop = bridgeTop;
            return this;
        }
        public GeometricPrimitive BridgeBottom { get; set; }
        public GridPrimitivesStyle SetBridgeBottom(GeometricPrimitive bridgeBottom)
        {
            BridgeBottom = bridgeBottom;
            return this;
        }
    }


}

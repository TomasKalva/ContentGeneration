using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator
{
    public abstract class CubePrimitive
    {
        public int Priority { get; }
    }

    public interface ICubePrimitivePlacer<CubePrimitiveT> where CubePrimitiveT : CubePrimitive
    {
        public abstract void PlacePrimitive(CubePrimitiveT otherPrimitive);
    }
    #region Horizontal primitives
    public abstract class HorCubePrimitive : CubePrimitive, ICubePrimitivePlacer<HorCubePrimitive>
    {
        protected FACE_HOR FaceType { get; }

        public abstract void PlacePrimitive(HorCubePrimitive otherPrimitive);
    }

    public class WallPrimitive : HorCubePrimitive
    {
        GeometricPrimitive InsideWall { get; }
        GeometricPrimitive OutsideWall { get; }

        public WallPrimitive(GeometricPrimitive insideWall, GeometricPrimitive outsideWall)
        {
            InsideWall = insideWall;
            OutsideWall = outsideWall;
        }

        public override void PlacePrimitive(HorCubePrimitive otherPrimitive)
        {
            throw new NotImplementedException();
        }
    }

    public class HorExclusivePrimitive : HorCubePrimitive
    {
        GeometricPrimitive Face { get; }

        public HorExclusivePrimitive(GeometricPrimitive face)
        {
            Face = face;
        }

        public override void PlacePrimitive(HorCubePrimitive otherPrimitive)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Vertical cube primitives
    public abstract class VerCubePrimitive : CubePrimitive, ICubePrimitivePlacer<VerCubePrimitive>
    {
        protected FACE_VER FaceType { get; }

        public abstract void PlacePrimitive(VerCubePrimitive otherPrimitive);
    }

    public class FloorPrimitive : VerCubePrimitive
    {
        GeometricPrimitive Floor { get; }
        GeometricPrimitive Ceiling { get; }

        public FloorPrimitive(GeometricPrimitive floor, GeometricPrimitive ceiling)
        {
            Floor = floor;
            Ceiling = ceiling;
        }

        public override void PlacePrimitive(VerCubePrimitive otherPrimitive)
        {
            throw new NotImplementedException();
        }
    }

    public class BeamPrimitive : VerCubePrimitive
    {
        GeometricPrimitive Bottom { get; }
        GeometricPrimitive Middle { get; }
        GeometricPrimitive Top { get; }

        public BeamPrimitive(GeometricPrimitive bottom, GeometricPrimitive middle, GeometricPrimitive top)
        {
            Bottom = bottom;
            Middle = middle;
            Top = top;
        }

        public override void PlacePrimitive(VerCubePrimitive otherPrimitive)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Corner cube primitives
    public abstract class CornerCubePrimitive : CubePrimitive, ICubePrimitivePlacer<CornerCubePrimitive>
    {
        protected CORNER FaceType { get; }

        public abstract void PlacePrimitive(CornerCubePrimitive otherPrimitive);
    }

    public class CornerExclusivePrimitive : CornerCubePrimitive
    {
        GeometricPrimitive Face { get; }

        public CornerExclusivePrimitive(GeometricPrimitive face)
        {
            Face = face;
        }

        public override void PlacePrimitive(CornerCubePrimitive otherPrimitive)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}

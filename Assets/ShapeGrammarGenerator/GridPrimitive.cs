using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator
{
    public abstract class GridPrimitive
    {
        public int Priority { get; }
    }

    public interface ICubePrimitivePlacer<CubePrimitiveT> where CubePrimitiveT : GridPrimitive
    {
        public abstract void PlacePrimitive(CubePrimitiveT otherPrimitive);
    }
    #region Horizontal primitives
    public abstract class HorFacePrimitive : GridPrimitive, ICubePrimitivePlacer<HorFacePrimitive>
    {
        protected FACE_HOR FaceType { get; }

        public abstract void PlacePrimitive(HorFacePrimitive otherPrimitive);
    }

    public class WallPrimitive : HorFacePrimitive
    {
        GeometricPrimitive InsideWall { get; }
        GeometricPrimitive OutsideWall { get; }

        public WallPrimitive(GeometricPrimitive insideWall, GeometricPrimitive outsideWall)
        {
            InsideWall = insideWall;
            OutsideWall = outsideWall;
        }

        public override void PlacePrimitive(HorFacePrimitive otherPrimitive)
        {
            throw new NotImplementedException();
        }
    }

    public class HorFaceExclusivePrimitive : HorFacePrimitive
    {
        GeometricPrimitive Face { get; }

        public HorFaceExclusivePrimitive(GeometricPrimitive face)
        {
            Face = face;
        }

        public override void PlacePrimitive(HorFacePrimitive otherPrimitive)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Vertical cube primitives
    public abstract class VerFacePrimitive : GridPrimitive, ICubePrimitivePlacer<VerFacePrimitive>
    {
        protected FACE_VER FaceType { get; }

        public abstract void PlacePrimitive(VerFacePrimitive otherPrimitive);
    }

    public class FloorPrimitive : VerFacePrimitive
    {
        GeometricPrimitive Floor { get; }
        GeometricPrimitive Ceiling { get; }

        public FloorPrimitive(GeometricPrimitive floor, GeometricPrimitive ceiling)
        {
            Floor = floor;
            Ceiling = ceiling;
        }

        public override void PlacePrimitive(VerFacePrimitive otherPrimitive)
        {
            throw new NotImplementedException();
        }
    }

    public class BeamPrimitive : VerFacePrimitive
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

        public override void PlacePrimitive(VerFacePrimitive otherPrimitive)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Corner primitives
    public abstract class CornerFacePrimitive : GridPrimitive, ICubePrimitivePlacer<CornerFacePrimitive>
    {
        protected CORNER FaceType { get; }

        public abstract void PlacePrimitive(CornerFacePrimitive otherPrimitive);
    }

    public class CornerFaceExclusivePrimitive : CornerFacePrimitive
    {
        GeometricPrimitive Face { get; }

        public CornerFaceExclusivePrimitive(GeometricPrimitive face)
        {
            Face = face;
        }

        public override void PlacePrimitive(CornerFacePrimitive otherPrimitive)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Cube primitives
    public abstract class CubePrimitive : GridPrimitive, ICubePrimitivePlacer<CubePrimitive>
    {
        protected CORNER FaceType { get; }

        public abstract void PlacePrimitive(CubePrimitive otherPrimitive);
    }

    public class CubeExclusivePrimitive : CubePrimitive
    {
        GeometricPrimitive Object { get; }

        public CubeExclusivePrimitive(GeometricPrimitive obj)
        {
            Object = obj;
        }

        public override void PlacePrimitive(CubePrimitive otherPrimitive)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

}

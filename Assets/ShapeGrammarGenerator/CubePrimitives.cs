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
    }

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

    public class VerExclusivePrimitive : VerCubePrimitive
    {
        GeometricPrimitive Face { get; }

        public VerExclusivePrimitive(GeometricPrimitive face)
        {
            Face = face;
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

    public class CornerExclusiveCubePrimitive : CornerCubePrimitive
    {
        GeometricPrimitive Face { get; }

        public CornerExclusiveCubePrimitive(GeometricPrimitive face)
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

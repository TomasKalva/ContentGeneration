using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.ShapeGrammarGenerator
{
    public abstract class GridPrimitive
    {
        public int Priority { get; protected set; }
        /// <summary>
        /// True if the facet of this primitive was already resolved.
        /// </summary>
        public bool Resolved { get; set; } = false;
    }

    public interface ICubePrimitivePlacer<CubePrimitiveT> where CubePrimitiveT : GridPrimitive
    {
        public abstract void PlacePrimitive(CubePrimitiveT otherPrimitive);
    }
    #region Horizontal primitives
    public abstract class HorFacePrimitive : GridPrimitive, ICubePrimitivePlacer<HorFacePrimitive>
    {
        public FACE_HOR FaceType { get; }

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
    public class VerFacePrimitive : GridPrimitive, ICubePrimitivePlacer<VerFacePrimitive>
    {
        public FACE_VER FaceType { get; protected set; }

        public VerFacePrimitive()
        {
            FaceType = FACE_VER.Nothing;
            Priority = 0;
        }

        public virtual void PlacePrimitive(VerFacePrimitive otherPrimitive) { }
    }

    public class FloorPrimitive : VerFacePrimitive
    {
        GeometricPrimitive Floor { get; }
        GeometricPrimitive Ceiling { get; }

        public FloorPrimitive(GeometricPrimitive floor, GeometricPrimitive ceiling)
        {
            Floor = floor;
            Ceiling = ceiling;
            FaceType = FACE_VER.Floor;
            Priority = 1;
        }

        public override void PlacePrimitive(VerFacePrimitive otherPrimitive)
        {
            /*
            var offset = Vector3.up * Math.Max(0, Direction.y);
            var obj = Floor.transform;

            obj.localScale = scale * Vector3.one;
            obj.localPosition = (cubePosition + offset) * scale;
            */
        }
    }

    public class NoFloorPrimitive : VerFacePrimitive
    {
        public NoFloorPrimitive()
        {
            FaceType = FACE_VER.Nothing;
            Priority = 2;
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

    public class BeamPrimitive : CornerFacePrimitive
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

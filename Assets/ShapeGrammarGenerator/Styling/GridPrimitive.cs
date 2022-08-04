using ContentGeneration.Assets.UI;
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

    public interface IGridPrimitivePlacer<GridPrimitiveT> where GridPrimitiveT : GridPrimitive 
    {
        public abstract void PlacePrimitive(IGridGeometryOwner worldGeometry, Facet facet, GridPrimitiveT otherPrimitive);
    }
    #region Horizontal primitives
    public class HorFacePrimitive : GridPrimitive, IGridPrimitivePlacer<HorFacePrimitive>
    {
        public HorFacePrimitive()
        {
            FaceType = FACE_HOR.Nothing;
            Priority = 0;
        }

        public FACE_HOR FaceType { get; protected set; }

        public virtual void PlacePrimitive(IGridGeometryOwner worldGeometry, Facet facet, HorFacePrimitive otherPrimitive) { }
    }

    public class WallPrimitive : HorFacePrimitive
    {
        GeometricPrimitive InsideWall { get; }
        GeometricPrimitive OutsideWall { get; }

        public WallPrimitive(GeometricPrimitive insideWall, GeometricPrimitive outsideWall)
        {
            InsideWall = insideWall;
            OutsideWall = outsideWall;
            FaceType = FACE_HOR.Wall;
            Priority = 2;
        }

        public override void PlacePrimitive(IGridGeometryOwner geometryOwner, Facet facet, HorFacePrimitive otherPrimitive)
        {
            //todo: replace with the correct implementation
            var cubePosition = facet.MyCube.Position;
            var scale = geometryOwner.WorldGeometry.WorldScale;
            var offset = (Vector3)facet.Direction * 0.5f;
            var obj = InsideWall.New().transform;

            obj.localScale = scale * Vector3.one;
            obj.localPosition = (cubePosition + offset) * scale;
            obj.rotation = Quaternion.LookRotation(facet.Direction, Vector3.up);

            geometryOwner.AddArchitectureElement(obj);
            facet.OnObjectCreated(obj);
        }
    }

    public class HorFaceExclusivePrimitive : HorFacePrimitive
    {
        GeometricPrimitive Face { get; }

        public HorFaceExclusivePrimitive(GeometricPrimitive face, FACE_HOR faceType, int priority)
        {
            Face = face;
            Priority = priority;
            FaceType = faceType;
        }

        public override void PlacePrimitive(IGridGeometryOwner geometryOwner, Facet facet, HorFacePrimitive otherPrimitive)
        {
            var cubePosition = facet.MyCube.Position;
            var scale = geometryOwner.WorldGeometry.WorldScale;
            var offset = (Vector3)facet.Direction * 0.5f;
            var obj = Face.New().transform;

            obj.localScale = scale * Vector3.one;
            obj.localPosition = (cubePosition + offset) * scale;
            obj.rotation = Quaternion.LookRotation(facet.Direction, Vector3.up);

            geometryOwner.AddArchitectureElement(obj);
            facet.OnObjectCreated(obj);
        }
    }

    public class NoWallPrimitive : HorFacePrimitive
    {
        public NoWallPrimitive()
        {
            FaceType = FACE_HOR.Nothing;
            Priority = 4;
        }

        public override void PlacePrimitive(IGridGeometryOwner worldGeometry, Facet facet, HorFacePrimitive otherPrimitive)
        {
            // No object to create => don't do anything
        }
    }
    #endregion

    #region Vertical cube primitives
    public class VerFacePrimitive : GridPrimitive, IGridPrimitivePlacer<VerFacePrimitive>
    {
        public FACE_VER FaceType { get; protected set; }

        public VerFacePrimitive()
        {
            FaceType = FACE_VER.Nothing;
            Priority = 0;
        }

        public virtual void PlacePrimitive(IGridGeometryOwner geometryOwner, Facet facet, VerFacePrimitive otherPrimitive) { }
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

        public override void PlacePrimitive(IGridGeometryOwner geometryOwner, Facet face, VerFacePrimitive otherPrimitive)
        {
            var cubePosition = face.MyCube.Position;
            var scale = geometryOwner.WorldGeometry.WorldScale;
            var offset = Vector3.up * Math.Max(0, face.Direction.y);

            var obj = Floor.New().transform;
            obj.localScale = scale * Vector3.one;
            obj.localPosition = (cubePosition + offset) * scale; 

            geometryOwner.AddArchitectureElement(obj);
            face.OnObjectCreated(obj);
        }
    }

    public class NoFloorPrimitive : VerFacePrimitive
    {
        public NoFloorPrimitive()
        {
            FaceType = FACE_VER.Nothing;
            Priority = 2;
        }

        public override void PlacePrimitive(IGridGeometryOwner worldGeometry, Facet facet, VerFacePrimitive otherPrimitive)
        {
            // No object to create => don't do anything
        }
    }
    #endregion

    #region Corner primitives
    public abstract class CornerFacePrimitive : GridPrimitive, IGridPrimitivePlacer<CornerFacePrimitive>
    {
        protected CORNER FaceType { get; }

        public abstract void PlacePrimitive(IGridGeometryOwner worldGeometry, Facet facet, CornerFacePrimitive otherPrimitive);
    }

    public class CornerFaceExclusivePrimitive : CornerFacePrimitive
    {
        GeometricPrimitive Face { get; }

        public CornerFaceExclusivePrimitive(GeometricPrimitive face)
        {
            Face = face;
        }

        public override void PlacePrimitive(IGridGeometryOwner worldGeometry, Facet facet, CornerFacePrimitive otherPrimitive)
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

        public override void PlacePrimitive(IGridGeometryOwner worldGeometry, Facet facet, CornerFacePrimitive otherPrimitive)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Cube primitives
    public abstract class CubePrimitive : GridPrimitive, IGridPrimitivePlacer<CubePrimitive>
    {
        protected CORNER FaceType { get; }

        public abstract void PlacePrimitive(IGridGeometryOwner worldGeometry, Facet facet, CubePrimitive otherPrimitive);
    }

    public class CubeExclusivePrimitive : CubePrimitive
    {
        GeometricPrimitive Object { get; }

        public CubeExclusivePrimitive(GeometricPrimitive obj)
        {
            Object = obj;
        }

        public override void PlacePrimitive(IGridGeometryOwner worldGeometry, Facet facet, CubePrimitive otherPrimitive)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

}

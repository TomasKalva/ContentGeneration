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
        public float Priority { get; protected set; }
        /// <summary>
        /// True if the facet of this primitive was already resolved.
        /// </summary>
        public bool Resolved { get; set; } = false;
    }

    public interface IGridPrimitivePlacer<GridPrimitiveT> where GridPrimitiveT : GridPrimitive 
    {
        public abstract void PlacePrimitive(IGridGeometryOwner worldGeometry, IFacet facet, GridPrimitiveT otherPrimitive);
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

        public virtual void PlacePrimitive(IGridGeometryOwner worldGeometry, IFacet facet, HorFacePrimitive otherPrimitive) { }

        protected void PlaceHorizontally(IGridGeometryOwner geometryOwner, IFacet facet, GeometricPrimitive toPlace, Vector3Int direction)
        {
            var cubePosition = facet.MyCube.Position;
            var scale = geometryOwner.WorldGeometry.WorldScale;
            var offset = (Vector3)facet.Direction * 0.5f;
            var obj = toPlace.New().transform;

            obj.localScale = scale * Vector3.one;
            obj.localPosition = (cubePosition + offset) * scale;
            obj.rotation = Quaternion.LookRotation(direction, Vector3.up);

            geometryOwner.AddArchitectureElement(obj);
            facet.OnObjectCreated(obj);
        }
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

        public override void PlacePrimitive(IGridGeometryOwner geometryOwner, IFacet facet, HorFacePrimitive otherPrimitive)
        {
            if(otherPrimitive is WallPrimitive otherWallPrimitive)
            {
                //todo: replace with the correct implementation
                PlaceHorizontally(geometryOwner, facet, InsideWall, -facet.Direction);
                PlaceHorizontally(geometryOwner, facet, otherWallPrimitive.InsideWall, facet.Direction);
            }
            else
            {
                PlaceHorizontally(geometryOwner, facet, InsideWall, -facet.Direction);
                PlaceHorizontally(geometryOwner, facet, OutsideWall, facet.Direction);
            }
        }
    }

    public class CladdedWallPrimitive : WallPrimitive
    {
        GeometricPrimitive Cladding { get; }

        public CladdedWallPrimitive(GeometricPrimitive insideWall, GeometricPrimitive outsideWall, GeometricPrimitive cladding) : base(insideWall, outsideWall)
        {
            Cladding = cladding;
            Priority = 1.5f;
        }

        public override void PlacePrimitive(IGridGeometryOwner geometryOwner, IFacet facet, HorFacePrimitive otherPrimitive)
        {
            base.PlacePrimitive(geometryOwner, facet, otherPrimitive);
            PlaceHorizontally(geometryOwner, facet, Cladding, facet.Direction);
        }
    }

    public class HorFaceExclusivePrimitive : HorFacePrimitive
    {
        GeometricPrimitive Face { get; }

        public HorFaceExclusivePrimitive(GeometricPrimitive face, FACE_HOR faceType, float priority)
        {
            Face = face;
            Priority = priority;
            FaceType = faceType;
        }

        public override void PlacePrimitive(IGridGeometryOwner geometryOwner, IFacet facet, HorFacePrimitive otherPrimitive)
        {
            PlaceHorizontally(geometryOwner, facet, Face, facet.Direction);
        }
    }

    public class NoWallPrimitive : HorFacePrimitive
    {
        public NoWallPrimitive()
        {
            FaceType = FACE_HOR.Nothing;
            Priority = 4;
        }

        public override void PlacePrimitive(IGridGeometryOwner worldGeometry, IFacet facet, HorFacePrimitive otherPrimitive)
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

        public virtual void PlacePrimitive(IGridGeometryOwner geometryOwner, IFacet facet, VerFacePrimitive otherPrimitive) { }

        protected void PlaceVertically(IGridGeometryOwner geometryOwner, IFacet facet, GeometricPrimitive toPlace)
        {
            var cubePosition = facet.MyCube.Position;
            var scale = geometryOwner.WorldGeometry.WorldScale;
            var offset = Vector3.up * Math.Max(0, facet.Direction.y);

            var obj = toPlace.New().transform;
            obj.localScale = scale * Vector3.one;
            obj.localPosition = (cubePosition + offset) * scale;

            geometryOwner.AddArchitectureElement(obj);
            facet.OnObjectCreated(obj);
        }
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

        public override void PlacePrimitive(IGridGeometryOwner geometryOwner, IFacet face, VerFacePrimitive otherPrimitive)
        {
            PlaceVertically(geometryOwner, face, Floor);
            PlaceVertically(geometryOwner, face, Ceiling);
        }
    }

    public class NoFloorPrimitive : VerFacePrimitive
    {
        public NoFloorPrimitive()
        {
            FaceType = FACE_VER.Nothing;
            Priority = 2;
        }

        public override void PlacePrimitive(IGridGeometryOwner worldGeometry, IFacet facet, VerFacePrimitive otherPrimitive)
        {
            // No object to create => don't do anything
        }
    }
    #endregion

    #region Corner primitives
    public class CornerFacetPrimitive : GridPrimitive, IGridPrimitivePlacer<CornerFacetPrimitive>
    {
        public CORNER CornerType { get; protected set; }

        public CornerFacetPrimitive()
        {
            CornerType = CORNER.Nothing;
            Priority = 0;
        }

        public virtual void PlacePrimitive(IGridGeometryOwner worldGeometry, IFacet facet, CornerFacetPrimitive otherPrimitive) { }

        protected void PlaceCorner(IGridGeometryOwner geometryOwner, IFacet facet, GeometricPrimitive toPlace)
        {
            var cubePosition = facet.MyCube.Position;
            var scale = geometryOwner.WorldGeometry.WorldScale;
            var offset = (Vector3)facet.Direction * 0.5f;

            var obj = toPlace.New().transform;
            obj.localScale = scale * Vector3.one;
            obj.localPosition = (cubePosition + offset) * scale;

            geometryOwner.AddArchitectureElement(obj);
            facet.OnObjectCreated(obj);
        }
    }

    public class CornerFaceExclusivePrimitive : CornerFacetPrimitive
    {
        GeometricPrimitive Corner { get; }

        public CornerFaceExclusivePrimitive(GeometricPrimitive face, CORNER cornerType, float priority)
        {
            Priority = priority;
            Corner = face;
            CornerType = cornerType;
        }

        public override void PlacePrimitive(IGridGeometryOwner worldGeometry, IFacet facet, CornerFacetPrimitive otherPrimitive)
        {
            PlaceCorner(worldGeometry, facet, Corner);
        }
    }

    public class BeamPrimitive : CornerFacetPrimitive
    {
        GeometricPrimitive Bottom { get; }
        GeometricPrimitive Middle { get; }
        GeometricPrimitive Top { get; }

        public BeamPrimitive(GeometricPrimitive bottom, GeometricPrimitive middle, GeometricPrimitive top)
        {
            Priority = 2;
            Bottom = bottom;
            Middle = middle;
            Top = top;
            CornerType = CORNER.Beam;
        }

        public override void PlacePrimitive(IGridGeometryOwner worldGeometry, IFacet facet, CornerFacetPrimitive otherPrimitive)
        {
            var cornerDirection = facet.Direction;
            var below = facet.MyCube.NeighborsDirections(Vector3Int.down.ToEnumerable()).Select(cube => cube.Corners(cornerDirection)).First().CornerType;
            var above = facet.MyCube.NeighborsDirections(Vector3Int.up.ToEnumerable()).Select(cube => cube.Corners(cornerDirection)).First().CornerType;
            //todo: rewrite ugly branching
            if(below == CORNER.Beam)
            {
                if(above == CORNER.Beam)
                {
                    PlaceCorner(worldGeometry, facet, Middle);
                }
                else
                {
                    PlaceCorner(worldGeometry, facet, Top);
                }
            }
            else
            {
                if (above == CORNER.Beam)
                {
                    PlaceCorner(worldGeometry, facet, Bottom);
                }
                else
                {
                    PlaceCorner(worldGeometry, facet, Middle);
                }
            }
        }
    }
    #endregion

    #region Cube primitives
    public class CubePrimitive : GridPrimitive, IGridPrimitivePlacer<CubePrimitive>
    {
        public CUBE CubeType { get; }
        protected Vector3Int Direction { get; set; }

        public CubePrimitive()
        {
            CubeType = CUBE.Nothing;
        }

        public virtual void PlacePrimitive(IGridGeometryOwner worldGeometry, IFacet facet, CubePrimitive otherPrimitive) { }

        protected void PlaceCube(IGridGeometryOwner geometryOwner, IFacet cube, GeometricPrimitive toPlace, Vector3Int direction)
        {
            var cubePosition = cube.MyCube.Position;
            var scale = geometryOwner.WorldGeometry.WorldScale;
            var obj = toPlace.New().transform;

            obj.localScale = scale * Vector3.one;
            obj.localPosition = ((Vector3)cubePosition) * scale;
            obj.rotation = Quaternion.LookRotation(direction, Vector3.up);

            geometryOwner.AddArchitectureElement(obj);
        }
    }

    public class CubeExclusivePrimitive : CubePrimitive
    {
        GeometricPrimitive Object { get; }

        public CubeExclusivePrimitive(GeometricPrimitive obj, Vector3Int direction)
        {
            Object = obj;
            Direction = direction;
        }

        public override void PlacePrimitive(IGridGeometryOwner worldGeometry, IFacet facet, CubePrimitive otherPrimitive)
        {
            PlaceCube(worldGeometry, facet, Object, Direction);
        }
    }
    #endregion

}

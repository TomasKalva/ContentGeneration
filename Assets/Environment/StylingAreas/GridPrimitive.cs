using OurFramework.UI;
using OurFramework.Environment.StylingAreas;
using System;
using System.Linq;
using UnityEngine;
using OurFramework.Util;
using OurFramework.Game;

namespace OurFramework.Environment.GridMembers
{
    /// <summary>
    /// Primitive placed to grid by styles. Used for resolving conflicts.
    /// </summary>
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
        /// <summary>
        /// Places the primitive to the grid and tries to resolve conflicts with other primitives.
        /// </summary>
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

            obj.localScale = scale * obj.localScale;
            obj.localPosition = (cubePosition + offset) * scale;
            obj.rotation = Quaternion.LookRotation(direction, Vector3.up);

            geometryOwner.AddArchitectureElement(obj);
            facet.OnObjectCreated(obj);
        }
    }

    /// <summary>
    /// Places full wall or only inside part of the wall if the other part is already taken by another wall.
    /// </summary>
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

    /// <summary>
    /// Places wall with cladding.
    /// </summary>
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

    /// <summary>
    /// Primitive that can be placed only from one side - for example door.
    /// The user of this primitive has to make sure that it's not placed multiple times.
    /// </summary>
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

    /// <summary>
    /// Doesn't place any object.
    /// </summary>
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

    /// <summary>
    /// Places floor and the ceiling below.
    /// </summary>
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

    /// <summary>
    /// Doesn't place any object.
    /// </summary>
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
            obj.localScale = scale * obj.localScale;
            obj.localPosition = (cubePosition + offset) * scale;

            geometryOwner.AddArchitectureElement(obj);
            facet.OnObjectCreated(obj);
        }
    }

    /// <summary>
    /// Primitive that can be placed only from one side of the corner.
    /// The user of this primitive has to make sure that it's not placed multiple times.
    /// </summary>
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

    /// <summary>
    /// Beam made of bottom, middle and top part.
    /// </summary>
    public class BeamPrimitive : CornerFacetPrimitive
    {
        GeometricPrimitive Bottom { get; }
        GeometricPrimitive Middle { get; }
        GeometricPrimitive Top { get; }
        GeometricPrimitive BottomTop { get; }

        public BeamPrimitive(GeometricPrimitive bottom, GeometricPrimitive middle, GeometricPrimitive top, GeometricPrimitive bottomTop)
        {
            Priority = 2;
            Bottom = bottom;
            Middle = middle;
            BottomTop = bottomTop;
            Top = top;
            CornerType = CORNER.Beam;
        }

        public override void PlacePrimitive(IGridGeometryOwner worldGeometry, IFacet facet, CornerFacetPrimitive otherPrimitive)
        {
            var cornerDirection = facet.Direction;
            var below = facet.MyCube.NeighborsDirections(Vector3Int.down.ToEnumerable()).Select(cube => cube.Corners(cornerDirection)).First().CornerType;
            var above = facet.MyCube.NeighborsDirections(Vector3Int.up.ToEnumerable()).Select(cube => cube.Corners(cornerDirection)).First().CornerType;
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
                    PlaceCorner(worldGeometry, facet, BottomTop);
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

    /// <summary>
    /// Primitive that can be placed only once.
    /// The user of this primitive has to make sure that it's not placed multiple times.
    /// </summary>
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

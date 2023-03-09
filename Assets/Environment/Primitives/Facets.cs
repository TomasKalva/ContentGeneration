using ContentGeneration.Assets.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OurFramework.Environment.GridMembers
{

    public interface IFacet
    {
        Cube MyCube { get; }
        Vector3Int Direction { get; }
        Action<Transform> OnObjectCreated { get; }
    }

    public abstract class Facet : IFacet
    {

        public Vector3Int Direction { get; }
        public Cube MyCube { get; }
        public Cube OtherCube => MyCube.Grid[MyCube.Position + Direction];
        public Action<Transform> OnObjectCreated { get; set; } = _ => { };

        protected Facet(Cube myCube, Vector3Int direction)
        {
            Direction = direction;
            MyCube = myCube;
        }

        public abstract void CreateGeometry(float cubeSide, IGridGeometryOwner world);

        public FacetT MoveBy<FacetT>(Vector3Int offset) where FacetT : Facet
        {
            var offsetCube = MyCube.MoveBy(offset);
            return (FacetT)offsetCube.Facets[Direction];
        }

        public IEnumerable<FacetT> MoveInDirUntil<FacetT>(Grid<Cube> gridView, Vector3Int dir, Func<FacetT, bool> stopPred) where FacetT : Facet
        {
            var validFacets = MyCube.MoveInDirUntil(dir, cube => stopPred((FacetT)cube.Facets[Direction])).Select(cube => (FacetT)cube.Facets[Direction]);
            return validFacets;
        }
    }

    public class FaceHor : Facet
    {
        private HorFacePrimitive facePrimitive;

        public HorFacePrimitive FacePrimitive
        {
            get => facePrimitive;
            set
            {
                facePrimitive = value;
                MyCube.Changed = true;
            }
        }

        public FACE_HOR FaceType => FacePrimitive.FaceType;

        public FaceHor(Cube myCube, Vector3Int direction) : base(myCube, direction)
        {
            FacePrimitive = new HorFacePrimitive();
        }

        public override void CreateGeometry(float scale, IGridGeometryOwner world)
        {
            if (FacePrimitive.Resolved)
                return;

            var otherFace = OtherCube.FacesHor(-Direction);
            var primitives = new HorFacePrimitive[2] { FacePrimitive, otherFace.FacePrimitive };
            var winningPrimitive = primitives.ArgMax(p => p.Priority);
            // Place stuff only from view of winning primitive
            if (winningPrimitive != FacePrimitive)
                return;

            var losingPrimitive = primitives.Others(winningPrimitive).First();
            winningPrimitive.PlacePrimitive(world, this, losingPrimitive);

            primitives.ForEach(primitive => primitive.Resolved = true);
        }

        public IEnumerable<Corner> Corners()
        {
            var ortDir = ExtensionMethods.OrthogonalHorizontalDir(Direction);
            yield return MyCube.Corners(Direction + ortDir);
            yield return MyCube.Corners(Direction - ortDir);
        }

        public FaceHor MoveBy(Vector3Int offset) => MoveBy<FaceHor>(offset);
        public IEnumerable<FaceHor> MoveInDirUntil(Grid<Cube> gridView, Vector3Int dir, Func<FaceHor, bool> stopPred) => MoveInDirUntil<FaceHor>(gridView, dir, stopPred);
        public FaceHorGroup Group() => new FaceHorGroup(MyCube.Grid, new List<FaceHor>() { this });

        public FaceHor OtherFacet() => OtherCube.FacesHor(-Direction);
    }

    public class FaceVer : Facet
    {
        private VerFacePrimitive facePrimitive;

        public VerFacePrimitive FacePrimitive
        {
            get => facePrimitive;
            set
            {
                facePrimitive = value;
                MyCube.Changed = true;
            }
        }

        public FACE_VER FaceType => FacePrimitive.FaceType;

        public FaceVer(Cube myCube, Vector3Int direction) : base(myCube, direction)
        {
            FacePrimitive = new VerFacePrimitive();
        }

        public override void CreateGeometry(float _, IGridGeometryOwner world)
        {
            if (FacePrimitive.Resolved)
                return;

            var otherFace = OtherCube.FacesVer(-Direction);
            var primitives = new VerFacePrimitive[2] { FacePrimitive, otherFace.FacePrimitive };
            var winningPrimitive = primitives.ArgMax(p => p.Priority);
            // Place stuff only from view of winning primitive
            if (winningPrimitive != FacePrimitive)
                return;

            var losingPrimitive = primitives.Others(winningPrimitive).First();
            winningPrimitive.PlacePrimitive(world, this, losingPrimitive);

            primitives.ForEach(primitive => primitive.Resolved = true);
        }

        public FaceVer MoveBy(Vector3Int offset) => MoveBy<FaceVer>(offset);
        public IEnumerable<FaceVer> MoveInDirUntil(Grid<Cube> gridView, Vector3Int dir, Func<FaceVer, bool> stopPred) => MoveInDirUntil<FaceVer>(gridView, dir, stopPred);
        public FaceVerGroup Group() => new FaceVerGroup(MyCube.Grid, new List<FaceVer>() { this });
    }

    public class Corner : Facet
    {
        private CornerFacetPrimitive facePrimitive;

        public CornerFacetPrimitive FacePrimitive
        {
            get => facePrimitive;
            set
            {
                facePrimitive = value;
                MyCube.Changed = true;
            }
        }

        public CORNER CornerType => FacePrimitive.CornerType;

        public Corner(Cube myCube, Vector3Int direction) : base(myCube, direction)
        {
            FacePrimitive = new CornerFacetPrimitive();
        }

        public override void CreateGeometry(float scale, IGridGeometryOwner world)
        {
            if (FacePrimitive.Resolved)
                return;

            var dir = Direction;
            var myPosition = MyCube.Position;
            var middle = new Vector3Int(dir.x, 0, dir.z);
            var directions = new Vector3Int[4] {
                new Vector3Int(0, 0, 0),
                new Vector3Int(0, 0, dir.z),
                new Vector3Int(dir.x, 0, 0),
                new Vector3Int(dir.x, 0, dir.z)
            };
            var primitives = directions.Select(direction => MyCube.Grid[myPosition + direction].Corners(middle - 2 * direction).FacePrimitive).ToList();
            var winningPrimitive = primitives.ArgMax(p => p.Priority);
            // Place stuff only from view of winning primitive
            if (winningPrimitive != FacePrimitive)
                return;

            var losingPrimitive = primitives.Others(winningPrimitive).First();
            winningPrimitive.PlacePrimitive(world, this, losingPrimitive);

            primitives.ForEach(primitive => primitive.Resolved = true);

        }

        public Corner MoveBy(Vector3Int offset) => MoveBy<Corner>(offset);
        public IEnumerable<Corner> MoveInDirUntil(Grid<Cube> gridView, Vector3Int dir, Func<Corner, bool> stopPred) => MoveInDirUntil<Corner>(gridView, dir, stopPred);
        public CornerGroup Group() => new CornerGroup(MyCube.Grid, new List<Corner>() { this });

        public IEnumerable<Cube> AllNeighbors()
        {
            var pos = MyCube.Position;
            var grid = MyCube.Grid;
            yield return MyCube;
            yield return grid[pos + Direction];
            yield return grid[pos + Vector3Int.forward * Direction.z];
            yield return grid[pos + Vector3Int.right * Direction.x];
        }
    }

    public enum FACE_HOR
    {
        Nothing,
        Railing,
        Wall,
        Door,
    }

    public enum FACE_VER
    {
        Nothing,
        Floor
    }

    public enum CORNER
    {
        Nothing,
        Beam,
        RailingPillar
    }

    public enum CUBE
    {
        Nothing,
        Stairs
    }
}

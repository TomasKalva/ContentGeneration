using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{

    public abstract class Facet
    {
        public Vector3Int Direction { get; set; }
        public ShapeGrammarObjectStyle Style { get; set; }
        public Cube MyCube { get; set; }
        public Cube OtherCube => MyCube.Grid[MyCube.Position + Direction];

        public abstract void Generate(float cubeSide, Transform parent, Vector3Int cubePosition);

        public FacetT MoveBy<FacetT>(Vector3Int offset) where FacetT : Facet
        {
            var offsetCube = MyCube.MoveBy(offset);
            return (FacetT)offsetCube?.Facets[Direction];
        }

        public IEnumerable<FacetT> MoveInDirUntil<FacetT>(Grid<Cube> gridView, Vector3Int dir, Func<FacetT, bool> stopPred) where FacetT : Facet
        {
            var validFacets = MyCube.MoveInDirUntil(dir, cube => stopPred((FacetT)cube.Facets[Direction])).Select(cube => (FacetT)cube.Facets[Direction]);
            return validFacets;
        }
    }

    public class FaceHor : Facet
    {
        private FACE_HOR faceType;

        public FACE_HOR FaceType
        {
            get => faceType;
            set
            {
                faceType = value;
                MyCube.Changed = true;
            }
        }

        public override void Generate(float scale, Transform parent, Vector3Int cubePosition)
        {
            if (Style == null)
                return;

            var otherFaceType = OtherCube.FacesHor(-Direction).FaceType;
            if (FaceType < otherFaceType)
                return;

            var offset = (Vector3)Direction * 0.5f;
            var obj = Style.GetFaceHor(FaceType);

            obj.SetParent(parent);
            obj.localScale = scale * Vector3.one;
            obj.localPosition = (cubePosition + offset) * scale;
            obj.rotation = Quaternion.LookRotation(Direction, Vector3.up);
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
        private FACE_VER faceType;

        public FACE_VER FaceType
        {
            get => faceType;
            set
            {
                faceType = value;
                MyCube.Changed = true;
            }
        }

        public override void Generate(float scale, Transform parent, Vector3Int cubePosition)
        {
            if (Style == null)
                return;

            var offset = Vector3.up * Math.Max(0, Direction.y);
            var obj = Style.GetFaceVer(FaceType);

            obj.SetParent(parent);
            obj.localScale = scale * Vector3.one;
            obj.localPosition = (cubePosition + offset) * scale;
        }

        public FaceVer MoveBy(Vector3Int offset) => MoveBy<FaceVer>(offset);
        public IEnumerable<FaceVer> MoveInDirUntil(Grid<Cube> gridView, Vector3Int dir, Func<FaceVer, bool> stopPred) => MoveInDirUntil<FaceVer>(gridView, dir, stopPred);
        public FaceVerGroup Group() => new FaceVerGroup(MyCube.Grid, new List<FaceVer>() { this });
    }

    public class Corner : Facet
    {
        private CORNER cornerType;

        public CORNER CornerType
        {
            get => cornerType;
            set
            {
                cornerType = value;
                MyCube.Changed = true;
            }
        }

        public override void Generate(float scale, Transform parent, Vector3Int cubePosition)
        {
            if (Style == null)
                return;

            var offset = (Vector3)Direction * 0.5f;
            var obj = Style.GetCorner(CornerType);

            obj.SetParent(parent);
            obj.localScale = scale * Vector3.one;
            obj.localPosition = (cubePosition + offset) * scale;
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
        Fence,
        Window,
        Wall,
        Door,
        Open,
        Special
    }

    public enum FACE_VER
    {
        Nothing,
        Floor
    }

    public enum CORNER
    {
        Nothing,
        Pillar,
        RailingPillar
    }

    public enum CUBE
    {
        Nothing,
        Stairs
    }
}

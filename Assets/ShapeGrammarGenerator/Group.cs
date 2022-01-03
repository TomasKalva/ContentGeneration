using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Grid = ShapeGrammar.Grid;

namespace ShapeGrammar
{

    public class Group
    {
        public Grid Grid { get; }

        public Group(Grid grid)
        {
            Grid = grid;
        }
    }

    public class CubeGroup : Group
    {
        public List<Cube> Cubes { get; }

        public CubeGroup(Grid grid, List<Cube> cubes) : base(grid)
        {
            Cubes = cubes;
        }

        public CubeGroup CubesLayer(Vector3Int dir)
        {
            return new CubeGroup(Grid, Cubes.Where(cube => !Cubes.Contains(Grid[cube.Position + dir])).ToList());
        }

        public CubeGroup MoveBy(Vector3Int offset)
        {
            var movedCubes = Cubes.SelectNN(cube => cube.MoveBy(offset));
            return new CubeGroup(Grid, movedCubes.ToList());
        }

        public CubeGroup MoveInDirUntil(Vector3Int dir, Func<Cube, bool> stopPred)
        {
            var validCubes = Cubes.SelectMany(corner => corner.MoveInDirUntil(dir, stopPred));
            return new CubeGroup(Grid, validCubes.ToList());
        }

        public CubeGroup Where(Func<Cube, bool> pred) => new CubeGroup(Grid, Cubes.Where(pred).ToList());
        public CubeGroup Minus(CubeGroup group) => new CubeGroup(Grid, Cubes.Except(group.Cubes).ToList());

        public CubeGroup ExtrudeHor()
        {
            var faceCubes = AllBoundaryFacesH().Extrude(1).Cubes;
            var cornerCubes = AllBoundaryCorners().Extrude(1).Cubes;
            return new CubeGroup(Grid, faceCubes.Concat(cornerCubes).Except(Cubes).ToList());
        }

        public CubeGroup WithFloor() => Where(cube => cube.FacesVer(Vector3Int.down).FaceType == FACE_VER.Floor);

        public CubeGroup SetGrammarStyle(StyleSetter styleSetter)
        {
            styleSetter(this);
            return this;
        }

        #region FacesH

        public FaceHorGroup FacesH(params Vector3Int[] horDirs)
        {
            var faces =
                from horDir in horDirs
                from cube in Cubes
                select cube.FacesHor(horDir);
            return new FaceHorGroup(Grid, faces.ToList());
        }

        public FaceHorGroup BoundaryFacesH(params Vector3Int[] horDirs)
        {
            return new FaceHorGroup(Grid, horDirs.Select(horDir => CubesLayer(horDir).FacesH(horDir)).SelectMany(i => i.Facets).ToList());
        }

        public FaceHorGroup AllBoundaryFacesH()
        {
            return BoundaryFacesH(ExtensionMethods.HorizontalDirections().ToArray());
        }
        #endregion

        #region FacesV

        public FaceVerGroup FacesV(params Vector3Int[] verDirs)
        {
            var faces =
                from horDir in verDirs
                from cube in Cubes
                select cube.FacesVer(horDir);
            return new FaceVerGroup(Grid, faces.ToList());
        }

        public FaceVerGroup BoundaryFacesV(params Vector3Int[] verDirs)
        {
            return new FaceVerGroup(Grid, verDirs.Select(verDir => CubesLayer(verDir).FacesV(verDir)).SelectMany(i => i.Facets).ToList());
        }

        public FaceVerGroup AllBoundaryFacesV()
        {
            return BoundaryFacesV(ExtensionMethods.VerticalDirections().ToArray());
        }
        #endregion

        #region Corners

        public CornerGroup Corners(params Vector3Int[] horDirs)
        {

            var cornerPairs =
                from horDir in horDirs
                let orthDir = ExtensionMethods.OrthogonalHorizontalDir(horDir)
                from cube in Cubes
                select new { i0 = cube.Corners(horDir + orthDir), i1 = cube.Corners(horDir - orthDir) };
            var corners = cornerPairs.Select(twoCorners => twoCorners.i0).Concat(cornerPairs.Select(twoCorners => twoCorners.i1)).Distinct();
            return new CornerGroup(Grid, corners.ToList());
        }

        public CornerGroup BoundaryCorners(params Vector3Int[] horDirs)
        {
            return new CornerGroup(Grid, horDirs.Select(verDir => CubesLayer(verDir).Corners(verDir)).SelectMany(i => i.Facets).ToList());
        }

        public CornerGroup AllBoundaryCorners()
        {
            return BoundaryCorners(ExtensionMethods.HorizontalDirections().ToArray());
        }

        #endregion
    }

    public abstract class FacetGroup<FacetT> : Group where FacetT : Facet
    {
        public List<FacetT> Facets { get; }

        public FacetGroup(Grid grid, List<FacetT> facets) : base(grid)
        {
            Facets = facets;
        }

        public CubeGroup Cubes()
        {
            return new CubeGroup(Grid, Facets.Select(face => face.MyCube).ToList());
        }

        /// <summary>
        /// Finishes after time invocations.
        /// </summary>
        Func<Cube, bool> CountdownMaker(int time)
        {
            var countdown = new Countdown(time);
            return cube => countdown.Tick();
        }

        public CubeGroup Extrude(int dist)
        {
            return new CubeGroup(Grid, Facets.SelectManyNN(face => face.OtherCube?.MoveInDirUntil(face.Direction, CountdownMaker(dist)))
                .ToList());
        }

        protected IEnumerable<FacetT> NeighboringIE(CubeGroup cubeGroup)
        {
            return Facets.Where(facet => facet.OtherCube.In(cubeGroup));
        }
    }

    public class FaceHorGroup : FacetGroup<FaceHor>
    {
        public FaceHorGroup(Grid grid, List<FaceHor> faces) : base(grid, faces)
        {
        }

        public FaceHorGroup Fill(FACE_HOR faceType)
        {
            Facets.ForEach(face => face.FaceType = faceType);
            return this;
        }

        public FaceHorGroup SetStyle(ShapeGrammarObjectStyle style)
        {
            Facets.ForEach(face => face.Style = style);
            return this;
        }

        public CornerGroup Corners()
        {
            return new CornerGroup(Grid, Facets.SelectManyNN(faceHor => faceHor.Corners()).Distinct().ToList());
        }

        public FaceHorGroup Where(Func<FaceHor, bool> pred) => new FaceHorGroup(Grid, Facets.Where(pred).ToList());
        public FaceHorGroup Neighboring(CubeGroup cubeGroup) => new FaceHorGroup(Grid, NeighboringIE(cubeGroup).ToList());
        public FaceHorGroup Minus(FaceHorGroup faceHorGroup) => new FaceHorGroup(Grid, Facets.Except(faceHorGroup.Facets).ToList());
    }

    class Countdown
    {
        public int Elapsed { get; private set; }
        public Countdown(int elapsed)
        {
            Elapsed = elapsed;
        }
        /// <summary>
        /// Returns true after finished.
        /// </summary>
        public bool Tick()
        {
            Elapsed--;
            return Elapsed < 0;
        }
    }

    public class FaceVerGroup : FacetGroup<FaceVer>
    {
        public FaceVerGroup(Grid grid, List<FaceVer> faces) : base(grid, faces)
        {
        }

        public FaceVerGroup Fill(FACE_VER faceType)
        {
            Facets.ForEach(face => face.FaceType = faceType);
            return this;
        }

        public FaceVerGroup SetStyle(ShapeGrammarObjectStyle style)
        {
            Facets.ForEach(face => face.Style = style);
            return this;
        }
        public FaceVerGroup Where(Func<FaceVer, bool> pred) => new FaceVerGroup(Grid, Facets.Where(pred).ToList());
        public FaceVerGroup Neighboring(CubeGroup cubeGroup) => new FaceVerGroup(Grid, NeighboringIE(cubeGroup).ToList());
        public FaceVerGroup Minus(FaceVerGroup faceVerGroup) => new FaceVerGroup(Grid, Facets.Except(faceVerGroup.Facets).ToList());
    }

    public class CornerGroup : FacetGroup<Corner>
    {
        public CornerGroup(Grid grid, List<Corner> faces) : base(grid, faces)
        {
        }

        public CornerGroup Fill(CORNER cornerType)
        {
            Facets.ForEach(corner => corner.CornerType = cornerType);
            return this;
        }

        public CornerGroup SetStyle(ShapeGrammarObjectStyle style)
        {
            Facets.ForEach(corner => corner.Style = style);
            return this;
        }

        public CornerGroup MoveBy(Vector3Int offset)
        {
            var movedCorners = Facets.SelectNN(corner => corner.MoveBy(offset));
            return new CornerGroup(Grid, movedCorners.ToList());
        }

        public CornerGroup MoveInDirUntil(Vector3Int dir, Func<Corner, bool> stopPred)
        {
            var validCorners = Facets.SelectMany(corner => corner.MoveInDirUntil(dir, stopPred));
            return new CornerGroup(Grid, validCorners.ToList());
        }

        public CornerGroup Where(Func<Corner, bool> pred) => new CornerGroup(Grid, Facets.Where(pred).ToList());
        public CornerGroup Neighboring(CubeGroup cubeGroup) => new CornerGroup(Grid, NeighboringIE(cubeGroup).ToList());
        public CornerGroup Minus(CornerGroup cornerGroup) => new CornerGroup(Grid, Facets.Except(cornerGroup.Facets).ToList());
    }

}

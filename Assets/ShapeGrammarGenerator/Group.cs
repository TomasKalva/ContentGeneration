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
        public GridView GridView { get; }
        public AreaType AreaType { get; set; }

        public Group(GridView grid, AreaType areaType)
        {
            GridView = grid;
            AreaType = areaType;
        }
    }

    /// <summary>
    /// Disjoint union of cube groups.
    /// </summary>
    public class CubeGroupGroup : Group
    {
        public List<CubeGroup> Groups { get; }

        public List<Cube> Cubes => Groups.SelectMany(g => g.Cubes).ToList();

        public CubeGroupGroup(GridView grid, AreaType areaType, params CubeGroup[] groups) : base(grid, areaType)
        {
            Groups = groups.ToList();
        }

        public CubeGroupGroup(GridView grid, AreaType areaType, List<CubeGroup> groups) : base(grid, areaType)
        {
            Groups = groups;
        }

        public CubeGroupGroup WithAreaType(AreaType areaType) => new CubeGroupGroup(GridView, areaType, Groups.Where(g => g.AreaType == areaType).ToList());

        public CubeGroupGroup Add(CubeGroup group) => new CubeGroupGroup(GridView, AreaType, Groups.Append(group).ToList());

        public CubeGroupGroup MoveBy(Vector3Int offset)
        {
            var movedGroups = Groups.Select(g => g.MoveBy(offset)).ToList();
            return new CubeGroupGroup(GridView, AreaType, movedGroups);
        }

        public CubeGroupGroup SetAreaType(AreaType areaType)
        {
            AreaType = areaType;
            return this;
        }

        public CubeGroupGroup SetGrammarStyle(StyleSetter styleSetter)
        {
            Groups.ForEach(g => styleSetter(g));
            return this;
        }

        public CubeGroupGroup ApplyGrammarStyleRules(StyleRules styleRules) 
        {
            styleRules.Apply(this);
            return this;
        }

        public CubeGroup CubeGroup() => new CubeGroup(GridView, AreaType, Groups.SelectMany(g => g.Cubes).ToList());
    }



    public class CubeGroup : Group
    {
        public virtual List<Cube> Cubes { get; }

        public CubeGroup(GridView grid, AreaType areaType, List<Cube> cubes) : base(grid, areaType)
        {
            Cubes = cubes;
        }

        public CubeGroup SetAreaType(AreaType areaType)
        {
            AreaType = areaType;
            return this;
        }

        public CubeGroup CubesLayer(Vector3Int dir)
        {
            return new CubeGroup(GridView, AreaType, Cubes.Where(cube => !Cubes.Contains(GridView[cube.Position + dir])).ToList());
        }

        public CubeGroup MoveBy(Vector3Int offset)
        {
            var movedCubes = Cubes.SelectNN(cube => cube.MoveBy(offset));
            return new CubeGroup(GridView, AreaType, movedCubes.ToList());
        }

        public CubeGroup MoveInDirUntil(Vector3Int dir, Func<Cube, bool> stopPred)
        {
            var validCubes = Cubes.SelectMany(corner => corner.MoveInDirUntil(dir, stopPred));
            return new CubeGroup(GridView, AreaType, validCubes.ToList());
        }

        public CubeGroup Where(Func<Cube, bool> pred) => new CubeGroup(GridView, AreaType, Cubes.Where(pred).ToList());
        public CubeGroup Minus(CubeGroup group) => new CubeGroup(GridView, AreaType, Cubes.Except(group.Cubes).ToList());

        public IEnumerable<Vector3Int> MinkowskiMinus(CubeGroup grp) => 
            (from cube1 in Cubes 
            from cube2 in grp.Cubes
            select cube1.Position - cube2.Position).Distinct();

        public CubeGroup ExtrudeHor()
        {
            var faceCubes = AllBoundaryFacesH().Extrude(1).Cubes;
            var cornerCubes = AllBoundaryCorners().Extrude(1).Cubes;
            return new CubeGroup(GridView, AreaType, faceCubes.Concat(cornerCubes).Except(Cubes).ToList());
        }

        public CubeGroup WithFloor() => Where(cube => cube.FacesVer(Vector3Int.down).FaceType == FACE_VER.Floor);

        public CubeGroup SetGrammarStyle(StyleSetter styleSetter)
        {
            styleSetter(this);
            Cubes.ForEach(cube => cube.Changed = true);
            return this;
        }

        #region FacesH

        public FaceHorGroup FacesH(params Vector3Int[] horDirs)
        {
            var faces =
                from horDir in horDirs
                from cube in Cubes
                select cube.FacesHor(horDir);
            return new FaceHorGroup(GridView, faces.ToList());
        }

        public FaceHorGroup BoundaryFacesH(params Vector3Int[] horDirs)
        {
            return new FaceHorGroup(GridView, horDirs.Select(horDir => CubesLayer(horDir).FacesH(horDir)).SelectMany(i => i.Facets).ToList());
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
            return new FaceVerGroup(GridView, faces.ToList());
        }

        public FaceVerGroup BoundaryFacesV(params Vector3Int[] verDirs)
        {
            return new FaceVerGroup(GridView, verDirs.Select(verDir => CubesLayer(verDir).FacesV(verDir)).SelectMany(i => i.Facets).ToList());
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
            return new CornerGroup(GridView, corners.ToList());
        }

        public CornerGroup BoundaryCorners(params Vector3Int[] horDirs)
        {
            return new CornerGroup(GridView, horDirs.Select(verDir => CubesLayer(verDir).Corners(verDir)).SelectMany(i => i.Facets).ToList());
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

        public FacetGroup(GridView grid, List<FacetT> facets) : base(grid, AreaType.None)
        {
            Facets = facets;
        }

        public CubeGroup Cubes()
        {
            return new CubeGroup(GridView, AreaType.None, Facets.Select(face => face.MyCube).ToList());
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
            return new CubeGroup(GridView, AreaType, Facets.SelectManyNN(face => face.OtherCube?.MoveInDirUntil(face.Direction, CountdownMaker(dist)))
                .ToList());
        }

        protected IEnumerable<FacetT> NeighboringIE(CubeGroup cubeGroup)
        {
            return Facets.Where(facet => facet.OtherCube.In(cubeGroup));
        }
    }

    public class FaceHorGroup : FacetGroup<FaceHor>
    {
        public FaceHorGroup(GridView grid, List<FaceHor> faces) : base(grid, faces)
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
            return new CornerGroup(GridView, Facets.SelectManyNN(faceHor => faceHor.Corners()).Distinct().ToList());
        }

        public FaceHorGroup Where(Func<FaceHor, bool> pred) => new FaceHorGroup(GridView, Facets.Where(pred).ToList());
        public FaceHorGroup Neighboring(CubeGroup cubeGroup) => new FaceHorGroup(GridView, NeighboringIE(cubeGroup).ToList());
        public FaceHorGroup Minus(FaceHorGroup faceHorGroup) => new FaceHorGroup(GridView, Facets.Except(faceHorGroup.Facets).ToList());
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
        public FaceVerGroup(GridView grid, List<FaceVer> faces) : base(grid, faces)
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
        public FaceVerGroup Where(Func<FaceVer, bool> pred) => new FaceVerGroup(GridView, Facets.Where(pred).ToList());
        public FaceVerGroup Neighboring(CubeGroup cubeGroup) => new FaceVerGroup(GridView, NeighboringIE(cubeGroup).ToList());
        public FaceVerGroup Minus(FaceVerGroup faceVerGroup) => new FaceVerGroup(GridView, Facets.Except(faceVerGroup.Facets).ToList());
    }

    public class CornerGroup : FacetGroup<Corner>
    {
        public CornerGroup(GridView grid, List<Corner> faces) : base(grid, faces)
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
            return new CornerGroup(GridView, movedCorners.ToList());
        }

        public CornerGroup MoveInDirUntil(Vector3Int dir, Func<Corner, bool> stopPred)
        {
            var validCorners = Facets.SelectMany(corner => corner.MoveInDirUntil(dir, stopPred));
            return new CornerGroup(GridView, validCorners.ToList());
        }

        public CornerGroup Where(Func<Corner, bool> pred) => new CornerGroup(GridView, Facets.Where(pred).ToList());
        public CornerGroup Neighboring(CubeGroup cubeGroup) => new CornerGroup(GridView, NeighboringIE(cubeGroup).ToList());
        public CornerGroup Minus(CornerGroup cornerGroup) => new CornerGroup(GridView, Facets.Except(cornerGroup.Facets).ToList());
    }

}

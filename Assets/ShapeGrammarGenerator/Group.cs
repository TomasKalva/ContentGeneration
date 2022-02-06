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
        public virtual List<Cube> Cubes { get; }

        public int ExtentsDir(Vector3Int dir) => dir.Dot(CubesMaxLayer(dir).FirstOrDefault().Position - CubesMaxLayer(-dir).FirstOrDefault().Position) + 1;

        public int LengthX() => ExtentsDir(Vector3Int.right);
        public int LengthY() => ExtentsDir(Vector3Int.up);
        public int LengthZ() => ExtentsDir(Vector3Int.forward);

        public CubeGroup(Grid grid,List<Cube> cubes) : base(grid)
        {
            Debug.Assert(cubes.Any());
            Cubes = cubes.Distinct().ToList();
        }

        public CubeGroup CubeGroupLayer(Vector3Int dir)
        {
            return new CubeGroup(Grid, Cubes.Where(cube => !Cubes.Contains(Grid[cube.Position + dir])).ToList());
        }

        public CubeGroup CubeGroupMaxLayer(Vector3Int dir)
        {
            return new CubeGroup(Grid, CubesMaxLayer(dir).ToList());
        }

        IEnumerable<Cube> CubesMaxLayer(Vector3Int dir)
        {
            var max = Cubes.Max(cube => Vector3.Dot(cube.Position, dir));
            return Cubes.Where(cube => Vector3.Dot(cube.Position, dir) == max);
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

        public LevelGeometryElement LevelElement(AreaType areaType = null)
        {
            if(areaType == null)
            {
                // AreaType.None is not a constant so can't be a default value
                areaType = AreaType.None;
            }
            return new LevelGeometryElement(Grid, areaType, this);
        }

        public CubeGroup Where(Func<Cube, bool> pred) => new CubeGroup(Grid, Cubes.Where(pred).ToList());
        public CubeGroup Select3(Func<Cube, Cube, Cube, bool> pred) => new CubeGroup(Grid, Cubes.Where3(pred).ToList());
        /// <summary>
        /// Includes first and last element.
        /// </summary>
        public CubeGroup Where3Cycle(Func<Cube, Cube, Cube, bool> pred) => new CubeGroup(Grid, Cubes.Where3(pred).Prepend(Cubes.FirstOrDefault()).Append(Cubes.LastOrDefault()).ToList());
        public CubeGroup Minus(CubeGroup group) => new CubeGroup(Grid, Cubes.Except(group.Cubes).ToList());

        public IEnumerable<Vector3Int> MinkowskiMinus(CubeGroup grp) => 
            (from cube1 in Cubes 
            from cube2 in grp.Cubes
            select cube1.Position - cube2.Position).Distinct();

        public bool Intersects(CubeGroup cg)
        {
            return Cubes.Intersect(cg.Cubes).Any();
        }

        public IEnumerable<CubeGroup> SplitToConnected()
        {
            var cubesSet = new HashSet<Cube>(Cubes);
            Neighbors<Cube> neighbors = 
                cube => ExtensionMethods.Directions()
                    .Select(dir => cube.Grid[cube.Position + dir])
                    .Where(neighCube => cubesSet.Contains(neighCube));
            // create graph for searching for the path
            var graph = new ImplicitGraph<Cube>(neighbors);
            var graphAlgs = new GraphAlgorithms<Cube, Edge<Cube>, ImplicitGraph<Cube>>(graph);

            return graphAlgs.ConnectedComponentsSymm(Cubes).Select(cubes => new CubeGroup(Grid, cubes.ToList()));
        }

        public CubeGroup Merge(CubeGroup cg)
        {
            return new CubeGroup(Grid, Cubes.Concat(cg.Cubes).Distinct().ToList());
        }

        public CubeGroup ExtrudeHor(bool outside = true, bool takeChanged = true)
        {
            int dir = outside ? 1 : -1;
            var faceCubes = AllBoundaryFacesH().Extrude(dir, takeChanged).Cubes;
            var cornerCubes = AllBoundaryCorners().Extrude(dir, takeChanged).Cubes;
            return new CubeGroup(Grid, faceCubes.Concat(cornerCubes).Distinct().ToList());
        }

        public CubeGroup ExtrudeHorOut(int dist, bool takeChanged = true)
        {
            var totalGroup = this;
            for(int i = 0; i < dist; i++)
            {
                totalGroup = totalGroup.ExtrudeHor(true, takeChanged);
            }
            return totalGroup.Minus(this);
        }

        public CubeGroup ExtrudeVer(Vector3Int dir, int dist, bool takeChanged = true)
        {
            var upCubes = BoundaryFacesV(dir).Extrude(dist, takeChanged).Cubes;
            return new CubeGroup(Grid, upCubes.ToList());
        }

        public CubeGroup ExtrudeAll(bool outside = true, bool takeChanged = true)
        {
            var sides = ExtrudeHor(outside, takeChanged).Cubes;
            var up = ExtrudeVer(Vector3Int.up, 1, takeChanged).Cubes;
            var down = ExtrudeVer(Vector3Int.down, 1, takeChanged).Cubes;
            return new CubeGroup(Grid, sides.Concat(up.Concat(down)).ToList());
        }

        public CubeGroup ExtrudeDir(Vector3Int dir, int dist = 1, bool takeChanged = true)
        {
            var extruded = dir.y == 0 ?
                BoundaryFacesH(dir).Extrude(dist, takeChanged) :
                BoundaryFacesV(dir).Extrude(dist, takeChanged);
            return extruded;
        }

        public CubeGroup WithFloor() => Where(cube => cube.FacesVer(Vector3Int.down).FaceType == FACE_VER.Floor);

        public CubeGroup Symmetrize(FaceHor faceHor)
        {
            var myCubePos = faceHor.MyCube.Position;
            var dir = faceHor.OtherCube.Position - faceHor.MyCube.Position;
            var absDir = dir.ComponentWise(Mathf.Abs);
            Func<Vector3Int, Vector3Int> flipped =
                faceHor.Direction.x == 0 ?
                p => p + 2 * (myCubePos.z - p.z) * absDir + dir :
                (p => p + 2 * (myCubePos.x - p.x) * absDir + dir);
            return new CubeGroup(Grid, Cubes.Select(cube => Grid[flipped(cube.Position)]).ToList());
        }

        /// <summary>
        /// dists are in number of cubes from cube from lowest value in dir.
        /// </summary>
        public IEnumerable<CubeGroup> Split(Vector3Int dir, params int[] dists)
        {
            var min = Cubes.Min(cube => cube.Position.Dot(dir));
            var split = from cube in Cubes
                   group cube by dists.IndexOfInterval((cube.Position.Dot(dir) - min)) into splitGroup
                   select splitGroup.AsEnumerable().ToCubeGroup(Grid);
            return Vector3Int.one.Dot(dir) > 0 ? split : split.Reverse();
        }

        /// <summary>
        /// relDists are relative in distance between cube with lowest and highest value in dir.
        /// </summary>
        public IEnumerable<CubeGroup> SplitRel(Vector3Int dir, params float[] relDists)
        {
            var min = Cubes.Min(cube => cube.Position.Dot(dir));
            var max = Cubes.Max(cube => cube.Position.Dot(dir));
            var minMaxDist = max - min + 1;
            return Split(dir, relDists.Select(relDist => Mathf.RoundToInt((minMaxDist * relDist))).ToArray());
        }

        public CubeGroup SetGrammarStyle(StyleSetter styleSetter)
        {
            styleSetter(this);
            Cubes.ForEach(cube => cube.Changed = true);
            return this;
        }

        public CubeGroup Fill(CUBE cubeObject)
        {
            Cubes.ForEach(cube =>
            {
                cube.Object = cubeObject;
                cube.Changed = true;
            });
            return this;
        }

        public CubeGroup SetStyle(ShapeGrammarObjectStyle style)
        {
            Cubes.ForEach(face => face.Style = style);
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
            return new FaceHorGroup(Grid, horDirs.Select(horDir => CubeGroupLayer(horDir).FacesH(horDir)).SelectMany(i => i.Facets).ToList());
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
            return new FaceVerGroup(Grid, verDirs.Select(verDir => CubeGroupLayer(verDir).FacesV(verDir)).SelectMany(i => i.Facets).ToList());
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
            return new CornerGroup(Grid, horDirs.Select(verDir => CubeGroupLayer(verDir).Corners(verDir)).SelectMany(i => i.Facets).ToList());
        }

        public CornerGroup SpecialCorners(params Vector3Int[] horDirs)
        {
            var specialCorners = BoundaryCorners(horDirs).Facets.Where(corner => corner.AllNeighbors().Where(c => c.In(this)).Count() != 2).ToList();
            return new CornerGroup(Grid, specialCorners);
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

        public CubeGroup Extrude(int dist, bool takeChanged = true)
        {
            Func<Func<Cube, bool>> stopConditionFact = () =>
            {
                var countdown = CountdownMaker(Mathf.Abs(dist));
                return takeChanged ? countdown : cube => cube.Changed || countdown(cube);
            };
            Func<Facet, IEnumerable<Cube>> cubeSelector = dist > 0 ?
                    face => face.OtherCube?.MoveInDirUntil(face.Direction, stopConditionFact()) :
                    face => face.MyCube.MoveInDirUntil(-face.Direction, stopConditionFact());
            var extrudedCubes = Facets.SelectManyNN(cubeSelector).Distinct().ToList();
            return new CubeGroup(Grid, extrudedCubes);
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
        public FaceHorGroup Intersect(FaceHorGroup faceHGroup) => new FaceHorGroup(Grid, Facets.Intersect(faceHGroup.Facets).ToList());
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
        public FaceVerGroup Intersect(FaceVerGroup faceVGroup) => new FaceVerGroup(Grid, Facets.Intersect(faceVGroup.Facets).ToList());
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
            var validCorners = Facets.SelectMany(corner => corner.MoveInDirUntil(Grid, dir, stopPred));
            return new CornerGroup(Grid, validCorners.ToList());
        }

        public CornerGroup Where(Func<Corner, bool> pred) => new CornerGroup(Grid, Facets.Where(pred).ToList());
        public CornerGroup Intersect(CornerGroup cornerGroup) => new CornerGroup(Grid, Facets.Intersect(cornerGroup.Facets).ToList());
        public CornerGroup Neighboring(CubeGroup cubeGroup) => new CornerGroup(Grid, NeighboringIE(cubeGroup).ToList());
        public CornerGroup Minus(CornerGroup cornerGroup) => new CornerGroup(Grid, Facets.Except(cornerGroup.Facets).ToList());
    }

}

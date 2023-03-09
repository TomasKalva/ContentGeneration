using OurFramework.Environment.StylingAreas;
using OurFramework.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OurFramework.Environment.GridMembers
{
    /// <summary>
    /// Group of grid primitives. The group is immutable - operations return a new group.
    /// </summary>
    public class Group
    {
        public Grid<Cube> Grid { get; }

        public Group(Grid<Cube> grid)
        {
            Grid = grid;
        }
    }

    /// <summary>
    /// Group of cubes.
    /// </summary>
    public class CubeGroup : Group
    {
        public static CubeGroup Zero(Grid<Cube> grid) => new CubeGroup(grid, grid[0, 0, 0].ToEnumerable().ToList());

        public virtual List<Cube> Cubes { get; }

        public Vector3Int LeftBottomBack()
        {
            AssertNonEmpty();
            return new Vector3Int(
                Cubes.Min(c => c.Position.x),
                Cubes.Min(c => c.Position.y),
                Cubes.Min(c => c.Position.z)
            );
        }

        public Vector3Int RightTopFront()
        {
            AssertNonEmpty();
            return new Vector3Int(
                Cubes.Max(c => c.Position.x),
                Cubes.Max(c => c.Position.y),
                Cubes.Max(c => c.Position.z)
            );
        }

        public Vector3Int Extents() => RightTopFront() - LeftBottomBack() + Vector3Int.one;
        public int ExtentsDir(Vector3Int dir) => dir.Dot(CubesMaxLayer(dir).FirstOrDefault().Position - CubesMaxLayer(-dir).FirstOrDefault().Position) + 1;

        public int LengthX() => ExtentsDir(Vector3Int.right);
        public int LengthY() => ExtentsDir(Vector3Int.up);
        public int LengthZ() => ExtentsDir(Vector3Int.forward);

        /// <summary>
        /// Center of mass of the group.
        /// </summary>
        public Vector3 Center()
        {
            AssertNonEmpty();
            var sum = Cubes.Aggregate(Vector3Int.zero, (total, c) => total + c.Position);
            var count = Cubes.Count;
            return sum / count;
        }

        public Func<Transform> MakeArchitectureElements { get; set; }

        public delegate CubeGroup Constructor(CubeGroup existing, List<Cube> newCubes);

        /// <summary>
        /// Used by mainly extruding operations to construct a new cube group.
        /// todo: make all operations preserve Constr
        /// </summary>
        Constructor Constr { get; set; }

        public CubeGroup(Grid<Cube> grid,List<Cube> cubes) : base(grid)
        {
            //Debug.Assert(cubes.Any());
            Cubes = cubes.Distinct().ToList();
            MakeArchitectureElements = null;
            OpNew();
        }

        CubeGroup(Grid<Cube> grid, List<Cube> cubes, Constructor constr) : this(grid, cubes)
        {
            Constr = constr;
        }

        public CubeGroup AssertNonEmpty()
        {
            if (!Cubes.Any())
            {
                throw new GroupEmptyException("Failed group not empty assertion.");
            }
            return this;
        }

        /// <summary>
        /// Sets mode of operations to adding - new cubes will be added to this one.
        /// </summary>
        public CubeGroup OpAdd()
        {
            CubeGroup add(CubeGroup existing, List<Cube> newCubes)
            {
                return new CubeGroup(Grid, existing.Cubes.Concat(newCubes).ToList(), add);
            }
            Constr = add;
            return this;
        }

        /// <summary>
        /// Sets mode of operations to subtracting - new cubes will be subtracted from this one.
        /// </summary>
        public CubeGroup OpSub()
        {
            CubeGroup sub(CubeGroup existing, List<Cube> newCubes)
            {
                return new CubeGroup(Grid, existing.Cubes.SetMinus(newCubes).ToList(), sub);
            }
            Constr = sub;
            return this;
        }

        /// <summary>
        /// Sets mode of operations to replacing - only new cubes will be in the new group.
        /// </summary>
        public CubeGroup OpNew()
        {
            CubeGroup @new(CubeGroup existing, List<Cube> newCubes)
            {
                return new CubeGroup(Grid, newCubes, @new);
            }
            Constr = @new;
            return this;
        }

        /// <summary>
        /// True iff none of the cubes are taken.
        /// </summary>
        public bool AllAreNotTaken() => Cubes.All(cube => !cube.Changed);

        /// <summary>
        /// Returns cubes that are not taken yet.
        /// </summary>
        public CubeGroup NotTaken() => Cubes.Where(cube => !cube.Changed).ToCubeGroup(Grid);

        /// <summary>
        /// Returns all cubes that don't have neighbor in given direction in this group.
        /// </summary>
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
            AssertNonEmpty();
            var max = Cubes.Max(cube => Vector3.Dot(cube.Position, dir));
            return Cubes.Where(cube => Vector3.Dot(cube.Position, dir) == max);
        }

        public CubeGroup MoveBy(Vector3Int offset)
        {
            var movedCubes = Cubes.SelectNN(cube => cube.MoveBy(offset));
            return Constr(this, movedCubes.ToList());
        }

        /// <summary>
        /// Moves in the direction dir until stopPred is true. All the cubes of the path are taken.
        /// </summary>
        public CubeGroup MoveInDirUntil(Vector3Int dir, Func<Cube, bool> stopPred)
        {
            var validCubes = Cubes.SelectMany(cube => cube.MoveInDirUntil(dir, stopPred));
            return new CubeGroup(Grid, validCubes.ToList());
        }

        /// <summary>
        /// Creates level element out of this.
        /// </summary>
        public LevelGeometryElement LE(AreaStyle areaType = null)
        {
            if(areaType == null)
            {
                // AreaType.None is not a constant so it can't be a default value
                areaType = AreaStyles.None();
            }
            return new LevelGeometryElement(Grid, areaType, this);
        }

        public CubeGroup Where(Func<Cube, bool> pred) => new CubeGroup(Grid, Cubes.Where(pred).ToList());
        
        /// <summary>
        /// For each consecutive triple (l, t, r) of cubes returns t if the triple satisfies the condition.
        /// </summary>
        public CubeGroup Where3(Func<Cube, Cube, Cube, bool> pred) => new CubeGroup(Grid, Cubes.Where3(pred).ToList());
        
        /// <summary>
        /// Includes first and last element.
        /// </summary>
        public CubeGroup Where3All(Func<Cube, Cube, Cube, bool> pred) => new CubeGroup(Grid, Cubes.Where3(pred).Prepend(Cubes.FirstOrDefault()).Append(Cubes.LastOrDefault()).ToList());
        public CubeGroup Minus(CubeGroup group) => new CubeGroup(Grid, Cubes.Except(group.Cubes).ToList(), Constr);

        /// <summary>
        /// Returns Minkowski difference of the two groups.
        /// </summary>
        public IEnumerable<Vector3Int> MinkowskiMinus(CubeGroup grp) => 
            (from cube1 in Cubes 
            from cube2 in grp.Cubes
            select cube1.Position - cube2.Position).Distinct();

        public bool Intersects(CubeGroup cg)
        {
            return Cubes.Intersect(cg.Cubes).Any();
        }

        /// <summary>
        /// Splits this group to connected components.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Merges the two groups and removes all distinct cubes.
        /// </summary>
        public CubeGroup Merge(CubeGroup cg)
        {
            return new CubeGroup(Grid, Cubes.Concat(cg.Cubes).Distinct().ToList());
        }

        /// <summary>
        /// Extrudes the group in all horizontal directions.
        /// </summary>
        public CubeGroup ExtrudeHor(bool outside = true, bool takeChanged = true)
        {
            int dir = outside ? 1 : -1;
            var faceCubes = AllBoundaryFacesH().Extrude(dir, takeChanged).Cubes;
            var cornerCubes = AllBoundaryCorners().Extrude(dir, takeChanged).Cubes;
            return Constr(this, faceCubes.Concat(cornerCubes).Distinct().ToList());
        }

        /// <summary>
        /// Extrudes the group outside in all horizontal directions.
        /// </summary>
        public CubeGroup ExtrudeHorOut(int dist, bool takeChanged = true)
        {
            var totalGroup = this;
            for(int i = 0; i < dist; i++)
            {
                totalGroup = totalGroup.ExtrudeHor(true, takeChanged);
            }
            return Constr(this, totalGroup.Cubes.SetMinus(Cubes).ToList());
        }

        /// <summary>
        /// Extrude in a vertical direction.
        /// </summary>
        public CubeGroup ExtrudeVer(Vector3Int dir, int dist, bool takeChanged = true)
        {
            var upCubes = BoundaryFacesV(dir).Extrude(dist, takeChanged).Cubes;
            return Constr(this, upCubes.ToList());
        }

        /// <summary>
        /// Extrudes in all directions.
        /// </summary>
        public CubeGroup ExtrudeAll(bool outside = true, bool takeChanged = true)
        {
            var currentConstr = Constr;
            OpNew();
            var sides = ExtrudeHor(outside, takeChanged).Cubes;
            var up = ExtrudeVer(Vector3Int.up, 1, takeChanged).Cubes;
            var down = ExtrudeVer(Vector3Int.down, 1, takeChanged).Cubes;
            Constr = currentConstr;
            return Constr(this, sides.Concat(up.Concat(down)).ToList());
        }

        /// <summary>
        /// Extrude in direction dir.
        /// </summary>
        public CubeGroup ExtrudeDir(Vector3Int dir, int dist = 1, bool takeChanged = true)
        {
            var extruded = dir.y == 0 ?
                BoundaryFacesH(dir).Extrude(dist, takeChanged) :
                BoundaryFacesV(dir).Extrude(dist, takeChanged);
            return Constr(this, extruded.Cubes);
        }

        public CubeGroup BottomLayer() => CubesMaxLayer(Vector3Int.down).ToCubeGroup(Grid);
        /// <summary>
        /// Group of cubes that have floor.
        /// </summary>
        public CubeGroup WithFloor() => Where(cube => cube.FacesVer(Vector3Int.down).FaceType == FACE_VER.Floor);

        /// <summary>
        /// Symmetrizes this group with faceHor as an axis.
        /// </summary>
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
            AssertNonEmpty();
            var min = Cubes.Min(cube => cube.Position.Dot(dir));
            var split = from cube in Cubes
                   group cube by dists.IndexOfInterval((cube.Position.Dot(dir) - min)) into splitGroup
                   select splitGroup.AsEnumerable().ToCubeGroup(Grid);
            split = split.OrderBy(cg => cg.Cubes.First().Position.Dot(dir));
            return Vector3Int.one.Dot(dir) > 0 ? split : split;
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

        /// <summary>
        /// Put cube primitive to this cube.
        /// </summary>
        public CubeGroup Fill(CubePrimitive cubePrimitive)
        {
            Cubes.ForEach(cube =>
            {
                cube.CubePrimitive = cubePrimitive;
                cube.Changed = true;
            });
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

        /// <summary>
        /// Inside faces between consecutive cubes.
        /// </summary>
        public FaceHorGroup ConsecutiveInsideFacesH()
        {
            return new FaceHorGroup(Grid,
                Cubes.Select2((first, second) =>
                    {
                        var dir = second.Position - first.Position;
                        return dir.y == 0 ? 
                            new FaceHor[2] { first.FacesHor(dir), second.FacesHor(-dir) } : 
                            Enumerable.Empty<FaceHor>();
                    })
                    .SelectMany(faces => faces)
                    .ToList()
                );
        }

        /// <summary>
        /// Faces neighboring with the other group.
        /// </summary>
        public FaceHorGroup NeighborsInGroupH(CubeGroup cubes)
        {
            var cubesSet = new HashSet<Cube>(Cubes);
            var facesH = from cube in cubes.Cubes
                   from neighbor in cube.NeighborsHor()
                   where cubesSet.Contains(neighbor)
                   select cube.FacesHor(neighbor.Position - cube.Position);
            return new FaceHorGroup(Grid, facesH.ToList());
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

        /// <summary>
        /// Corners that correspond to real corners in wall. 
        /// </summary>
        public CornerGroup SpecialCorners(params Vector3Int[] horDirs)
        {
            var specialCorners = BoundaryCorners(horDirs).Facets.Where(corner => corner.AllNeighbors().Where(c => c.In(this)).Count() != 2).ToList();
            return new CornerGroup(Grid, specialCorners);
        }

        public CornerGroup AllSpecialCorners()
        {
            return SpecialCorners(ExtensionMethods.HorizontalDirections().ToArray());
        }

        public CornerGroup AllBoundaryCorners()
        {
            return BoundaryCorners(ExtensionMethods.HorizontalDirections().ToArray());
        }

        #endregion
    }

    /// <summary>
    /// Group of cube facets.
    /// </summary>
    public abstract class FacetGroup<FacetT> : Group where FacetT : Facet
    {
        public List<FacetT> Facets { get; }

        public FacetGroup(Grid<Cube> grid, List<FacetT> facets) : base(grid)
        {
            Facets = facets;
        }

        /// <summary>
        /// Cube group of cubes who own these facets.
        /// </summary>
        /// <returns></returns>
        public CubeGroup CG()
        {
            return new CubeGroup(Grid, Facets.Select(face => face.MyCube).ToList());
        }

        /// <summary>
        /// Finishes after time + 1 invocations.
        /// </summary>
        Func<Cube, bool> CountdownMaker(int time)
        {
            var countdown = new EventsCountdown(time + 1);
            return cube => countdown.Finished(true);
        }

        /// <summary>
        /// Extrude cube group from these facets.
        /// </summary>
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

    /// <summary>
    /// Group of facets with horizontal normals.
    /// </summary>
    public class FaceHorGroup : FacetGroup<FaceHor>
    {
        public FaceHorGroup(Grid<Cube> grid, List<FaceHor> faces) : base(grid, faces)
        {
        }

        public FaceHorGroup Fill(Func<HorFacePrimitive> facePrimitiveF)
        {
            Facets.ForEach(face => face.FacePrimitive = facePrimitiveF());
            return this;
        }

        public FaceHorGroup FillIfEmpty(Func<HorFacePrimitive> facePrimitiveF)
        {
            Facets.ForEach(face =>
            {
                if (face.FaceType == FACE_HOR.Nothing)
                {
                    face.FacePrimitive = facePrimitiveF();
                }
            });
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

    /// <summary>
    /// Group of facets with vertical normal.
    /// </summary>
    public class FaceVerGroup : FacetGroup<FaceVer>
    {
        public FaceVerGroup(Grid<Cube> grid, List<FaceVer> faces) : base(grid, faces)
        {
        }

        public FaceVerGroup Fill(Func<VerFacePrimitive> facePrimitiveF)
        {
            Facets.ForEach(face => face.FacePrimitive = facePrimitiveF());
            return this;
        }

        public FaceVerGroup Where(Func<FaceVer, bool> pred) => new FaceVerGroup(Grid, Facets.Where(pred).ToList());
        public FaceVerGroup Intersect(FaceVerGroup faceVGroup) => new FaceVerGroup(Grid, Facets.Intersect(faceVGroup.Facets).ToList());
        public FaceVerGroup Neighboring(CubeGroup cubeGroup) => new FaceVerGroup(Grid, NeighboringIE(cubeGroup).ToList());
        public FaceVerGroup Minus(FaceVerGroup faceVerGroup) => new FaceVerGroup(Grid, Facets.Except(faceVerGroup.Facets).ToList());
    }

    /// <summary>
    /// Group of corner edges.
    /// </summary>
    public class CornerGroup : FacetGroup<Corner>
    {
        public CornerGroup(Grid<Cube> grid, List<Corner> faces) : base(grid, faces)
        {
        }

        public CornerGroup Fill(Func<CornerFacetPrimitive> cornerPrimitiveF)
        {
            Facets.ForEach(corner => corner.FacePrimitive = cornerPrimitiveF());
            return this;
        }

        public CornerGroup FillIfEmpty(Func<CornerFacetPrimitive> facePrimitiveF)
        {
            Facets.ForEach(face =>
            {
                if (face.CornerType == CORNER.Nothing)
                {
                    face.FacePrimitive = facePrimitiveF();
                }
            });
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

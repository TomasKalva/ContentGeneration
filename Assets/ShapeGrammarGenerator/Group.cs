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
        public AreaType AreaType { get; set; }

        public Group(Grid grid, AreaType areaType)
        {
            Grid = grid;
            AreaType = areaType;
        }
    }

    public abstract class LevelElement
    {
        public Grid Grid { get; }
        public AreaType AreaType { get; set; }

        public abstract List<Cube> Cubes();

        public LevelElement(Grid grid, AreaType areaType)
        {
            Grid = grid;
            AreaType = areaType;
        }

        public IEnumerable<LevelElement> WithAreaType(AreaType areaType) => Flatten().Where(g => g.AreaType == areaType).ToList();

        protected abstract LevelElement MoveByImpl(Vector3Int offset);

        public LevelElement MoveBy(Vector3Int offset)
        {
            return MoveByImpl(offset);
        }

        public abstract IEnumerable<LevelElement> Flatten();

        public LevelElement SetAreaType(AreaType areaType)
        {
            AreaType = areaType;
            return this;
        }

        public LevelElement SetGrammarStyle(StyleSetter styleSetter)
        {
            styleSetter(CubeGroup());
            return this;
        }

        public LevelElement ApplyGrammarStyleRules(StyleRules styleRules) 
        {
            styleRules.Apply(this);
            return this;
        }

        public abstract CubeGroup CubeGroup();
    }

    public class LevelGroupElement : LevelElement
    {
        public List<LevelElement> LevelElements { get; }

        public override List<Cube> Cubes() => LevelElements.SelectMany(le => le.Cubes()).ToList();

        public LevelGroupElement(Grid grid, AreaType areaType, params LevelElement[] levelElements) : base(grid, areaType)
        {
            LevelElements = levelElements.ToList();
        }

        public LevelGroupElement(Grid grid, AreaType areaType, List<LevelElement> levelElements) : base(grid, areaType)
        {
            LevelElements = levelElements.ToList();
        }

        public LevelGroupElement SetChildrenAreaType(AreaType areaType)
        {
            LevelElements.ForEach(le => le.SetAreaType(areaType));
            return this;
        }

        public override IEnumerable<LevelElement> Flatten() => LevelElements.SelectMany(le => le.Flatten()).Prepend(this);

        public LevelGroupElement Add(LevelElement levelElement) => new LevelGroupElement(Grid, AreaType, LevelElements.Append(levelElement).ToList());

        protected override LevelElement MoveByImpl(Vector3Int offset)
        {
            var movedGroups = LevelElements.Select(le => le.MoveBy(offset)).ToList();
            return new LevelGroupElement(Grid, AreaType, movedGroups);
        }

        public new LevelGroupElement MoveBy(Vector3Int offset)
        {
            return (LevelGroupElement)MoveByImpl(offset);
        }

        public LevelGroupElement Select(Func<LevelElement, LevelElement> selector) => new LevelGroupElement(Grid, AreaType, LevelElements.Select(selector).ToList());

        public override CubeGroup CubeGroup() => new CubeGroup(Grid, AreaType, Cubes());
    }

    public class LevelGeometryElement : LevelElement
    {
        public CubeGroup Group { get; }

        public override List<Cube> Cubes() => Group.Cubes;

        public LevelGeometryElement(Grid grid, AreaType areaType, CubeGroup group) : base(grid, areaType)
        {
            Group = group;
        }

        public override IEnumerable<LevelElement> Flatten() => new LevelElement[1] { this };

        protected override LevelElement MoveByImpl(Vector3Int offset)
        {
            var movedGroup = Group.MoveBy(offset);
            return new LevelGeometryElement(Grid, AreaType, movedGroup);
        }

        public new LevelGeometryElement MoveBy(Vector3Int offset)
        {
            return (LevelGeometryElement)MoveByImpl(offset);
        }

        public override CubeGroup CubeGroup() => Group;
    }

    public class CubeGroup : Group
    {
        public virtual List<Cube> Cubes { get; }

        public CubeGroup(Grid grid, AreaType areaType, List<Cube> cubes) : base(grid, areaType)
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
            return new CubeGroup(Grid, AreaType, Cubes.Where(cube => !Cubes.Contains(Grid[cube.Position + dir])).ToList());
        }

        public CubeGroup CubesMaxLayer(Vector3Int dir)
        {
            var max = Cubes.Max(cube => Vector3.Dot(cube.Position, dir));
            return new CubeGroup(Grid, AreaType, Cubes.Where(cube => Vector3.Dot(cube.Position, dir) == max).ToList());
        }

        public CubeGroup MoveBy(Vector3Int offset)
        {
            var movedCubes = Cubes.SelectNN(cube => cube.MoveBy(offset));
            return new CubeGroup(Grid, AreaType, movedCubes.ToList());
        }

        public CubeGroup MoveInDirUntil(Vector3Int dir, Func<Cube, bool> stopPred)
        {
            var validCubes = Cubes.SelectMany(corner => corner.MoveInDirUntil(dir, stopPred));
            return new CubeGroup(Grid, AreaType, validCubes.ToList());
        }

        public LevelGeometryElement LevelElement(AreaType areaType) => new LevelGeometryElement(Grid, areaType, this);

        public CubeGroup Where(Func<Cube, bool> pred) => new CubeGroup(Grid, AreaType, Cubes.Where(pred).ToList());
        public CubeGroup Select3(Func<Cube, Cube, Cube, bool> pred) => new CubeGroup(Grid, AreaType, Cubes.Select3(pred).ToList());
        /// <summary>
        /// Includes first and last element.
        /// </summary>
        public CubeGroup Select3Incl(Func<Cube, Cube, Cube, bool> pred) => new CubeGroup(Grid, AreaType, Cubes.Select3(pred).Prepend(Cubes.FirstOrDefault()).Append(Cubes.LastOrDefault()).ToList());
        public CubeGroup Minus(CubeGroup group) => new CubeGroup(Grid, AreaType, Cubes.Except(group.Cubes).ToList());

        public IEnumerable<Vector3Int> MinkowskiMinus(CubeGroup grp) => 
            (from cube1 in Cubes 
            from cube2 in grp.Cubes
            select cube1.Position - cube2.Position).Distinct();

        public CubeGroup ExtrudeHor(int dir, bool takeChanged = true)
        {
            dir = dir > 0 ? 1 : -1;
            var faceCubes = AllBoundaryFacesH().Extrude(dir, takeChanged).Cubes;
            var cornerCubes = AllBoundaryCorners().Extrude(dir, takeChanged).Cubes;
            return new CubeGroup(Grid, AreaType, faceCubes.Concat(cornerCubes).Distinct().ToList());
        }

        public CubeGroup ExtrudeVer(Vector3Int dir, int dist, bool takeChanged = true)
        {
            var upCubes = BoundaryFacesV(dir).Extrude(dist, takeChanged).Cubes;
            return new CubeGroup(Grid, AreaType, upCubes.ToList());
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
                (Func < Vector3Int, Vector3Int > )(p => p + 2 * (myCubePos.x - p.x) * absDir + dir);
            return new CubeGroup(Grid, AreaType, Cubes.Select(cube => Grid[flipped(cube.Position)]).ToList());
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

        public FacetGroup(Grid grid, List<FacetT> facets) : base(grid, AreaType.None)
        {
            Facets = facets;
        }

        public CubeGroup Cubes()
        {
            return new CubeGroup(Grid, AreaType.None, Facets.Select(face => face.MyCube).ToList());
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
            var extrudedCubes = Facets.SelectManyNN(cubeSelector).ToList();
            return new CubeGroup(Grid, AreaType, extrudedCubes);
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

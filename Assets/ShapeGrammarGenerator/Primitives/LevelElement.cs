using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public abstract class LevelElement
    {
        public Grid<Cube> Grid { get; }
        public AreaType AreaType { get; set; }

        public abstract List<Cube> Cubes();

        public LevelElement(Grid<Cube> grid, AreaType areaType)
        {
            Grid = grid;
            AreaType = areaType;
        }

        public static LevelElement Empty(Grid<Cube> grid) => new LevelGeometryElement(grid, AreaType.None, new CubeGroup(grid, new List<Cube>()));

        #region Collection methods
        public IEnumerable<LevelElement> WithAreaType(AreaType areaType) => Where(g => g.AreaType == areaType).LevelElements.ToList();

        public LevelGroupElement Where(Func<LevelElement, bool> cond) => Flatten().Where(g => cond(g)).ToLevelGroupElement(Grid);
        public LevelGroupElement WhereGeom(Func<LevelElement, bool> cond) => Leafs().Where(g => cond(g)).ToLevelGroupElement(Grid);

        public abstract LevelElement ReplaceLeafs(Func<LevelGeometryElement, bool> cond, Func<LevelGeometryElement, LevelElement> replaceF);
        public LevelGeometryElement MapGeom(Func<CubeGroup, CubeGroup> f) => new LevelGeometryElement(Grid, AreaType, f(CG()));

        #endregion

        public IEnumerable<LevelGeometryElement> Nonterminals(AreaType areaType) => Leafs().Where(g => g.AreaType == areaType);

        protected abstract LevelElement MoveByImpl(Vector3Int offset);

        public LevelElement MoveBy(Vector3Int offset)
        {
            return MoveByImpl(offset);
        }

        public LevelElement MoveBottomBy(int yOffset, int minY)
        {
            var cg = CG();
            if (!cg.Cubes.Any())
                return this;

            var bottom = cg.LeftBottomBack().y;
            var newBottom = Math.Max(minY, bottom + yOffset);
            return MoveBy((newBottom - bottom) * Vector3Int.up);
        }

        public LevelElement MoveBottomTo(int yPosition)
        {
            var cg = CG();
            if (!cg.Cubes.Any())
                return this;

            var bottom = cg.LeftBottomBack().y;
            return MoveBy((yPosition - bottom) * Vector3Int.up);
        }

        public abstract IEnumerable<LevelElement> Flatten();

        public abstract IEnumerable<LevelGeometryElement> Leafs();

        public LevelElement SetAreaType(AreaType areaType)
        {
            AreaType = areaType;
            return this;
        }

        public LevelElement SetGrammarStyle(StyleSetter styleSetter)
        {
            styleSetter(CG());
            return this;
        }

        public LevelElement ApplyGrammarStyleRules(StyleRules styleRules)
        {
            styleRules.Apply(this);
            return this;
        }

        public abstract CubeGroup CG();

        #region Movement

        public class LEMoves
        {
            public LevelElement LE { get; }
            /// <summary>
            /// Has to be finite.
            /// </summary>
            public IEnumerable<Vector3Int> Ms { get; }

            public LEMoves(LevelElement le, IEnumerable<Vector3Int> ms)
            {
                LE = le;
                Ms = ms;
            }

            LEMoves Intersect(IEnumerable<Vector3Int> newMoves) => new LEMoves(LE, Ms.SetIntersect(newMoves));

            public LEMoves Where(Func<Vector3Int, bool> filter) => new LEMoves(LE, Ms.Where(filter));
            public LEMoves XZ() => Where(m => m.y == 0);

            public LEMoves Intersect(LevelElement toIntersect)
            {
                return Intersect(LE.MovesToIntersect(toIntersect).Ms);
            }

            public LEMoves DontIntersect(LevelElement toNotIntersect)
            {
                var moves = new HashSet<Vector3Int>(Ms);
                Intersect(toNotIntersect).Ms.ForEach(v => moves.Remove(v));
                return new LEMoves(LE, moves);
            }

            public LEMoves MovesNearXZ(LevelElement nearThis)
            {
                return Intersect(LE.MovesNearXZ(nearThis).Ms);
            }

            public LEMoves BeInside(LevelElement bounding)
            {
                return Intersect(LE.MovesToBeInside(bounding).Ms);
            }

            public LEMoves PartlyIntersectXZ(LevelElement toPartlyIntersect)
            {
                return Intersect(LE.MovesToPartlyIntersectXZ(toPartlyIntersect).Ms);
            }

            /// <summary>
            /// Dist should be at least 1.
            /// </summary>
            public LEMoves MovesInDistanceXZ(LevelElement toThis, int dist)
            {
                return Intersect(LE.MovesInDistanceXZ(toThis, dist).Ms);
            }

            /// <summary>
            /// Moves the level element by a random move and returns it.
            /// If no move exists, returns null.
            /// </summary>
            /// <param name="randomFromFirstCount">Cap the maximum number of moves for selection.</param>
            public LevelElement TryMove(int randomFromFirstCount = 10_000)
            {
                if (!Ms.Any())
                    return null;

                var move = Ms.Take(randomFromFirstCount).GetRandom();
                return LE.MoveBy(move);
            }
        }

        /// <summary>
        /// Moves of this level element to intersect the argument element.
        /// </summary>
        public LEMoves MovesToIntersect(LevelElement toIntersect)
        {
            return new LEMoves(this, toIntersect.CG().MinkowskiMinus(CG()));
        }

        public LEMoves MovesToBeInside(LevelElement bounding)
        {
            var intersectBounding = MovesToIntersect(bounding).Ms;

            var border = bounding.CG().ExtrudeAll().LE(AreaType.None);
            var intersectBorder = MovesToIntersect(border).Ms;
            return new LEMoves(this, intersectBounding.SetMinus(intersectBorder));
        }

        public LEMoves DontIntersect(IEnumerable<Vector3Int> possibleMoves, IEnumerable<LevelElement> toNotIntersect)
        {
            var moves = new HashSet<Vector3Int>(possibleMoves);
            toNotIntersect.ForEach(le => MovesToIntersect(le).Ms.ForEach(v => moves.Remove(v)));
            return new LEMoves(this, moves);
        }

        public LEMoves MovesNearXZ(LevelElement nearThis)
        {
            var intersectNear = nearThis.CG().MinkowskiMinus(CG().AllBoundaryFacesH().Extrude(1)).Where(move => move.y == 0);
            var intersect = MovesToIntersect(nearThis).Ms;
            return new LEMoves(this, intersectNear.SetMinus(intersect));
        }

        public LEMoves MovesToPartlyIntersectXZ(LevelElement partlyIntersectThis)
        {
            var intersectNear = partlyIntersectThis.CG().MinkowskiMinus(CG().AllBoundaryFacesH().Extrude(1, false));
            var intersect = MovesToIntersect(partlyIntersectThis).Ms;
            return new LEMoves(this, intersectNear.SetIntersect(intersect));
        }

        /// <summary>
        /// Dist should be at least 1.
        /// </summary>
        public LEMoves MovesInDistanceXZ(LevelElement toThis, int dist)
        {
            var cg = CG();
            var toThisCg = toThis.CG();
            var intersectClose = toThisCg.MinkowskiMinus(cg.AllBoundaryFacesH().Extrude(dist - 1, false).Merge(cg)).Where(move => move.y == 0);
            var intersectNear = toThisCg.MinkowskiMinus(cg.AllBoundaryFacesH().Extrude(dist, false)).Where(move => move.y == 0);
            return new LEMoves(this, intersectNear.SetMinus(intersectClose));
        }

        /// <summary>
        /// Returns all (infinitely many) possible moves so that this doesn't intersect toNotIntersect.
        /// </summary>
        public LEMoves MovesToNotIntersectXZ(IEnumerable<LevelElement> toNotIntersect)
        {
            var forbiddenMoves = new HashSet<Vector3Int>();
            toNotIntersect.ForEach(le => MovesToIntersect(le).Ms.ForEach(v => forbiddenMoves.Add(v)));
            return new LEMoves(this, ExtensionMethods.AllVectorsXZ().Where(move => !forbiddenMoves.Contains(move)));
        }

        #endregion

        /*
        public abstract LevelGroupElement Merge(params LevelElement[] le);
        */

        public abstract LevelElement Minus(LevelElement le);

        public abstract LevelElement MinusInPlace(LevelElement levelElement);

        public IEnumerable<LevelElement> NeighborsInDirection(Vector3Int dir, IEnumerable<LevelElement> possibleNeighbors)
        {
            var cubesInDir = CG().ExtrudeDir(dir);
            return possibleNeighbors.Where(le => le.CG().Intersects(cubesInDir));
        }

        public LevelGeometryElement ProjectToY(int y)
        {
            return new LevelGeometryElement(Grid, AreaType.None, Cubes().Select(cube => Grid[cube.Position.x, y, cube.Position.z]).Distinct().ToList().ToCubeGroup(Grid));
        }

        public abstract LevelElement Symmetrize(FaceHor faceHor);

        public abstract LevelGroupElement Merge(params LevelElement[] les);

        public abstract LevelGroupElement Split(Vector3Int dir, AreaType subareasType, params int[] dist);

        public abstract LevelGroupElement SplitRel(Vector3Int dir, AreaType subareasType, params float[] dist);

        public virtual string Print(int indent)
        {
            return $"{new string('\t', indent)}{AreaType.Name}({Cubes().Count})";
        }
    }

    public class LevelGroupElement : LevelElement
    {
        public List<LevelElement> LevelElements { get; private set; }

        public override List<Cube> Cubes() => LevelElements.SelectMany(le => le.Cubes()).ToList();

        public LevelGroupElement(Grid<Cube> grid, AreaType areaType, params LevelElement[] levelElements) : base(grid, areaType)
        {
            LevelElements = levelElements.ToList();
        }

        public LevelGroupElement(Grid<Cube> grid, AreaType areaType, List<LevelElement> levelElements) : base(grid, areaType)
        {
            Debug.Assert(grid != null, $"{AreaType.Name}");
            Debug.Assert(levelElements.All(le => le != null), $"{AreaType.Name}");
            LevelElements = levelElements.ToList();
        }

        public LevelGroupElement SetChildrenAreaType(AreaType areaType)
        {
            LevelElements.ForEach(le => le.SetAreaType(areaType));
            return this;
        }

        public override IEnumerable<LevelElement> Flatten() => LevelElements.SelectMany(le => le.Flatten()).Append(this);
        public override IEnumerable<LevelGeometryElement> Leafs() => LevelElements.SelectMany(le => le.Leafs());

        public LevelGroupElement Add(LevelElement levelElement) => new LevelGroupElement(Grid, AreaType, LevelElements.Append(levelElement).ToList());
        public LevelGroupElement AddAll(params LevelElement[] levelElement) => new LevelGroupElement(Grid, AreaType, LevelElements.Concat(levelElement).ToList());

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


        public override CubeGroup CG() => new CubeGroup(Grid, Cubes());

        public LevelGroupElement ReplaceLeafsGrp(Func<LevelGeometryElement, bool> cond, Func<LevelGeometryElement, LevelElement> replaceF) => (LevelGroupElement)ReplaceLeafs(cond, replaceF);
        public LevelGroupElement ReplaceLeafsGrp(int index, Func<LevelGeometryElement, LevelElement> replaceF)
        {
            var toReplace = Leafs().ElementAt(index);
            return (LevelGroupElement)ReplaceLeafs(le => le == toReplace, replaceF);
        }
        public LevelGroupElement ReplaceLeafsGrp(LevelGeometryElement replacedElement, Func<LevelGeometryElement, LevelElement> replaceF) => (LevelGroupElement)ReplaceLeafs(le => le == replacedElement, replaceF);

        public override LevelElement ReplaceLeafs(Func<LevelGeometryElement, bool> cond, Func<LevelGeometryElement, LevelElement> replaceF)
        {
            // todo: don't replace the group elements that aren't changed
            return new LevelGroupElement(Grid, AreaType, LevelElements.Select(le => le.ReplaceLeafs(cond, replaceF)).ToList());
        }

        public override LevelElement Minus(LevelElement levelElement)
        {
            // todo: don't replace the group elements that aren't changed
            return new LevelGroupElement(Grid, AreaType, LevelElements.Select(le => le.Minus(levelElement)).ToList());
        }

        public override LevelElement MinusInPlace(LevelElement levelElement)
        {
            LevelElements = LevelElements.Select(le => le.MinusInPlace(levelElement)).ToList();
            return this;
        }

        public LevelGroupElement MinusGrp(LevelElement levelElement)
        {
            return (LevelGroupElement)Minus(levelElement);
        }

        
        public override LevelGroupElement Merge(params LevelElement[] les)
        {
            return new LevelGroupElement(Grid, AreaType, LevelElements.Concat(les).ToList());
        }

        public override LevelElement Symmetrize(FaceHor faceHor)
        {
            return new LevelGroupElement(Grid, AreaType, LevelElements.Select(le => le.Symmetrize(faceHor)).ToList<LevelElement>());
        }

        public LevelGroupElement SymmetrizeGrp(FaceHor faceHor) => (LevelGroupElement)Symmetrize(faceHor);

        public override LevelGroupElement Split(Vector3Int dir, AreaType subareasType, params int[] dist)
        {
            return new LevelGroupElement(Grid, AreaType, LevelElements.Select(le => le.Split(dir, subareasType, dist)).ToList<LevelElement>());
        }

        public override LevelGroupElement SplitRel(Vector3Int dir, AreaType subareasType, params float[] dist)
        {
            return new LevelGroupElement(Grid, AreaType, LevelElements.Select(le => le.SplitRel(dir, subareasType, dist)).ToList<LevelElement>());
        }
        public LevelGroupElement Empty() => LevelElements.Where(le => le.AreaType == AreaType.Empty).ToLevelGroupElement(Grid);
        public LevelGroupElement NonEmpty() => LevelElements.Where(le => le.AreaType != AreaType.Empty).ToLevelGroupElement(Grid);

        public override string Print(int indent)
        {
            var sb = new StringBuilder();
            sb.AppendLine(base.Print(indent));
            LevelElements.ForEach(
                le =>
                {
                    sb.AppendLine(le.Print(indent + 1));
                });
            return sb.ToString();
        }
    }

    public class LevelGeometryElement : LevelElement
    {
        public CubeGroup Group { get; private set; }

        public override List<Cube> Cubes() => Group.Cubes;

        public LevelGeometryElement(Grid<Cube> grid, AreaType areaType, CubeGroup group) : base(grid, areaType)
        {
            Debug.Assert(grid != null, $"{AreaType.Name}");
            Group = group;
        }

        public override IEnumerable<LevelElement> Flatten() => new LevelElement[1] { this };
        public override IEnumerable<LevelGeometryElement> Leafs() => this.ToEnumerable();

        protected override LevelElement MoveByImpl(Vector3Int offset)
        {
            var movedGroup = Group.MoveBy(offset);
            return new LevelGeometryElement(Grid, AreaType, movedGroup);
        }

        public new LevelGeometryElement MoveBy(Vector3Int offset)
        {
            return (LevelGeometryElement)MoveByImpl(offset);
        }

        public override CubeGroup CG() => Group;

        public override LevelElement ReplaceLeafs(Func<LevelGeometryElement, bool> cond, Func<LevelGeometryElement, LevelElement> replaceF)
        {
            return cond(this) ? replaceF(this) : this;
        }

        public override LevelElement Minus(LevelElement levelElement)
        {
            // todo: don't materialize levelElement.CubeGroup() for every leaf
            return new LevelGeometryElement(Grid, AreaType, Group.Minus(levelElement.CG()));
        }

        public override LevelElement MinusInPlace(LevelElement levelElement)
        {
            Group = Group.Minus(levelElement.CG());
            return this;
        }

        public override LevelGroupElement Merge(params LevelElement[] le)
        {
            return new LevelGroupElement(Grid, AreaType.None, le.Prepend(this).ToList());
        }

        public override LevelElement Symmetrize(FaceHor faceHor)
        {
            return new LevelGeometryElement(Grid, AreaType, Group.Symmetrize(faceHor));
        }

        public override LevelGroupElement Split(Vector3Int dir, AreaType subareasType, params int[] dist)
        {
            return new LevelGroupElement(Grid, AreaType, Group.Split(dir, dist).Select(g => new LevelGeometryElement(Grid, subareasType, g)).ToList<LevelElement>());
        }

        public override LevelGroupElement SplitRel(Vector3Int dir, AreaType subareasType, params float[] dist)
        {
            return new LevelGroupElement(Grid, AreaType, Group.SplitRel(dir, dist).Select(g => new LevelGeometryElement(Grid, subareasType, g)).ToList<LevelElement>());
        }
    }
}

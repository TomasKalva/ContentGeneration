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
        public LevelGeometryElement MapGeom(Func<CubeGroup, CubeGroup> f) => new LevelGeometryElement(Grid, AreaType, f(CubeGroup()));

        #endregion

        public IEnumerable<LevelGeometryElement> Nonterminals(AreaType areaType) => Leafs().Where(g => g.AreaType == areaType);

        protected abstract LevelElement MoveByImpl(Vector3Int offset);

        public LevelElement MoveBy(Vector3Int offset)
        {
            return MoveByImpl(offset);
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
            styleSetter(CubeGroup());
            return this;
        }

        public LevelElement ApplyGrammarStyleRules(StyleRules styleRules)
        {
            styleRules.Apply(this);
            return this;
        }

        public abstract CubeGroup CubeGroup();

        #region Movement
        /// <summary>
        /// Moves of this level element to intersect the argument element.
        /// </summary>
        public IEnumerable<Vector3Int> MovesToIntersect(LevelElement toIntersect)
        {
            return toIntersect.CubeGroup().MinkowskiMinus(CubeGroup());
        }

        public IEnumerable<Vector3Int> MovesToBeInside(LevelElement bounding)
        {
            var intersectBounding = MovesToIntersect(bounding);

            var border = bounding.CubeGroup().ExtrudeAll().LevelElement(AreaType.None);
            var intersectBorder = MovesToIntersect(border);
            return intersectBounding.SetMinus(intersectBorder);
        }

        public IEnumerable<Vector3Int> Moves(IEnumerable<Vector3Int> possibleMoves, IEnumerable<LevelElement> toNotIntersect)
        {
            
            var moves = new HashSet<Vector3Int>(possibleMoves);
            toNotIntersect.ForEach(le => MovesToIntersect(le).ForEach(v => moves.Remove(v)));
            return moves;
        }

        public IEnumerable<Vector3Int> MovesNearXZ(LevelElement nearThis)
        {
            var intersectNear = nearThis.CubeGroup().MinkowskiMinus(CubeGroup().AllBoundaryFacesH().Extrude(1, false));
            var intersect = MovesToIntersect(nearThis);
            return intersectNear.SetMinus(intersect);
        }

        public IEnumerable<Vector3Int> MovesToPartlyIntersectXZ(LevelElement partlyIntersectThis)
        {
            var intersectNear = partlyIntersectThis.CubeGroup().MinkowskiMinus(CubeGroup().AllBoundaryFacesH().Extrude(1, false));
            var intersect = MovesToIntersect(partlyIntersectThis);
            return intersectNear.SetIntersect(intersect);
        }

        /// <summary>
        /// Dist should be at least 1.
        /// </summary>
        public IEnumerable<Vector3Int> MovesInDistanceXZ(LevelElement toThis, int dist)
        {
            var intersectClose = toThis.CubeGroup().MinkowskiMinus(CubeGroup().AllBoundaryFacesH().Extrude(dist, false));
            var intersectNear = toThis.CubeGroup().MinkowskiMinus(CubeGroup().AllBoundaryFacesH().Extrude(dist + 1, false));
            return intersectNear.SetMinus(intersectClose);
        }

        /// <summary>
        /// Returns all (infinitely many) possible moves so that this doesn't intersect toNotIntersect.
        /// </summary>
        public IEnumerable<Vector3Int> NotIntersecting(IEnumerable<LevelElement> toNotIntersect)
        {
            var forbiddenMoves = new HashSet<Vector3Int>();
            toNotIntersect.ForEach(le => MovesToIntersect(le).ForEach(v => forbiddenMoves.Add(v)));
            foreach (var move in ExtensionMethods.AllVectorsXZ())
            {
                if (!forbiddenMoves.Contains(move))
                {
                    yield return move;
                }
            }
        }

        #endregion

        /*
        public abstract LevelGroupElement Merge(params LevelElement[] le);
        */

        public abstract LevelElement Minus(LevelElement le);

        public abstract LevelElement MinusInPlace(LevelElement levelElement);

        public IEnumerable<LevelElement> NeighborsInDirection(Vector3Int dir, IEnumerable<LevelElement> possibleNeighbors)
        {
            var cubesInDir = CubeGroup().ExtrudeDir(dir);
            return possibleNeighbors.Where(le => le.CubeGroup().Intersects(cubesInDir));
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


        public override CubeGroup CubeGroup() => new CubeGroup(Grid, Cubes());

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

        public override CubeGroup CubeGroup() => Group;

        public override LevelElement ReplaceLeafs(Func<LevelGeometryElement, bool> cond, Func<LevelGeometryElement, LevelElement> replaceF)
        {
            return cond(this) ? replaceF(this) : this;
        }

        public override LevelElement Minus(LevelElement levelElement)
        {
            // todo: don't materialize levelElement.CubeGroup() for every leaf
            return new LevelGeometryElement(Grid, AreaType, Group.Minus(levelElement.CubeGroup()));
        }

        public override LevelElement MinusInPlace(LevelElement levelElement)
        {
            Group = Group.Minus(levelElement.CubeGroup());
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

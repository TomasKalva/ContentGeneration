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
        public Grid Grid { get; }
        public AreaType AreaType { get; set; }

        public abstract List<Cube> Cubes();

        public LevelElement(Grid grid, AreaType areaType)
        {
            Grid = grid;
            AreaType = areaType;
        }

        #region Collection methods
        public IEnumerable<LevelElement> WithAreaType(AreaType areaType) => Where(g => g.AreaType == areaType).ToList();

        public IEnumerable<LevelElement> Where(Func<LevelElement, bool> cond) => Flatten().Where(g => cond(g));

        public abstract LevelElement ReplaceLeafs(Func<LevelElement, bool> cond, Func<LevelElement, LevelElement> replaceF);

        #endregion

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
            Debug.Log("Moves executed");
            toNotIntersect.ForEach(le => MovesToIntersect(le).ForEach(v => moves.Remove(v)));
            return moves;
        }

        public LevelGroupElement Merge(LevelElement le, AreaType areaType = null)
        {
            return new LevelGroupElement(Grid, areaType ?? AreaType.None, this, le);
        }

        public IEnumerable<LevelElement> NeighborsInDirection(Vector3Int dir, IEnumerable<LevelElement> possibleNeighbors)
        {
            var cubesInDir = CubeGroup().ExtrudeDir(dir);
            return possibleNeighbors.Where(le => le.CubeGroup().Intersects(cubesInDir));
        }
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


        public override CubeGroup CubeGroup() => new CubeGroup(Grid, Cubes());

        public LevelGroupElement ReplaceLeafsGrp(Func<LevelElement, bool> cond, Func<LevelElement, LevelElement> replaceF) => (LevelGroupElement)ReplaceLeafs(cond, replaceF);

        public override LevelElement ReplaceLeafs(Func<LevelElement, bool> cond, Func<LevelElement, LevelElement> replaceF)
        {
            // todo: don't replace the group elements that aren't changed
            return new LevelGroupElement(Grid, AreaType, LevelElements.Select(le => le.ReplaceLeafs(cond, replaceF)).ToList());
        }
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

        public override LevelElement ReplaceLeafs(Func<LevelElement, bool> cond, Func<LevelElement, LevelElement> replaceF)
        {
            return cond(this) ? replaceF(this) : this;
        }
    }
}

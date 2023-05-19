﻿using OurFramework.UI;
using OurFramework.Environment.StylingAreas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using OurFramework.Util;
using OurFramework.Game;

namespace OurFramework.Environment.GridMembers
{
    /// <summary>
    /// Hierarchical structure for storing cubes. It is immutable.
    /// Uses fluent interface pattern.
    /// </summary>
    public abstract class LevelElement
    {
        public Grid<Cube> Grid { get; }
        public AreaStyle AreaStyle { get; set; }

        public abstract List<Cube> Cubes();

        public LevelElement(Grid<Cube> grid, AreaStyle areaType)
        {
            Grid = grid;
            AreaStyle = areaType;
        }

        /// <summary>
        /// Create a new empty level element.
        /// </summary>
        public static LevelElement Empty(Grid<Cube> grid) => new LevelGeometryElement(grid, AreaStyles.None(), new CubeGroup(grid, new List<Cube>()));

        #region Collection methods
        /// <summary>
        /// All subelements with areaType.
        /// </summary>
        public IEnumerable<LevelElement> WithAreaType(AreaStyle areaType) => Where(g => g.AreaStyle == areaType).LevelElements.ToList();

        /// <summary>
        /// All subelements satisfying cond.
        /// </summary>
        public LevelGroupElement Where(Func<LevelElement, bool> cond) => Flatten().Where(g => cond(g)).ToLevelGroupElement(Grid);
        /// <summary>
        /// All leafs that satisfy cond.
        /// </summary>
        public LevelGroupElement WhereGeom(Func<LevelElement, bool> cond) => Leafs().Where(g => cond(g)).ToLevelGroupElement(Grid);

        /// <summary>
        /// Replaces leafs.
        /// </summary>
        public abstract LevelElement ReplaceLeafs(Func<LevelGeometryElement, bool> cond, Func<LevelGeometryElement, LevelElement> replaceF);
        /// <summary>
        /// New level element with the same cubes.
        /// </summary>
        public LevelGeometryElement MapGeom(Func<CubeGroup, CubeGroup> f) => new LevelGeometryElement(Grid, AreaStyle, f(CG()));

        #endregion

        protected abstract LevelElement MoveByImpl(Vector3Int offset);

        public LevelElement MoveBy(Vector3Int offset)
        {
            return MoveByImpl(offset);
        }

        /// <summary>
        /// Move bottom by yOffset, but not below minY.
        /// </summary>
        public LevelElement MoveBottomBy(int yOffset, int minY)
        {
            var cg = CG();
            if (!cg.Cubes.Any())
                return this;

            var bottom = cg.LeftBottomBack().y;
            var newBottom = Math.Max(minY, bottom + yOffset);
            return MoveBy((newBottom - bottom) * Vector3Int.up);
        }

        /// <summary>
        /// Move bottom to yPosition.
        /// </summary>
        public LevelElement MoveBottomTo(int yPosition)
        {
            var cg = CG();
            if (!cg.Cubes.Any())
                return this;

            var bottom = cg.LeftBottomBack().y;
            return MoveBy((yPosition - bottom) * Vector3Int.up);
        }

        /// <summary>
        /// Returns all subelements.
        /// </summary>
        public abstract IEnumerable<LevelElement> Flatten();

        /// <summary>
        /// Returns all geometry elements in leafs.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<LevelGeometryElement> Leafs();

        public LevelElement SetAreaStyle(AreaStyle areaType)
        {
            AreaStyle = areaType;
            return this;
        }

        /// <summary>
        /// Apply style on these cubes.
        /// </summary>
        public LevelElement ApplyStyle()
        {
            AreaStyle.ApplyStyle(CG());
            return this;
        }

        /// <summary>
        /// Apply style to all leafs.
        /// </summary>
        public LevelElement ApplyGrammarStyles()
        {
            Leafs().ForEach(le => le.ApplyStyle());
            return this;
        }

        /// <summary>
        /// Creates special geometry.
        /// </summary>
        public LevelElement CreateGeometry(IGridGeometryOwner geometryOwner)
        {
            Leafs().ForEach(
                le =>
                {
                    var elemntsMaker = le.CG().MakeArchitectureElements;
                    if(elemntsMaker != null)
                    {
                        geometryOwner.AddArchitectureElement(elemntsMaker());
                    }
                });
            return this;
        }

        public abstract CubeGroup CG();

        #region Movement

        /// <summary>
        /// Set of moves used for positioning of level elements.
        /// </summary>
        public class LEMoves
        {
            public LevelElement LE { get; }
            /// <summary>
            /// All possible moves. Has to be finite.
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
            /// Returns null if no moves exist.
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

        /// <summary>
        /// Moves to be inside the bounding element.
        /// </summary>
        public LEMoves MovesToBeInside(LevelElement bounding)
        {
            var intersectBounding = MovesToIntersect(bounding).Ms;

            var border = bounding.CG().ExtrudeAll().LE(AreaStyles.None());
            var intersectBorder = MovesToIntersect(border).Ms;
            return new LEMoves(this, intersectBounding.SetMinus(intersectBorder));
        }

        /// <summary>
        /// Moves to not intereset given level elements.
        /// </summary>
        public LEMoves DontIntersect(IEnumerable<Vector3Int> possibleMoves, IEnumerable<LevelElement> toNotIntersect)
        {
            var moves = new HashSet<Vector3Int>(possibleMoves);
            toNotIntersect.ForEach(le => MovesToIntersect(le).Ms.ForEach(v => moves.Remove(v)));
            return new LEMoves(this, moves);
        }

        /// <summary>
        /// Moves to be near nearThis.
        /// </summary>
        public LEMoves MovesNearXZ(LevelElement nearThis)
        {
            var intersectNear = nearThis.CG().MinkowskiMinus(CG().AllBoundaryFacesH().Extrude(1)).Where(move => move.y == 0);
            var intersect = MovesToIntersect(nearThis).Ms;
            return new LEMoves(this, intersectNear.SetMinus(intersect));
        }

        /// <summary>
        /// Moves to partly intersect the given element.
        /// </summary>
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
            var intersectClose = toThisCg.MinkowskiMinus(cg.ExtrudeHorOut(dist - 1, false).Merge(cg)).Where(move => move.y == 0);
            var intersectNear = toThisCg.MinkowskiMinus(cg.ExtrudeHorOut(dist, false)).Where(move => move.y == 0);
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

        public abstract LevelElement Minus(LevelElement le);

        public abstract LevelElement MinusInPlace(LevelElement levelElement);

        public IEnumerable<LevelElement> NeighborsInDirection(Vector3Int dir, IEnumerable<LevelElement> possibleNeighbors)
        {
            var cubesInDir = CG().ExtrudeDir(dir);
            return possibleNeighbors.Where(le => le.CG().Intersects(cubesInDir));
        }

        public LevelGeometryElement ProjectToY(int y)
        {
            return new LevelGeometryElement(Grid, AreaStyles.None(), Cubes().Select(cube => Grid[cube.Position.x, y, cube.Position.z]).Distinct().ToList().ToCubeGroup(Grid));
        }

        public abstract LevelElement Symmetrize(FaceHor faceHor);

        public abstract LevelGroupElement Merge(params LevelElement[] les);

        /// <summary>
        /// Split this element by planes with normal dir.
        /// </summary>
        public abstract LevelGroupElement Split(Vector3Int dir, AreaStyle subareasType, params int[] dist);

        /// <summary>
        /// Split this element by planes with normal dir. Distances are relative.
        /// </summary>
        public abstract LevelGroupElement SplitRel(Vector3Int dir, AreaStyle subareasType, params float[] dist);

        public virtual string Print(int indent)
        {
            return $"{new string('\t', indent)}{AreaStyle.Name}({Cubes().Count})";
        }
    }

    /// <summary>
    /// Group of level elements.
    /// </summary>
    public class LevelGroupElement : LevelElement
    {
        public List<LevelElement> LevelElements { get; private set; }

        public override List<Cube> Cubes() => LevelElements.SelectMany(le => le.Cubes()).ToList();

        public LevelGroupElement(Grid<Cube> grid, AreaStyle areaType, params LevelElement[] levelElements) : base(grid, areaType)
        {
            LevelElements = levelElements.ToList();
        }

        public LevelGroupElement(Grid<Cube> grid, AreaStyle areaType, List<LevelElement> levelElements) : base(grid, areaType)
        {
            Debug.Assert(grid != null, $"{AreaStyle.Name}");
            Debug.Assert(levelElements.All(le => le != null), $"{AreaStyle.Name}");
            LevelElements = levelElements.ToList();
        }

        public LevelGroupElement SetChildrenAreaType(AreaStyle areaType)
        {
            LevelElements.ForEach(le => le.SetAreaStyle(areaType));
            return this;
        }

        public override IEnumerable<LevelElement> Flatten() => LevelElements.SelectMany(le => le.Flatten()).Append(this);
        public override IEnumerable<LevelGeometryElement> Leafs() => LevelElements.SelectMany(le => le.Leafs());

        public LevelGroupElement Add(LevelElement levelElement) => new LevelGroupElement(Grid, AreaStyle, LevelElements.Append(levelElement).ToList());
        public LevelGroupElement AddAll(params LevelElement[] levelElement) => new LevelGroupElement(Grid, AreaStyle, LevelElements.Concat(levelElement).ToList());

        protected override LevelElement MoveByImpl(Vector3Int offset)
        {
            var movedGroups = LevelElements.Select(le => le.MoveBy(offset)).ToList();
            return new LevelGroupElement(Grid, AreaStyle, movedGroups);
        }

        public new LevelGroupElement MoveBy(Vector3Int offset)
        {
            return (LevelGroupElement)MoveByImpl(offset);
        }

        public LevelGroupElement Select(Func<LevelElement, LevelElement> selector) => new LevelGroupElement(Grid, AreaStyle, LevelElements.Select(selector).ToList());


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
            return new LevelGroupElement(Grid, AreaStyle, LevelElements.Select(le => le.ReplaceLeafs(cond, replaceF)).ToList());
        }

        public override LevelElement Minus(LevelElement levelElement)
        {
            // todo: don't replace the group elements that aren't changed
            return new LevelGroupElement(Grid, AreaStyle, LevelElements.Select(le => le.Minus(levelElement)).ToList());
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
            return new LevelGroupElement(Grid, AreaStyle, LevelElements.Concat(les).ToList());
        }

        public override LevelElement Symmetrize(FaceHor faceHor)
        {
            return new LevelGroupElement(Grid, AreaStyle, LevelElements.Select(le => le.Symmetrize(faceHor)).ToList());
        }

        public LevelGroupElement SymmetrizeGrp(FaceHor faceHor) => (LevelGroupElement)Symmetrize(faceHor);

        public override LevelGroupElement Split(Vector3Int dir, AreaStyle subareasType, params int[] dist)
        {
            return new LevelGroupElement(Grid, AreaStyle, LevelElements.Select(le => le.Split(dir, subareasType, dist)).ToList<LevelElement>());
        }

        public override LevelGroupElement SplitRel(Vector3Int dir, AreaStyle subareasType, params float[] dist)
        {
            return new LevelGroupElement(Grid, AreaStyle, LevelElements.Select(le => le.SplitRel(dir, subareasType, dist)).ToList<LevelElement>());
        }
        public LevelGroupElement Empty() => LevelElements.Where(le => le.AreaStyle == AreaStyles.Empty()).ToLevelGroupElement(Grid);
        public LevelGroupElement NonEmpty() => LevelElements.Where(le => le.AreaStyle != AreaStyles.Empty()).ToLevelGroupElement(Grid);

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

        public LevelGeometryElement(Grid<Cube> grid, AreaStyle areaType, CubeGroup group) : base(grid, areaType)
        {
            Debug.Assert(grid != null, $"{AreaStyle.Name}");
            Group = group;
        }

        public override IEnumerable<LevelElement> Flatten() => new LevelElement[1] { this };
        public override IEnumerable<LevelGeometryElement> Leafs() => this.ToEnumerable();

        protected override LevelElement MoveByImpl(Vector3Int offset)
        {
            var movedGroup = Group.MoveBy(offset);
            return new LevelGeometryElement(Grid, AreaStyle, movedGroup);
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
            return new LevelGeometryElement(Grid, AreaStyle, Group.Minus(levelElement.CG()));
        }

        public override LevelElement MinusInPlace(LevelElement levelElement)
        {
            Group = Group.Minus(levelElement.CG());
            return this;
        }

        public override LevelGroupElement Merge(params LevelElement[] le)
        {
            return new LevelGroupElement(Grid, AreaStyles.None(), le.Prepend(this).ToList());
        }

        public override LevelElement Symmetrize(FaceHor faceHor)
        {
            return new LevelGeometryElement(Grid, AreaStyle, Group.Symmetrize(faceHor));
        }

        public override LevelGroupElement Split(Vector3Int dir, AreaStyle subareasType, params int[] dist)
        {
            return new LevelGroupElement(Grid, AreaStyle, Group.Split(dir, dist).Select(g => new LevelGeometryElement(Grid, subareasType, g)).ToList<LevelElement>());
        }

        public override LevelGroupElement SplitRel(Vector3Int dir, AreaStyle subareasType, params float[] dist)
        {
            return new LevelGroupElement(Grid, AreaStyle, Group.SplitRel(dir, dist).Select(g => new LevelGeometryElement(Grid, subareasType, g)).ToList<LevelElement>());
        }
    }
}
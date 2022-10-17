using OurFramework.Environment.GridMembers;
using OurFramework.Environment.StylingAreas;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OurFramework.Environment.ShapeCreation
{
    /// <summary>
    /// D E P R E C A T E D
    /// and doesnt throw correct exceptions
    /// </summary>
    public class Placement
    {
        Grid<Cube> Grid { get; }

        public Placement(Grid<Cube> grid)
        {
            Grid = grid;
        }

        public delegate IEnumerable<Vector3Int> ValidMoves(IEnumerable<LevelElement> alreadyMoved, LevelElement toMove);
        public delegate Vector3Int MoveSelector(IEnumerable<Vector3Int> allMoves);

        /// <summary>
        /// D E P R E C A T E D
        /// </summary>
        public LevelGroupElement MoveLevelGroup(LevelGroupElement levelGroupElement, ValidMoves validMovesF, MoveSelector moveSelector)
        {
            var movedElements = levelGroupElement.LevelElements.Aggregate((IEnumerable<LevelElement>)new List<LevelElement>(), (moved, le) =>
            {
                var validMoves = validMovesF(moved, le);

                if (validMoves.Any())
                {
                    moved = moved.Append(le.MoveBy(moveSelector(validMoves)));
                }
                return moved;
            });
            return new LevelGroupElement(levelGroupElement.Grid, AreaStyles.None(), movedElements.ToList());
        }

        /// <summary>
        /// D E P R E C A T E D
        /// </summary>
        public LevelGroupElement MoveToNotOverlap(LevelGroupElement levelGroupElement)
        {
            return MoveLevelGroup(levelGroupElement, (moved, le) => le.MovesToNotIntersectXZ(moved).Ms, moves => moves.FirstOrDefault());
        }

        /// <summary>
        /// D E P R E C A T E D
        /// </summary>
        public LevelElement MoveToNotOverlap(LevelElement fixedElement, LevelElement toMove)
        {
            var bothMoved = MoveLevelGroup(new LevelGroupElement(fixedElement.Grid, AreaStyles.None(), fixedElement, toMove), (moved, le) => le.MovesToNotIntersectXZ(moved).Ms, moves => moves.FirstOrDefault());
            return bothMoved.LevelElements[1];
        }

        /// <summary>
        /// D E P R E C A T E D
        /// </summary>
        public LevelGroupElement MoveToIntersect(LevelGroupElement levelGroupElement)
        {
            return MoveLevelGroup(levelGroupElement, 
                (moved, le) => moved.Any() ? 
                    le.MovesToIntersect(moved.ToLevelGroupElement(levelGroupElement.Grid)).Ms :
                    Vector3Int.zero.ToEnumerable(), 
                moves => moves.GetRandom());
        }

        /// <summary>
        /// D E P R E C A T E D
        /// </summary>
        public LevelGroupElement MoveToIntersectAll(LevelGroupElement levelGroupElement)
        {
            return MoveLevelGroup(levelGroupElement,
                (moved, le) => moved.Any() ?
                    moved.IntersectMany(movedLe => le.MovesToIntersect(movedLe).Ms) :
                    Vector3Int.zero.ToEnumerable(),
                moves => moves.GetRandom());
        }

        /// <summary>
        /// D E P R E C A T E D
        /// </summary>
        public LevelElement MoveNearXZ(LevelElement fixedElement, LevelElement toMove, LevelElement notIntersect)
        {

            var movesNear = toMove.MovesNearXZ(fixedElement).Ms;
            var movesNotIntersecting = toMove.DontIntersect(movesNear, notIntersect.ToEnumerable()).Ms;
            return movesNotIntersecting.Any() ? toMove.MoveBy(movesNotIntersecting.GetRandom()) : null;
        }

        /// <summary>
        /// D E P R E C A T E D
        /// </summary>
        public LevelGroupElement SurroundWith(LevelElement levelElement, LevelGroupElement surroundings)
        {
            return MoveLevelGroup(surroundings,
                (moved, le) =>
                {
                    var movesNear = le.MovesNearXZ(levelElement).Ms;
                    var movesNotIntersecting = le.DontIntersect(movesNear, moved).Ms;
                    return movesNotIntersecting;
                },
                moves => moves.GetRandom()
                );
        }

        /// <summary>
        /// D E P R E C A T E D
        /// </summary>
        public LevelGroupElement PlaceInside(LevelElement bounds, LevelGroupElement shapes)
        {
            return MoveLevelGroup(shapes,
                (moved, le) =>
                {
                    return le.MovesToBeInside(bounds).Ms;
                },
                moves => moves.GetRandom()
                );
        }
    }
}

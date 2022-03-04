using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class Placement
    {
        Grid Grid { get; }

        public Placement(Grid grid)
        {
            Grid = grid;
        }

        public delegate IEnumerable<Vector3Int> ValidMoves(IEnumerable<LevelElement> alreadyMoved, LevelElement toMove);
        public delegate Vector3Int MoveSelector(IEnumerable<Vector3Int> allMoves);

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
            return new LevelGroupElement(levelGroupElement.Grid, AreaType.None, movedElements.ToList());
        }

        public LevelGroupElement MoveToNotOverlap(LevelGroupElement levelGroupElement)
        {
            return MoveLevelGroup(levelGroupElement, (moved, le) => le.NotIntersecting(moved), moves => moves.FirstOrDefault());
        }

        public LevelElement MoveToNotOverlap(LevelElement fixedElement, LevelElement moved)
        {
            var bothMoved = MoveLevelGroup(fixedElement.Merge(moved), (moved, le) => le.NotIntersecting(moved), moves => moves.FirstOrDefault());
            return bothMoved.LevelElements[1];
        }

        public LevelGroupElement MoveToIntersect(LevelGroupElement levelGroupElement)
        {
            return MoveLevelGroup(levelGroupElement, 
                (moved, le) => moved.Any() ? 
                    le.MovesToIntersect(moved.ToLevelGroupElement(levelGroupElement.Grid)) :
                    Vector3Int.zero.ToEnumerable(), 
                moves => moves.GetRandom());
        }

        public LevelGroupElement MoveToIntersectAll(LevelGroupElement levelGroupElement)
        {
            return MoveLevelGroup(levelGroupElement,
                (moved, le) => moved.Any() ?
                    moved.IntersectMany(movedLe => le.MovesToIntersect(movedLe)) :
                    Vector3Int.zero.ToEnumerable(),
                moves => moves.GetRandom());
        }

        public LevelGroupElement SurroundWith(LevelElement levelElement, LevelGroupElement surroundings)
        {
            return MoveLevelGroup(surroundings,
                (moved, le) =>
                {
                    var movesNear = le.MovesNearXZ(levelElement);
                    var movesNotIntersecting = le.Moves(movesNear, moved);
                    return movesNotIntersecting;
                },
                moves => moves.GetRandom()
                );
        }

        public LevelGroupElement PlaceInside(LevelElement bounds, LevelGroupElement shapes)
        {
            return MoveLevelGroup(shapes,
                (moved, le) =>
                {
                    return le.MovesToBeInside(bounds);
                },
                moves => moves.GetRandom()
                );
        }
    }
}

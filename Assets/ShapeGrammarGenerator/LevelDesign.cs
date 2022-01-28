using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public abstract class LevelDesign
    {
        public LevelDevelopmentKit ldk { get; }

        protected LevelDesign(LevelDevelopmentKit ldk)
        {
            this.ldk = ldk;
        }

        public abstract LevelElement CreateLevel();
    }

    public class ControlPointsLevelDesign : LevelDesign
    {
        public ControlPointsLevelDesign(LevelDevelopmentKit ldk) : base(ldk)
        {
        }

        public override LevelElement CreateLevel()
        {
            var root = new LevelGroupElement(ldk.grid, AreaType.WorldRoot);

            // Create ground layout of the tower
            var yard = ldk.qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(50, 50)).InflateY(0, 1));

            var controlPoints = ControlPoints(yard, 10);
            var controlLine = ConnectByLine(controlPoints).LevelElement(AreaType.Debug);

            var boxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(new Vector2Int(3, 3), new Vector2Int(7, 7)));
            var houses = ldk.qc.FlatBoxes(boxSequence, 20).Select(le => le.SetAreaType(AreaType.House));

            houses = houses.LevelElements.Aggregate((IEnumerable<LevelElement>)new List<LevelElement>(),
                (moved, le) =>
                {
                    var prev = moved.Any() ? moved.FirstOrDefault() : controlPoints.Cubes.FirstOrDefault().Group().LevelElement();
                    //var movesToPrev = le.MovesNearXZ(prev);
                    var movesOnLine = le.MovesToIntersect(controlLine);
                    var possibleMoves = le.Moves(/*movesToPrev.SetIntersect(*/movesOnLine/*)*/, moved);
                    if (possibleMoves.Any())
                    {
                        moved = moved.Append(le.MoveBy(possibleMoves.GetRandom()));
                    }
                    return moved;
                }).ToLevelGroupElement();

            houses = houses.ReplaceLeafsGrp(g => g.AreaType == AreaType.House, g => ldk.sgShapes.House(g.CubeGroup(), 3));

            root = root.AddAll(/*controlLine, controlPoints.MoveBy(Vector3Int.up).LevelElement(AreaType.Debug),*/ houses);
            root.ApplyGrammarStyleRules(ldk.houseStyleRules);

            return root;
        }

        public CubeGroup ControlPoints(CubeGroup bounds, int count)
        {
            var controlPoints = bounds.Cubes.Shuffle().Take(count).ToList();
            int iters = 0;

            RemoveIntersection:
            for(int i = 0; i < controlPoints.Count - 1; i++)
            {
                var p0 = controlPoints[i].Position;
                var p1 = controlPoints[i + 1].Position;
                for (int j = 0; j < controlPoints.Count - 1; j++)
                {
                    var p2 = controlPoints[j].Position;
                    var p3 = controlPoints[j + 1].Position;
                    if(ExtensionMethods.LinesIntersects(
                        p0.XZ(),
                        p1.XZ(),
                        p2.XZ(),
                        p3.XZ()))
                    {
                        controlPoints.Swap(i + 1, j);

                        if (iters++ > 1000)
                        {
                            break;
                        }
                        goto RemoveIntersection;
                    }
                }
            }
            Debug.Log($"Iterations: {iters}");

            return controlPoints.ToCubeGroup(bounds.Grid);
        }

        public CubeGroup ConnectByLine(CubeGroup controlPoints)
        {
            return controlPoints.Cubes.Select2((c0, c1) => c0.LineTo(c1)).ToCubeGroup();
        }
    }
}

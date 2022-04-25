using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{

    public class ControlPointsLevelDesign : LevelDesign
    {
        public ControlPointsLevelDesign(LevelDevelopmentKit ldk) : base(ldk)
        {
        }

        public override LevelElement CreateLevel()
        {
            var root = new LevelGroupElement(ldk.grid, AreaType.WorldRoot);

            // Create ground layout of the tower
            var yard = ldk.qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(30, 30)).InflateY(0, 1));

            var controlPoints = ControlPoints(yard, 10);
            var controlLine = ConnectByLine(controlPoints).LE(AreaType.Debug);

            var smallDistr = new SlopeDistr(center: 6, width: 3, rightness: 0.3f);
            //var boxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(new Vector2Int(3, 3), new Vector2Int(8, 8)));
            var smallBoxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(smallDistr, smallDistr));
            var largeDistr = new SlopeDistr(center: 10, width: 3, rightness: 0.3f);
            var largeBoxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(largeDistr, largeDistr));
            var houses = ldk.qc.FlatBoxes(smallBoxSequence.Take(7).Concat(largeBoxSequence).Take(10).Shuffle(), 10).Select(le => le.SetAreaType(AreaType.House));
            //houses = houses.ReplaceLeafsGrp(g => g.AreaType == AreaType.House, g => ldk.sgShapes.CompositeHouse(4));

            houses = ldk.pl.MoveLevelGroup(houses,
                (moved, le) =>
                {
                    //var prev = moved.Any() ? moved.FirstOrDefault() : controlPoints.Cubes.FirstOrDefault().Group().LevelElement();
                    //var movesToPrev = le.MovesNearXZ(prev);
                    var movesOnLine = le.MovesToIntersect(controlLine).Ms;
                    var possibleMoves = le.DontIntersect(/*movesToPrev.SetIntersect(*/movesOnLine/*)*/, moved).Ms;
                    return possibleMoves;
                },
                moves => moves.GetRandom());

            houses = houses.ReplaceLeafsGrp(g => g.AreaType == AreaType.House, g => ldk.sgShapes.SimpleHouseWithFoundation(g.CG(), UnityEngine.Random.Range(3, 7)));

            root = root.AddAll(/*controlLine, controlPoints.MoveBy(Vector3Int.up).LevelElement(AreaType.Debug),*/ houses);
            root.ApplyGrammarStyleRules(ldk.houseStyleRules);

            return root;
        }

        public CubeGroup ControlPoints(CubeGroup bounds, int count)
        {
            var controlPoints = bounds.Cubes.Shuffle().Take(count).ToList();
            //controlPoints = controlPoints.Select(cube => cube.Grid[cube.Position + Vector3Int.up * UnityEngine.Random.Range(0, 10)]).ToList();
            int iters = 0;

            RemoveIntersection:
            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                var p0 = controlPoints[i].Position;
                var p1 = controlPoints[i + 1].Position;
                for (int j = 0; j < controlPoints.Count - 1; j++)
                {
                    var p2 = controlPoints[j].Position;
                    var p3 = controlPoints[j + 1].Position;
                    if (ExtensionMethods.LinesIntersects(
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

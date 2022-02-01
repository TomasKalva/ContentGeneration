using System.Linq;
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
            var yard = ldk.qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(30, 30)).InflateY(0, 1));

            var controlPoints = ControlPoints(yard, 10);
            var controlLine = ConnectByLine(controlPoints).LevelElement(AreaType.Debug);

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
                    var movesOnLine = le.MovesToIntersect(controlLine);
                    var possibleMoves = le.Moves(/*movesToPrev.SetIntersect(*/movesOnLine/*)*/, moved);
                    return possibleMoves;
                },
                moves => moves.GetRandom());

            houses = houses.ReplaceLeafsGrp(g => g.AreaType == AreaType.House, g => ldk.sgShapes.SimpleHouseWithFoundation(g.CubeGroup(), UnityEngine.Random.Range(3, 7)));

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

    
    public class CurvesLevelDesign : LevelDesign
    {
        public CurvesLevelDesign(LevelDevelopmentKit ldk) : base(ldk)
        {
        }

        public override LevelElement CreateLevel()
        {
            int length = 30;
            // Height
            var heightDistr = new UniformDistr(5, 15);
            var heightCurve = ExtensionMethods.Naturals().Select(_ => heightDistr.Sample());
            var smooth = heightCurve.Select2((a,b) => (a + b) / 2);
            heightCurve = heightCurve.Interleave(smooth).Take(length);

            // Size
            var smallDistr = new SlopeDistr(center: 5, width: 3, rightness: 0.3f);
            var smallBoxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(smallDistr, smallDistr));
            var largeDistr = new SlopeDistr(center: 8, width: 3, rightness: 0.3f);
            var largeBoxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(largeDistr, largeDistr));

            // Type
            var areaTypeCurve = ExtensionMethods.Naturals().Select(i => UnityEngine.Random.Range(0f, 1f) < 0.5f ? AreaType.House : AreaType.Garden);

            var areas = ldk.qc.FlatBoxes(smallBoxSequence.Take((int)(0.7f * length)).Concat(largeBoxSequence).Take(length).Shuffle(), length).Select(le => le.SetAreaType(AreaType.Room));

            Debug.Log($"Cubes count {areas.Cubes().Count}");

            areas = ldk.pl.MoveLevelGroup(areas,
                (moved, le) =>
                {
                    var prev = moved.Any() ? moved.LastOrDefault() : areas.LevelElements.FirstOrDefault();
                    var movesToPrev = le.MovesNearXZ(prev);
                    //var movesOnLine = le.MovesToIntersect(controlLine);
                    var possibleMoves = le.Moves(movesToPrev, moved);
                    return possibleMoves;
                },
                moves => moves.GetRandom());

            //areas = ldk.pl.MoveToNotOverlap(areas);

            /*areas = areas.LevelElements.Select((le, i) =>
            {
                return le.SetAreaType(areaTypeCurve.ElementAt(i))
                    .MoveBy(Vector3Int.up * heightCurve.ElementAt(i));
            }).ToLevelGroupElement(areas.Grid);
            
            areas = areas.ReplaceLeafsGrp(g => g.AreaType == AreaType.House, g => ldk.sgShapes.SimpleHouseWithFoundation(g.CubeGroup(), 4));
            */

            Debug.Log($"After moving cubes count {areas.Cubes().Count()}");
            Debug.Log($"Distinct cubes count {areas.Cubes().Distinct().Count()}");

            areas.ApplyGrammarStyleRules(ldk.houseStyleRules);

            return areas;
            /*
            var sizeCurve =
            var areaCreatorCurve =
            var widenessCurve =
            */
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using static ShapeGrammar.WorldState;

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

        ChangeWorld AddNearXZ(Func<LevelElement> elementF)
        {
            return (addingState) =>
            {
                var element = elementF();
                var possibleMoves = element.Moves(element.MovesNearXZ(addingState.Current).Where(m => m.y == 0), addingState.Added.LevelElements);
                var movedElement = possibleMoves.Any() ? element.MoveBy(possibleMoves.GetRandom()) : null;
                return addingState.TryPush(movedElement);
            };
        }

        ChangeWorld AddRemoveOverlap(Func<LevelElement> elementF)
        {
            return (addingState) =>
            {
                var element = elementF();
                var possibleMoves = element.Moves(element.MovesToPartlyIntersectXZ(addingState.Current.Where(le => le.AreaType != AreaType.Path)).Where(m => m.y == 0), addingState.Added.LevelElements.Others(addingState.Current));
                var movedElement = possibleMoves.Any() ? element.MoveBy(possibleMoves.GetRandom()).Minus(addingState.Current) : null;
                return addingState.TryPush(movedElement);
            };
        }
        
        ChangeWorld PathTo(Func<LevelElement> elementF)
        {
            return (addingState) =>
            {
                var element = elementF();
                var possibleMoves = element.Moves(element.MovesInDistanceXZ(addingState.Current, 5).Where(m => m.y == 0), addingState.Added.LevelElements);
                if (!possibleMoves.Any())
                    return addingState;

                var area = element.MoveBy(possibleMoves.GetRandom());
                var start = addingState.Current.WhereGeom(le => le.AreaType != AreaType.Path);
                var pathCG = ldk.paths.PathH(start, area, 2, addingState.Added).CubeGroup();
                var path = pathCG.ExtrudeVer(Vector3Int.up, 2).Merge(pathCG).LevelElement(AreaType.Platform);
                var newElement = new LevelGroupElement(addingState.Current.Grid, AreaType.None, path, area);
                return addingState.TryPush(newElement);
            };
        }

        ChangeWorld SubdivideRoom()
        {
            return (addingState) =>
            {
                var room = addingState.Added.Nonterminals(AreaType.Room)
                    .Where(room => room.CubeGroup().Extents().AtLeast(new Vector3Int(3, 2, 3)))
                    .FirstOrDefault();
                if (room == null)
                    return addingState;

                var changedAdded = addingState.Added.ReplaceLeafsGrp(room, _ => ldk.tr.SubdivideRoom(room, ExtensionMethods.HorizontalDirections().GetRandom(), 0.3f));
                return addingState.ChangeAdded(changedAdded);
            };
        }

        ChangeWorld SplitToFloors()
        {
            return (addingState) =>
            {
                var room = addingState.Added.Nonterminals(AreaType.Room)
                    .Where(room => room.CubeGroup().Extents().AtLeast(new Vector3Int(5, 5, 5)))
                    .FirstOrDefault();
                if (room == null)
                    return addingState;

                var changedAdded = addingState.Added.ReplaceLeafsGrp(room, _ => ldk.tr.FloorHouse(room, ldk.tr.BrokenFloor, 3, 6, 9));
                return addingState.ChangeAdded(changedAdded);
            };
        }

        
        ChangeWorld ConnectTwoUnconnected(Graph<LevelGeometryElement> connectednessGraph, LevelGroupElement levelGeometry)
        {
            // find two areas with floor next to each other
            // calculation is done eagerly and in advance, so it doesn't react on changes of geometry
            var elementsWithFloor = levelGeometry.Leafs().Where(le => AreaType.CanBeConnectedByStairs(le.AreaType) && le.CubeGroup().WithFloor().Cubes.Any()).ToList();
            var closeElementsWithFloor = elementsWithFloor
                .Select2Distinct((el1, el2) => new { el1, el2 })
                .Where(pair => pair.el1.CubeGroup().ExtrudeAll().Intersects(pair.el2.CubeGroup())).ToList();

            return (addingState) =>
            {
                var closePair = closeElementsWithFloor.Where(pair => !connectednessGraph.PathExists(pair.el1, pair.el2)).GetRandom();

                if (closePair == null)
                    return addingState;

                var searchSpace = new CubeGroup(ldk.grid, closePair.el1.CubeGroup().Merge(closePair.el2.CubeGroup()).Cubes);
                Neighbors<PathNode> neighborNodes = PathNode.BoundedBy(PathNode.StairsNeighbors(), searchSpace);
                var newPath = ldk.paths.ConnectByPath(closePair.el1.CubeGroup().WithFloor(), closePair.el2.CubeGroup().WithFloor(), neighborNodes);

                if (newPath == null)
                    return addingState;

                connectednessGraph.Connect(closePair.el1, closePair.el2);
                return addingState.TryPushIntersecting(newPath.LevelElement(AreaType.Path));
            };
        }

        public CurvesLevelDesign(LevelDevelopmentKit ldk) : base(ldk)
        {
        }

        ChangeWorld LinearCurveDesign(LevelElement start, int length)
        {
            return addingState =>
            {
                var group = new LevelGroupElement(ldk.grid, AreaType.None);

                Func<LevelElement> smallBox = () =>
                {
                    var smallDistr = new SlopeDistr(center: 5, width: 3, rightness: 0.3f);
                    return ldk.sgShapes.TurnIntoHouse(ldk.qc.GetFlatBox(ExtensionMethods.RandomBox(smallDistr, smallDistr)).CubeGroup().ExtrudeVer(Vector3Int.up, 6));
                };

                Func<LevelElement> largeBox = () =>
                {
                    var largeDistr = new SlopeDistr(center: 8, width: 3, rightness: 0.3f);
                    return ldk.sgShapes.TurnIntoHouse(ldk.qc.GetFlatBox(ExtensionMethods.RandomBox(largeDistr, largeDistr)).CubeGroup().ExtrudeVer(Vector3Int.up, 6));
                };

                Func<LevelElement> balconyTower = () =>
                {
                    // Create ground layout of the tower
                    var towerLayout = ldk.qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));

                    // Create and set towers to the world
                    var tower = ldk.sgShapes.Tower(towerLayout, 3, 4);

                    tower = ldk.sgShapes.AddBalcony(tower);
                    return tower;
                };

                Func<LevelElement> tower = () =>
                {
                    var size = UnityEngine.Random.Range(3, 4);
                    var floorsCount = UnityEngine.Random.Range(3, 6);

                    // Create ground layout of the tower
                    var towerLayout = ldk.qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(size, size)).InflateY(0, 1));

                    // Create and set towers to the world
                    var tower = ldk.sgShapes.Tower(towerLayout, 3, floorsCount);
                    return tower;
                };


                Func<LevelElement> surrounded = () =>
                {
                    var surrounded = new LevelGroupElement(ldk.grid, AreaType.None);

                    // Create ground layout of the tower
                    var yard = ldk.qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)).InflateY(0, 1)).LevelElement(AreaType.Garden);

                    var boxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(new Vector2Int(3, 3), new Vector2Int(7, 7)));
                    var houses = ldk.qc.FlatBoxes(boxSequence, 6).Select(le => le.SetAreaType(AreaType.House));

                    houses = ldk.pl.SurroundWith(yard, houses);
                    houses = houses.ReplaceLeafsGrp(g => g.AreaType == AreaType.House, g => ldk.sgShapes.SimpleHouseWithFoundation(g.CubeGroup(), 3));

                    var wall = ldk.sgShapes.WallAround(yard, 2).Minus(houses);

                    surrounded = surrounded.AddAll(yard.CubeGroup().ExtrudeVer(Vector3Int.up, 2).Merge(yard.CubeGroup()).LevelElement(AreaType.Garden), houses, wall);
                    return surrounded;
                };

                Func<LevelElement> island = () =>
                {
                    var island = ldk.sgShapes.IslandExtrudeIter(ldk.grid[0, 0, 0].Group(), 13, 0.3f);

                    // wall
                    var wallTop = island.ExtrudeHor(true, false).Minus(island).MoveBy(Vector3Int.up).LevelElement(AreaType.WallTop);
                    var foundation = ldk.sgShapes.Foundation(wallTop).LevelElement(AreaType.Foundation);
                    return new LevelGroupElement(ldk.grid, AreaType.None, island.LevelElement(AreaType.Garden), foundation, wallTop);
                };

                var adders = new List<ChangeWorld>()
                {
                     AddNearXZ(smallBox),
                     AddNearXZ(largeBox),
                     AddNearXZ(balconyTower),
                     //AddNearXZ(surrounded),
                     //AddNearXZ(island),
                     //AddRemoveOverlap(largeBox),
                     PathTo(smallBox),
                     //PathTo(tower),
                }.Shuffle().ToList();

                return addingState.ChangeAll(Enumerable.Range(0, length).Select(i => adders[i % adders.Count]));
            };
        }

        ChangeWorld SplittingPath(LevelElement start, int length)
        {
            return addingState =>
            {
                var pathMaker = LinearCurveDesign(start, length / 3);
                var first = pathMaker(addingState);
                var branches = pathMaker(first);
                branches = pathMaker(branches.SetElement(first.Current));
                return branches;
            };
        }

        IEnumerable<int> HeightCurve()
        {
            var heightDistr = new UniformDistr(2, 10);
            var heightCurve = ExtensionMethods.Naturals().Select(_ => heightDistr.Sample());
            var smooth = heightCurve.Select2((a, b) => (a + b) / 2);
            heightCurve = heightCurve.Interleave(smooth);
            return heightCurve;
        }

        public override LevelElement CreateLevel()
        {
            int length = 28;

            // Height curve
            var heightCurve = HeightCurve();
            var heightDistr = new UniformDistr(0, 6);

            var start = ldk.qc.GetBox(new Box3Int(Vector3Int.zero, Vector3Int.one)).ExtrudeVer(Vector3Int.up, 10).LevelElement();
            WorldState state = new WorldState(start, ldk.grid, le => le.ApplyGrammarStyleRules(ldk.houseStyleRules)/*.MoveBy((heightDistr.Sample() - le.CubeGroup().CubeGroupMaxLayer(Vector3Int.down).Cubes.FirstOrDefault().Position.y)* Vector3Int.up)*/);

            var addedLine = LinearCurveDesign(start, length)(state);

            
            var changers = Enumerable.Repeat(SplitToFloors(), 100).Concat(
                    Enumerable.Repeat(SubdivideRoom(), 100)
                ).ToList();
            
            var subdividedRooms = addedLine.ChangeAll(changers);
            
            subdividedRooms.Added.ApplyGrammarStyleRules(ldk.houseStyleRules);

            var connectednessGraph = new Graph<LevelGeometryElement>(subdividedRooms.Added.Leafs().ToList(), new List<Edge<LevelGeometryElement>>());
            
            var connectedLine = subdividedRooms.AddUntilCan(ConnectTwoUnconnected(connectednessGraph, subdividedRooms.Added), 1000);
            var levelElements = connectedLine.Added;
            


            //levelElements.ApplyGrammarStyleRules(ldk.houseStyleRules);

            return levelElements;
        }
    }
}

using System;
using System.Collections.Generic;
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
        delegate AddingState AddElement(AddingState state);

        AddElement AddNearXZ(Func<LevelElement> elementF)
        {
            return (addingState) =>
            {
                var element = elementF();
                var possibleMoves = element.Moves(element.MovesNearXZ(addingState.Last).Where(m => m.y == 0), addingState.Added.LevelElements);
                var movedElement = possibleMoves.Any() ? element.MoveBy(possibleMoves.GetRandom()) : null;
                return addingState.PushElement(movedElement);
            };
        }

        AddElement AddRemoveOverlap(Func<LevelElement> elementF)
        {
            return (addingState) =>
            {
                var element = elementF();
                var possibleMoves = element.Moves(element.MovesToPartlyIntersectXZ(addingState.Last.Where(le => le.AreaType != AreaType.Path)).Where(m => m.y == 0), addingState.Added.LevelElements.Others(addingState.Last));
                var movedElement = possibleMoves.Any() ? element.MoveBy(possibleMoves.GetRandom()).Minus(addingState.Last) : null;
                return addingState.PushElement(movedElement);
            };
        }

        AddElement PathTo(Func<LevelElement> elementF)
        {
            return (addingState) =>
            {
                var element = elementF();
                var possibleMoves = element.Moves(element.MovesInDistanceXZ(addingState.Last, 5).Where(m => m.y == 0), addingState.Added.LevelElements);
                if (!possibleMoves.Any())
                    return addingState;

                var area = element.MoveBy(possibleMoves.GetRandom());
                var start = addingState.Last.WhereGeom(le => le.AreaType != AreaType.Path);
                var path = ldk.paths.PathH(start, area, 2, addingState.Added).SetAreaType(AreaType.Platform);
                var newElement = new LevelGroupElement(addingState.Last.Grid, AreaType.None, path, area);
                return addingState.PushElement(newElement);
            };
        }

        AddElement SubdivideRoom()
        {
            return (addingState) =>
            {
                var room = addingState.Added.Nonterminals(AreaType.Room).FirstOrDefault();
                if (room == null)
                    return addingState;

                var changedAdded = addingState.Added.ReplaceLeafsGrp(room, _ => ldk.tr.SubdivideRoom(room, ExtensionMethods.HorizontalDirections().GetRandom(), 0.3f));
                return addingState.ChangeAdded(changedAdded);
            };
        }

        public CurvesLevelDesign(LevelDevelopmentKit ldk) : base(ldk)
        {
        }

        AddElement LinearCurveDesign(LevelElement start, int length)
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

                    surrounded = surrounded.AddAll(yard, houses, wall);
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

                var adders = new List<AddElement>()
                {
                     AddNearXZ(smallBox),
                     AddNearXZ(largeBox),
                     AddNearXZ(balconyTower),
                     AddNearXZ(surrounded),
                     AddNearXZ(island),
                     //AddRemoveOverlap(largeBox),
                     PathTo(smallBox),
                     PathTo(tower),
                }.Shuffle().ToList();

                return addingState.AddAll(Enumerable.Range(0, length).Select(i => adders[i % adders.Count]));
            };
        }

        AddElement SplittingPath(LevelElement start, int length)
        {
            return addingState =>
            {
                var pathMaker = LinearCurveDesign(start, length / 3);
                var first = pathMaker(addingState);
                var branches = pathMaker(first);
                branches = pathMaker(branches.SetElement(first.Last));
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
            int length = 8;

            // Height curve
            var heightDistr = new UniformDistr(2, 10);
            var heightCurve = ExtensionMethods.Naturals().Select(_ => heightDistr.Sample());
            var smooth = heightCurve.Select2((a, b) => (a + b) / 2);
            heightCurve = heightCurve.Interleave(smooth);

            var start = ldk.qc.GetBox(new Box3Int(Vector3Int.zero, Vector3Int.one)).ExtrudeVer(Vector3Int.up, 10).LevelElement();

            AddingState state = new AddingState(start, ldk.grid);

            //var addedLine = SplittingPath(start, length)(state);
            var addedLine = LinearCurveDesign(start, length)(state);
            
            var levelElements = addedLine.Added;
            
            /*
            levelElements = levelElements.LevelElements.Select((le, i) =>
            {
                return le.MoveBy(Vector3Int.up * heightCurve.ElementAt(i));
            }).ToLevelGroupElement(ldk.grid);

            levelElements = levelElements.ReplaceLeafsGrp(g => g.AreaType == AreaType.House, g => ldk.sgShapes.SimpleHouseWithFoundation(g.CubeGroup(), 8));*/

            //levelElements = levelElements.ReplaceLeafsGrp(g => g.AreaType == AreaType.Room, g => ldk.sgShapes.SubdivideRoom(g, ExtensionMethods.HorizontalDirections().GetRandom(), 0.3f));
            //levelElements = levelElements.ReplaceLeafsGrp(g => g.AreaType == AreaType.Room && g.Cubes().Any(), g => ldk.sgShapes.FloorHouse(g, ldk.sgShapes.BrokenFloor, 3, 6, 9));
            
            levelElements.ApplyGrammarStyleRules(ldk.houseStyleRules);

            return levelElements;
        }

        class AddingState
        {
            public LevelGroupElement Added { get; }
            public LevelElement Last { get; }
            Grid Grid { get; }

            public AddingState(LevelElement last, Grid grid)
            {
                Added = new LevelGroupElement(grid, AreaType.None);
                Last = last;
            }

            public AddingState(LevelGroupElement added, LevelElement last, Grid grid)
            {
                Added = added;
                Last = last;
            }

            public AddingState AddAll(IEnumerable<AddElement> adders)
            {
                return adders.Aggregate(this,
                (addingState, adder) =>
                {
                    var newState = adder(addingState);

                    return newState.Last == null ? addingState : newState;
                });
            }

            public AddingState PushElement(LevelElement le)
            {
                return new AddingState(Added.Merge(le), le, Grid);
            }

            public AddingState SetElement(LevelElement le)
            {
                return new AddingState(Added, le, Grid);
            }

            public AddingState ChangeAdded(LevelGroupElement newAdded)
            {
                return new AddingState(newAdded, Last, Grid);
            }
        }
    }
}

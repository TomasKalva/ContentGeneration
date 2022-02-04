﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class LevelDevelopmentKit
    {
        public ShapeGrammarObjectStyle FountainheadStyle { get; }
        public Grid grid { get; }
        public QueryContext qc { get; }
        public ShapeGrammarStyles sgStyles { get; }
        public ShapeGrammarShapes sgShapes { get; }
        public Placement pl { get; }
        public Paths paths { get; }
        public StyleRules houseStyleRules { get; }

        public LevelDevelopmentKit(ShapeGrammarObjectStyle objectStyle)
        {
            FountainheadStyle = objectStyle;
            grid = new Grid(new Vector3Int(20, 10, 20));
            qc = new QueryContext(grid);
            sgStyles = new ShapeGrammarStyles(grid, FountainheadStyle);
            sgShapes = new ShapeGrammarShapes(grid);
            pl = new Placement(grid);
            paths = new Paths(grid);
            houseStyleRules = new StyleRules(
                new StyleRule(g => g.WithAreaType(AreaType.Room), g => g.SetGrammarStyle(sgStyles.RoomStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Roof), g => g.SetGrammarStyle(sgStyles.FlatRoofStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Foundation), g => g.SetGrammarStyle(sgStyles.FoundationStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Path), g => g.SetGrammarStyle(sgStyles.StairsPathStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Garden), g => g.SetGrammarStyle(sgStyles.GardenStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.WallTop), g => g.SetGrammarStyle(sgStyles.FlatRoofStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Wall), g => g.SetGrammarStyle(sgStyles.FoundationStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Empty), g => g.SetGrammarStyle(sgStyles.EmptyStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Debug), g => g.SetGrammarStyle(sgStyles.RoomStyle))
            );
        }
    }

    public class Examples : LevelDevelopmentKit
    {
        public Examples(ShapeGrammarObjectStyle objectStyle) : base(objectStyle)
        { }
        
        public void Island()
        {
            // island
            var island = sgShapes.IslandExtrudeIter(grid[0, 0, 0].Group(), 13, 0.3f);
            island.SetGrammarStyle(sgStyles.PlatformStyle);

            // house
            var house = sgShapes.SimpleHouseWithFoundation(new Box2Int(new Vector2Int(2, 2), new Vector2Int(8, 5)), 5);

            var houseBottom = house.WithAreaType(AreaType.Foundation).FirstOrDefault().CubeGroup().CubeGroupLayer(Vector3Int.down);
            var houseToIslandDir = island.MinkowskiMinus(houseBottom).GetRandom();
            house = house.MoveBy(houseToIslandDir);

            house.ApplyGrammarStyleRules(houseStyleRules);

            // wall
            var wallTop = island.ExtrudeHor(true, false).Minus(island).MoveBy(Vector3Int.up).SetGrammarStyle(sgStyles.FlatRoofStyle).LevelElement(AreaType.Wall);
            sgShapes.Foundation(wallTop).SetGrammarStyle(sgStyles.FoundationStyle);

            // balcony
            var balconyRoom = house.WithAreaType(AreaType.Room).FirstOrDefault();
            var balcony = sgShapes.BalconyWide(balconyRoom.CubeGroup()).LevelElement(AreaType.Balcony);
            house = house.ReplaceLeafsGrp(le => le == balconyRoom, le => new LevelGroupElement(grid, AreaType.None, balconyRoom, balcony));
            house.WithAreaType(AreaType.Balcony).FirstOrDefault().SetGrammarStyle(cg => sgStyles.BalconyStyle(cg, house.WithAreaType(AreaType.Room).FirstOrDefault().CubeGroup()));

            // house 2
            var house2 = house.MoveBy(Vector3Int.right * 8).ApplyGrammarStyleRules(houseStyleRules);

            var symmetryFace = house2.CubeGroup()
                .CubeGroupLayer(Vector3Int.back).Cubes.FirstOrDefault().Group()
                .MoveBy(Vector3Int.back)
                .BoundaryFacesH(Vector3Int.back).Facets.FirstOrDefault();
            var house3 = house2.CubeGroup().Symmetrize(symmetryFace).SetGrammarStyle(sgStyles.RoomStyle);
        }

        public void Houses()
        {
            // houses
            var flatBoxes = qc.FlatBoxes(3, 7, 5);
            var bounds = qc.GetFlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(15, 15)));
            var town = qc.RemoveOverlap(pl.PlaceInside(bounds, flatBoxes));
            town = qc.LiftRandomly(town, () => 2 + UnityEngine.Random.Range(1, 4));
            town = town.Select(g => sgShapes.SimpleHouseWithFoundation(g.CubeGroup(), 5).ApplyGrammarStyleRules(houseStyleRules));

            // house roofs
            var housesRoofs = town.Select(le =>
                qc.Partition(
                    (g, boundingG) => qc.GetRandomHorConnected1(g, boundingG),
                        le.CubeGroup()
                        .CubeGroupMaxLayer(Vector3Int.up),
                    3)
                );
            housesRoofs = housesRoofs.Select(houseRoof =>
                    qc.RaiseRandomly(houseRoof as LevelGroupElement, () => UnityEngine.Random.Range(0, 4))
                    .Select(part => sgShapes.TurnIntoHouse(part.CubeGroup()))
                    .SetChildrenAreaType(AreaType.Room));

            housesRoofs.ApplyGrammarStyleRules(houseStyleRules);

            // paths from room to roof
            town.LevelElements.ForEach(g =>
            {
                var upper = g.CubeGroup().WithFloor().CubeGroupMaxLayer(Vector3Int.up).ExtrudeHor(false, true);
                var lower = g.CubeGroup().WithFloor().CubeGroupMaxLayer(Vector3Int.down).ExtrudeHor(false, true);
                var searchSpace = g.CubeGroup().ExtrudeHor(false, true);
                Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.StairsNeighbors(), searchSpace);
                var path = paths.ConnectByPath(lower, upper, neighbors);
                path.SetGrammarStyle(sgStyles.StairsPathStyle);
            });
        }

        public void NonOverlappingTown()
        {
            var flatBoxes = qc.FlatBoxes(3, 8, 8);
            var bounds = qc.GetFlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)));
            var town = pl.MoveToNotOverlap(flatBoxes);// qc.GetOverlappingBoxes(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)), 8));
            //var town = qc.GetNonOverlappingBoxes(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)), 8);
            var intSeq = new IntervalDistr(new IntDistr(), 4, 10);
            town = qc.RaiseRandomly(town, intSeq.Sample);
            town = town.Select(le => sgShapes.TurnIntoHouse(le.CubeGroup()));// le.SetAreaType(AreaType.Room));
            town.ApplyGrammarStyleRules(houseStyleRules);
        }

        public void RemovingOverlap()
        {
            var boxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(new Vector2Int(3, 3), new Vector2Int(5, 5)));
            var town = pl.MoveToNotOverlap(qc.FlatBoxes(boxSequence, 16));
            town = town.Select(le => le.SetAreaType(AreaType.Room));
            town.ApplyGrammarStyleRules(houseStyleRules);
        }

        public void Tower()
        {
            var towerLayout = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));
            var tower = sgShapes.Tower(towerLayout, 3, 4);
            tower = sgShapes.AddBalcony(tower);
            tower.ApplyGrammarStyleRules(houseStyleRules);


            // add paths between floors
            var floors = tower.WithAreaType(AreaType.Room);
            var floorsAndRoof = floors.Concat(tower.WithAreaType(AreaType.Roof));
            floors.ForEach(floor =>
            {
                var upperFloor = floor.NeighborsInDirection(Vector3Int.up, floorsAndRoof).FirstOrDefault();
                if (upperFloor == null)
                    return;

                var lower = floor.CubeGroup().WithFloor().CubeGroupMaxLayer(Vector3Int.down);
                var upper = upperFloor.CubeGroup().WithFloor().CubeGroupMaxLayer(Vector3Int.down);
                var searchSpace = new CubeGroup(grid, floor.CubeGroup().ExtrudeHor(false, true).Cubes.Concat(upper.Cubes).ToList());
                Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.StairsNeighbors(), searchSpace);
                var path = paths.ConnectByPath(lower, upper, neighbors);
                path.SetGrammarStyle(sgStyles.StairsPathStyle);
            });
        }

        public void TwoConnectedTowers()
        {
            // Create ground layout of the tower
            var towerLayout = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));
            
            // Create and set towers to the world
            var tower = sgShapes.Tower(towerLayout, 3, 4);
            tower.ApplyGrammarStyleRules(houseStyleRules);
            var tower2 = tower.MoveBy(new Vector3Int(20, 0, 10)).ApplyGrammarStyleRules(houseStyleRules);

            // Create path between the towers
            paths.WalkableWallPathH(tower, tower2, 1).ApplyGrammarStyleRules(houseStyleRules);

            // Add balcony to one of the towers
            // house rules are applied to the entire house again...
            // todo: apply them only to the parts that were added
            sgShapes.AddBalcony(tower).ApplyGrammarStyleRules(houseStyleRules);
        }

        public void ManyConnectedTowers()
        {
            // Create ground layout of the tower
            var towerLayout = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));

            // Create and set towers to the world
            int towersCount = 5;
            Func<int, float> angle = i => Mathf.PI * 2f * (i / (float)towersCount);
            var towers = Enumerable.Range(0, towersCount).Select(
                i => sgShapes.Tower(towerLayout, 3, 4)
                .MoveBy(Vector3Int.RoundToInt(10f * new Vector3(Mathf.Cos(angle(i)), 0f, Mathf.Sin(angle(i))))));

            towers.ForEach(tower => tower.ApplyGrammarStyleRules(houseStyleRules));
            towers.ForEach2Cycle((t1, t2) => paths.WalkableWallPathH(t1, t2, 3).ApplyGrammarStyleRules(houseStyleRules));
        }

        public void Surrounded()
        {
            var root = new LevelGroupElement(grid, AreaType.WorldRoot);

            // Create ground layout of the tower
            var yard = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)).InflateY(0, 1)).LevelElement(AreaType.Garden);


            var boxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(new Vector2Int(3, 3), new Vector2Int(7, 7)));
            var houses = qc.FlatBoxes(boxSequence, 6).Select(le => le.SetAreaType(AreaType.House));
            
            houses = pl.SurroundWith(yard, houses);
            houses = houses.ReplaceLeafsGrp(g => g.AreaType == AreaType.House, g => sgShapes.SimpleHouseWithFoundation(g.CubeGroup(), 3));

            var wall = sgShapes.WallAround(yard, 2).Minus(houses);

            root = root.AddAll(yard, houses, wall);
            root.ApplyGrammarStyleRules(houseStyleRules);
        }

        public void TestingPaths()
        {
            // testing paths
            var box = sgShapes.Room(new Box3Int(new Vector3Int(0, 0, 0), new Vector3Int(10, 10, 10)));
            var start = box.CubeGroupLayer(Vector3Int.left);
            var end = box.CubeGroupLayer(Vector3Int.right);
            Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.StairsNeighbors(), box);
            var path = paths.ConnectByPath(start, end, neighbors);
            path.SetGrammarStyle(sgStyles.StairsPathStyle);
        }

        public void ControlPointDesign()
        {
            var controlPointsDesign = new ControlPointsLevelDesign(this);
            controlPointsDesign.CreateLevel();
        }

        public void CurveDesign()
        {
            var CurveDesign = new CurvesLevelDesign(this);
            CurveDesign.CreateLevel();
        }

        public void House()
        {
            var houseBox = qc.GetFlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 8)), 0);
            
            var box = houseBox.CubeGroup().ExtrudeVer(Vector3Int.up, 10).LevelElement();
            box.SplitRel(Vector3Int.up, 0.3f, 0.7f).SplitRel(Vector3Int.right, 0.3f, 0.7f)
                .ReplaceLeafs(_ => true, g => g.SetAreaType(AreaType.Room)).ApplyGrammarStyleRules(houseStyleRules);
            
            /*
            var house = sgShapes.SimpleHouseWithFoundation(houseBox.CubeGroup(), 8);
            var split = qc.RecursivelySplitXZ(houseBox, 4).ReplaceLeafs(_ => true, g => g.SetAreaType(AreaType.Room));
            split.ApplyGrammarStyleRules(houseStyleRules);*/


        }

        public void CompositeHouse()
        {
            var house = sgShapes.CompositeHouse(/*new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)),*/ 6);
            house.ApplyGrammarStyleRules(houseStyleRules);
        }
    }

    public interface IIntDistr
    {
        public int Sample();
    }

    public class RandomIntDistr : IIntDistr
    {
        int min, max;

        public RandomIntDistr(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public int Sample()
        {
            return UnityEngine.Random.Range(min, max);
        }
    }

    public class IntDistr : IIntDistr
    {
        int start, step;
        int current;

        public IntDistr(int start = 0, int step = 1)
        {
            this.start = start;
            this.step = step;
            this.current = start - step;
        }

        public int Sample()
        {
            current += step;
            return current;
        }
    }

    public class IntervalDistr : IIntDistr
    {
        IIntDistr seq;
        int min, max;

        public IntervalDistr(IIntDistr seq, int min, int max)
        {
            this.seq = seq;
            this.min = min;
            this.max = max;

            Debug.Assert(max > min);
        }

        public int Sample()
        {
            var i = seq.Sample();
            return i.Mod(max - min) + min;
        }
    }

    /// <summary>
    /// Binomial distribution given by center and width.
    /// </summary>
    public class SlopeDistr : IIntDistr
    {
        float p;
        int center, width;
        int size;

        public SlopeDistr(int center = 0, int width = 1, float rightness = 0.5f)
        {
            this.center = center;
            this.width = width;
            this.p = rightness;
            size = 1 + 2 * width;
        }

        public int Sample()
        {
            var successes = 0;
            for(int i = 0; i < size; i++)
            {
                if(UnityEngine.Random.Range(0f, 1f) < p)
                {
                    successes++;
                }
            }
            return center + successes - width;
        }
    }
    public class UniformDistr : IIntDistr
    {
        int min, max;

        public UniformDistr(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public int Sample()
        {
            return UnityEngine.Random.Range(min, max);
        }
    }
}

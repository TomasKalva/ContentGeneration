using OurFramework.Environment.GridMembers;
using OurFramework.Environment.StylingAreas;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util;

namespace OurFramework.Environment.ShapeCreation
{
    public class LevelDevelopmentKit
    {
        public Grid<Cube> grid { get; }
        public QueryContext qc { get; }
        public ShapeGrammarShapes sgShapes { get; }
        public Placement pl { get; }
        public Transformations tr { get; }
        public Connections con { get; }

        public LevelDevelopmentKit(GeometricPrimitives gp)
        {
            grid = new Grid<Cube>(new Vector3Int(20, 10, 20), (grid, pos) => new Cube(grid, pos));
            qc = new QueryContext(grid);
            sgShapes = new ShapeGrammarShapes(grid);
            pl = new Placement(grid);
            con = new Connections();
            tr = new Transformations(this);
            AreaStyles.Initialize(new GridPrimitives(gp), new GridPrimitivesPlacement(grid));
        }
    }

    public class Examples : LevelDevelopmentKit
    {
        public Examples(GeometricPrimitives gp) : base(gp)
        { }

        public Func<LevelElement>[] AllExamples() =>
            new Func<LevelElement>[]
            {
                Island,
                Houses,
                NonOverlappingTown,
                RemovingOverlap,
                Tower,
                TwoConnectedTowers,
                Surrounded,
                SplitRoom,
                PartlyBrokenFloorHouse,
                JustHouse,
                CompositeHouse,
                TestMoveInDistXZ
            };

        public LevelElement Island()
        {
            var island = sgShapes.IslandExtrudeIter(grid[0, 0, 0].Group(), 13, 0.3f);

            // wall
            var wallTop = island.ExtrudeHor(true, false).Minus(island).MoveBy(Vector3Int.up).LE(AreaStyles.WallTop());
            var foundation = sgShapes.Foundation(wallTop);
            var total = new LevelGroupElement(grid, AreaStyles.None(), island.LE(AreaStyles.Garden()), foundation, wallTop);

            total.ApplyGrammarStyles();

            return total;
        }

        public LevelElement Houses()
        {
            // houses
            var flatBoxes = qc.FlatBoxes(3, 7, 5);
            var bounds = qc.GetFlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(15, 15)));
            var town = qc.RemoveOverlap(pl.PlaceInside(bounds, flatBoxes));
            town = qc.LiftRandomly(town, () => 2 + MyRandom.Range(1, 4));
            town = town.Select(g => sgShapes.SimpleHouseWithFoundation(g.CG(), 5).ApplyGrammarStyles());

            // house roofs
            var housesRoofs = town.Select(le =>
                qc.Partition(
                    (g, boundingG) => qc.GetRandomHorConnected1(g, boundingG),
                        le.CG()
                        .CubeGroupMaxLayer(Vector3Int.up),
                    3)
                );
            housesRoofs = housesRoofs.Select(houseRoof =>
                    qc.RaiseRandomly(houseRoof as LevelGroupElement, () => MyRandom.Range(1, 4))
                    .Select(part => sgShapes.TurnIntoHouse(part.CG()))
                    .SetChildrenAreaType(AreaStyles.Room()));
            
            housesRoofs.ApplyGrammarStyles();
            
            return town;
        }

        public LevelElement NonOverlappingTown()
        {
            var flatBoxes = qc.FlatBoxes(3, 8, 20);
            var bounds = qc.GetFlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)));
            var town = pl.MoveToNotOverlap(flatBoxes);// qc.GetOverlappingBoxes(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)), 8));
            //var town = qc.GetNonOverlappingBoxes(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)), 8);
            var intSeq = new IntervalDistr(new IntSeqDistr(), 4, 10);
            town = qc.RaiseRandomly(town, intSeq.Sample);
            town = town.Select(le => sgShapes.TurnIntoHouse(le.CG()));// le.SetAreaType(AreaType.Room));
            town.ApplyGrammarStyles();

            return town;
        }

        public LevelElement RemovingOverlap()
        {
            var boxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(new Vector2Int(3, 3), new Vector2Int(5, 5)));
            var town = pl.MoveToNotOverlap(qc.FlatBoxes(boxSequence, 16));
            town = town.Select(le => le.SetAreaType(AreaStyles.Room()));
            town.ApplyGrammarStyles();

            return town;
        }
        public LevelElement Tower()
        {
            var towerLayout = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));
            var tower = sgShapes.Tower(towerLayout, 3, 4);
            tower = sgShapes.AddBalcony(tower);
            tower.ApplyGrammarStyles();

            return tower;
        }

        public LevelElement TwoConnectedTowers()
        {
            // Create ground layout of the tower
            var towerLayout = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));
            
            // Create and set towers to the world
            var tower = sgShapes.Tower(towerLayout, 3, 4);
            tower.ApplyGrammarStyles();
            var tower2 = tower.MoveBy(new Vector3Int(20, 0, 10)).ApplyGrammarStyles();

            // Create path between the towers
            var path = con.ConnectByStairsOutside(LevelElement.Empty(grid), new LevelGroupElement(grid, AreaStyles.None()))(tower, tower2);

            // Add balcony to one of the towers
            // house rules are applied to the entire house again...
            // todo: apply them only to the parts that were added
            sgShapes.AddBalcony(tower).ApplyGrammarStyles();

            return tower.Merge(tower2, path);
        }

        /*
        public LevelElement ManyConnectedTowers()
        {
            // Create ground layout of the tower
            var towerLayout = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));

            // Create and set towers to the world
            int towersCount = 5;
            Func<int, float> angle = i => Mathf.PI * 2f * (i / (float)towersCount);
            var towers = Enumerable.Range(0, towersCount).Select(
                i => sgShapes.Tower(towerLayout, 3, 4)
                .MoveBy(Vector3Int.RoundToInt(10f * new Vector3(Mathf.Cos(angle(i)), 0f, Mathf.Sin(angle(i))))));

            towers.ForEach(tower => tower.ApplyGrammarStyleRules());
            towers.ForEach2Cycle((t1, t2) => paths.WalkableWallPathH(t1, t2, 1).ApplyGrammarStyleRules());

            return towers.ToLevelGroupElement(grid);
        }*/

        public LevelElement Surrounded()
        {
            var root = new LevelGroupElement(grid, AreaStyles.Room());

            // Create ground layout of the tower
            var yard = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)).InflateY(0, 1)).LE(AreaStyles.Garden());


            var boxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(new Vector2Int(3, 3), new Vector2Int(7, 7)));
            var houses = qc.FlatBoxes(boxSequence, 6).Select(le => le.SetAreaType(AreaStyles.Room()));
            
            houses = pl.SurroundWith(yard, houses);
            houses = houses.ReplaceLeafsGrp(g => g.AreaStyle == AreaStyles.Room(), g => sgShapes.SimpleHouseWithFoundation(g.CG(), 3));

            var wall = sgShapes.WallAround(yard, 2).Minus(houses);

            root = root.AddAll(yard, houses, wall);
            root.ApplyGrammarStyles();

            return root;
        }

        public LevelElement SplitRoom()
        {
            var houseBox = qc.GetFlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 8)), 0);
            
            var box = houseBox.CG().ExtrudeVer(Vector3Int.up, 10).LE(AreaStyles.Room());
            var splitBox = box.SplitRel(Vector3Int.right, AreaStyles.None(), 0.5f).SplitRel(Vector3Int.up, AreaStyles.Room(), 0.5f);

            var wall = splitBox.Leafs().ElementAt(0);

            var empty = splitBox.Leafs().ElementAt(3);

            splitBox
                .ReplaceLeafsGrp(0, le => le.SetAreaType(AreaStyles.Wall()))
                .ReplaceLeafsGrp(1, le => le.SetAreaType(AreaStyles.OpenRoom()))
                .ReplaceLeafsGrp(3, le => le.SetAreaType(AreaStyles.Empty()))
                .ApplyGrammarStyles();

            return splitBox;
        }

        public LevelElement PartlyBrokenFloorHouse()
        {
            var floorBox = qc.GetFlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)), 0);

            Func<LevelGeometryElement, LevelElement> brokenFloor = (floor) =>
            {
                var floorPlan = tr.SplittingFloorPlan(floor, 4);
                var partlyBrokenFloor = tr.PickAndConnect(floorPlan, tr.PickWithChance(0.5f)).ReplaceLeafs(le => le.AreaStyle != AreaStyles.Empty(), le => le.SetAreaType(AreaStyles.Platform()));
                return partlyBrokenFloor;
            };

            var house = floorBox.CG().ExtrudeVer(Vector3Int.up, 15).LE(AreaStyles.Room());
            var houseFloors = house.SplitRel(Vector3Int.up, AreaStyles.None(), 0.2f, 0.5f, 0.7f).ReplaceLeafsGrp(_ => true, le => brokenFloor(le));

            houseFloors.ApplyGrammarStyles();

            return houseFloors;
        }

        public LevelElement JustHouse()
        {
            var floorBox = qc.GetFlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)), 0);
            var house = sgShapes.TurnIntoHouse(floorBox.CG().ExtrudeVer(Vector3Int.up, 3));

            house.ApplyGrammarStyles();
            return house;
        }

        public LevelElement CompositeHouse()
        {
            var house = sgShapes.CompositeHouse(/*new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)),*/ 6);
            return house.ApplyGrammarStyles();
        }

        public LevelElement TestMoveInDistXZ()
        {
            var room1 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5));
            var room2 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(1, 5));

            room1.ApplyGrammarStyles();
            room2.MovesInDistanceXZ(room1, 1).TryMove().ApplyGrammarStyles();

            return room1;
        }
    }
}

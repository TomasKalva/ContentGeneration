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
        public Transformations tr { get; }
        public Connections con { get; }

        public LevelDevelopmentKit(GeometricPrimitives gp)
        {
            grid = new Grid<Cube>(new Vector3Int(20, 10, 20), (grid, pos) => new Cube(grid, pos));
            qc = new QueryContext(grid);
            sgShapes = new ShapeGrammarShapes(grid);
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
                Tower,
                TwoConnectedTowers,
                SplitRoom,
                PartlyBrokenFloorHouse,
                JustHouse,
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

        public LevelElement Tower()
        {
            var towerLayout = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));
            var tower = sgShapes.Tower(towerLayout, 3, 4);
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

            return tower.Merge(tower2, path);
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

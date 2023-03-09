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
        public CGShapes cgs { get; }
        public LEShapes les { get; }
        public Connections con { get; }

        public LevelDevelopmentKit(GeometricPrimitives gp)
        {
            grid = new Grid<Cube>(new Vector3Int(20, 10, 20), (grid, pos) => new Cube(grid, pos));
            cgs = new CGShapes(grid);
            les = new LEShapes(grid);
            con = new Connections();
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
                JustHouse,
                TestMoveInDistXZ,
                PartitionedHouse,
                SplitHouse
            };

        public LevelElement Island()
        {
            var island = cgs.IslandExtrudeIter(grid[0, 0, 0].Group(), 13, 0.3f).LE(AreaStyles.Garden());
            var total = new LevelGroupElement(grid, AreaStyles.None(), island, les.WallAround(island, 3));
            total.ApplyGrammarStyles();

            return total;
        }

        public LevelElement Tower()
        {
            var towerLayout = cgs.Box(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));
            var tower = les.Tower(towerLayout, 3, 4);
            tower.ApplyGrammarStyles();

            return tower;
        }

        public LevelElement TwoConnectedTowers()
        {
            // Create ground layout of the tower
            var towerLayout = cgs.Box(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));
            
            // Create and set towers to the world
            var tower = les.Tower(towerLayout, 3, 4);
            tower.ApplyGrammarStyles();
            var tower2 = tower.MoveBy(new Vector3Int(20, 0, 10)).ApplyGrammarStyles();

            // Create path between the towers
            var path = con.ConnectByStairsOutside(LevelElement.Empty(grid), new LevelGroupElement(grid, AreaStyles.None()))(tower, tower2);

            return tower.Merge(tower2, path);
        }

        public LevelElement SplitRoom()
        {
            var houseBox = cgs.FlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 8)), 0);
            
            var box = houseBox.ExtrudeVer(Vector3Int.up, 10).LE(AreaStyles.Room());
            var splitBox = box.SplitRel(Vector3Int.right, AreaStyles.None(), 0.5f).SplitRel(Vector3Int.up, AreaStyles.Room(), 0.5f);

            var wall = splitBox.Leafs().ElementAt(0);

            var empty = splitBox.Leafs().ElementAt(3);

            splitBox
                .ReplaceLeafsGrp(0, le => le.SetAreaStyle(AreaStyles.Wall()))
                .ReplaceLeafsGrp(1, le => le.SetAreaStyle(AreaStyles.OpenRoom()))
                .ReplaceLeafsGrp(3, le => le.SetAreaStyle(AreaStyles.Empty()))
                .ApplyGrammarStyles();

            return splitBox;
        }

        public LevelElement JustHouse()
        {
            var floorBox = cgs.FlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)), 0);
            var house = les.TurnIntoHouse(floorBox.ExtrudeVer(Vector3Int.up, 3));

            house.ApplyGrammarStyles();
            return house;
        }

        public LevelElement TestMoveInDistXZ()
        {
            var room1 = les.Room(4, 4, 5);
            var room2 = les.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(1, 5));

            room1.ApplyGrammarStyles();
            room2.MovesInDistanceXZ(room1, 1).TryMove().ApplyGrammarStyles();

            return room1;
        }

        public LevelElement PartitionedHouse()
        {
            var houseBox = les.Box(10, 20);

            var partitionedHouse = les.Partition(cgs.GetRandomHorConnected, houseBox.CG(), 5)
                .ReplaceLeafs(_ => true, leaf => 
                    leaf.CG()
                        .OpAdd()
                            .ExtrudeDir(Vector3Int.up, 3)
                        .OpNew()
                        .LE(AreaStyles.Room()));

            return partitionedHouse;
        }

        public LevelElement SplitHouse()
        {
            var houseBox = les.Box(10, 20, 4);

            var splitHouse = les.RecursivelySplit(houseBox, 4)
                .ReplaceLeafs(_ => true, leaf =>
                    leaf.SetAreaStyle(AreaStyles.Room())); ;

            return splitHouse;
        }
    }
}

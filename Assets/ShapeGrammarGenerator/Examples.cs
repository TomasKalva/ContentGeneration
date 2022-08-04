using Assets.ShapeGrammarGenerator;
using ContentGeneration.Assets.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class LevelDevelopmentKit
    {
        public ShapeGrammarObjectStyle DefaultHouseStyle { get; }
        public ShapeGrammarObjectStyle GardenStyle { get; }
        public WorldGeometry wg { get; }
        public Grid<Cube> grid { get; }
        public QueryContext qc { get; }
        public ShapeGrammarStyles sgStyles { get; }
        public ShapeGrammarShapes sgShapes { get; }
        public AreaStyles ars { get; }
        public Placement pl { get; }
        public Paths paths { get; }
        public Transformations tr { get; }
        public StyleRules houseStyleRules { get; }
        public WorldChanging wc { get; }
        public Connections con { get; }

        public LevelDevelopmentKit(ShapeGrammarObjectStyle defaultHouseStyle, ShapeGrammarObjectStyle gardenStyle, GeometricPrimitives gp, Transform worldParent, Libraries lib)
        {
            DefaultHouseStyle = defaultHouseStyle;
            wg = new WorldGeometry(worldParent, 2.8f);
            grid = new Grid<Cube>(new Vector3Int(20, 10, 20), (grid, pos) => new Cube(grid, pos));
            qc = new QueryContext(grid);
            sgStyles = new ShapeGrammarStyles(grid, DefaultHouseStyle, gardenStyle);
            sgShapes = new ShapeGrammarShapes(grid);
            pl = new Placement(grid);
            paths = new Paths(grid);
            con = new Connections(grid);
            tr = new Transformations(this);
            ars = new AreaStyles(new GridPrimitives(gp), sgStyles);
            houseStyleRules = new StyleRules(
                new StyleRule(g => g.WithAreaType(AreaType.Room), g => g.SetGrammarStyle(sgStyles.PlainRoomStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Reservation), g => g.SetGrammarStyle(sgStyles.EmptyStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.OpenRoom), g => g.SetGrammarStyle(sgStyles.OpenRoomStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.FlatRoof), g => g.SetGrammarStyle(sgStyles.FlatRoofStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.GableRoof), g => g.SetGrammarStyle(area => sgStyles.GableRoofStyle(area, lib))),
                new StyleRule(g => g.WithAreaType(AreaType.PointyRoof), g => g.SetGrammarStyle(area => sgStyles.PointyRoofStyle(area, lib))),
                new StyleRule(g => g.WithAreaType(AreaType.CrossRoof), g => g.SetGrammarStyle(area => sgStyles.CrossRoofStyle(area, lib))),
                new StyleRule(g => g.WithAreaType(AreaType.Foundation), g => g.SetGrammarStyle(sgStyles.FoundationStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Path), g => g.SetGrammarStyle(sgStyles.StairsPathStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Garden), g => g.SetGrammarStyle(sgStyles.GardenStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Yard), g => g.SetGrammarStyle(sgStyles.FlatRoofStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.WallTop), g => g.SetGrammarStyle(sgStyles.FlatRoofStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Wall), g => g.SetGrammarStyle(sgStyles.FoundationStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.CliffFoundation), g => g.SetGrammarStyle(sgStyles.CliffFoundationStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Empty), g => g.SetGrammarStyle(sgStyles.EmptyStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Inside), g => g.SetGrammarStyle(sgStyles.RoomStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Platform), g => g.SetGrammarStyle(sgStyles.PlatformStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Debug), g => g.SetGrammarStyle(sgStyles.RoomStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Colonnade), g => g.SetGrammarStyle(sgStyles.ColonnadeStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Elevator), g => g.SetGrammarStyle(area => sgStyles.ElevatorStyle(area, lib))),
                new StyleRule(g => g.WithAreaType(AreaType.Fall), g => g.SetGrammarStyle(area => sgStyles.FallStyle(area))),
                new StyleRule(g => g.WithAreaType(AreaType.Door), g => g.SetGrammarStyle(sgStyles.DoorStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Bridge), g => g.SetGrammarStyle(sgStyles.FlatRoofStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.NoFloor), g => g.SetGrammarStyle(sgStyles.NoFloor))
            );
            wc = new WorldChanging(this);
        }
    }

    public class Examples : LevelDevelopmentKit
    {
        public Examples(ShapeGrammarObjectStyle defaultHouseStyle, ShapeGrammarObjectStyle gardenStyle, GeometricPrimitives gp, Transform worldParent, Libraries lib) : base(defaultHouseStyle, gardenStyle, gp, worldParent, lib)
        { }
        
        public void IslandAndHouses()
        {
            // island
            var island = sgShapes.IslandExtrudeIter(grid[0, 0, 0].Group(), 13, 0.3f);
            island.SetGrammarStyle(sgStyles.PlatformStyle);

            // house
            var house = sgShapes.SimpleHouseWithFoundation(new Box2Int(new Vector2Int(2, 2), new Vector2Int(8, 5)), 5);

            var houseBottom = house.WithAreaType(AreaType.Foundation).FirstOrDefault().CG().CubeGroupLayer(Vector3Int.down);
            var houseToIslandDir = island.MinkowskiMinus(houseBottom).GetRandom();
            house = house.MoveBy(houseToIslandDir);

            house.ApplyGrammarStyleRules(houseStyleRules);

            // wall
            var wallTop = island.ExtrudeHor(true, false).Minus(island).MoveBy(Vector3Int.up).SetGrammarStyle(sgStyles.FlatRoofStyle).LE(AreaType.Wall);
            sgShapes.Foundation(wallTop).SetGrammarStyle(sgStyles.FoundationStyle);

            // balcony
            var balconyRoom = house.WithAreaType(AreaType.Room).FirstOrDefault();
            var balcony = sgShapes.BalconyWide(balconyRoom.CG()).LE(AreaType.Balcony);
            house = house.ReplaceLeafsGrp(le => le == balconyRoom, le => new LevelGroupElement(grid, AreaType.None, balconyRoom, balcony));
            house.WithAreaType(AreaType.Balcony).FirstOrDefault().SetGrammarStyle(cg => sgStyles.BalconyStyle(cg, house.WithAreaType(AreaType.Room).FirstOrDefault().CG()));

            // house 2
            var house2 = house.MoveBy(Vector3Int.right * 8).ApplyGrammarStyleRules(houseStyleRules);

            var symmetryFace = house2.CG()
                .CubeGroupLayer(Vector3Int.back).Cubes.FirstOrDefault().Group()
                .MoveBy(Vector3Int.back)
                .BoundaryFacesH(Vector3Int.back).Facets.FirstOrDefault();
            var house3 = house2.CG().Symmetrize(symmetryFace).SetGrammarStyle(sgStyles.RoomStyle);
        }

        public LevelElement Island()
        {
            var island = sgShapes.IslandExtrudeIter(grid[0, 0, 0].Group(), 13, 0.3f);

            // wall
            var wallTop = island.ExtrudeHor(true, false).Minus(island).MoveBy(Vector3Int.up).LE(AreaType.WallTop);
            var foundation = sgShapes.Foundation(wallTop);
            var total = new LevelGroupElement(grid, AreaType.None, island.LE(AreaType.Garden), foundation, wallTop);

            total.ApplyGrammarStyleRules(houseStyleRules);

            return total;
        }

        public LevelElement Houses()
        {
            // houses
            var flatBoxes = qc.FlatBoxes(3, 7, 5);
            var bounds = qc.GetFlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(15, 15)));
            var town = qc.RemoveOverlap(pl.PlaceInside(bounds, flatBoxes));
            town = qc.LiftRandomly(town, () => 2 + UnityEngine.Random.Range(1, 4));
            town = town.Select(g => sgShapes.SimpleHouseWithFoundation(g.CG(), 5).ApplyGrammarStyleRules(houseStyleRules));

            // house roofs
            var housesRoofs = town.Select(le =>
                qc.Partition(
                    (g, boundingG) => qc.GetRandomHorConnected1(g, boundingG),
                        le.CG()
                        .CubeGroupMaxLayer(Vector3Int.up),
                    3)
                );
            housesRoofs = housesRoofs.Select(houseRoof =>
                    qc.RaiseRandomly(houseRoof as LevelGroupElement, () => UnityEngine.Random.Range(1, 4))
                    .Select(part => sgShapes.TurnIntoHouse(part.CG()))
                    .SetChildrenAreaType(AreaType.Room));
            
            housesRoofs.ApplyGrammarStyleRules(houseStyleRules);
            

            // paths from room to roof
            /*town.LevelElements.ForEach(g =>
            {
                var upper = g.CubeGroup().WithFloor().CubeGroupMaxLayer(Vector3Int.up).ExtrudeHor(false, true);
                var lower = g.CubeGroup().WithFloor().CubeGroupMaxLayer(Vector3Int.down).ExtrudeHor(false, true);
                var searchSpace = g.CubeGroup().ExtrudeHor(false, true);
                Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.StairsNeighbors(), searchSpace);
                var path = paths.ConnectByPath(lower, upper, neighbors);
                path.SetGrammarStyle(sgStyles.StairsPathStyle);
            });*/

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
            town.ApplyGrammarStyleRules(houseStyleRules);

            return town;
        }

        public void RemovingOverlap()
        {
            var boxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(new Vector2Int(3, 3), new Vector2Int(5, 5)));
            var town = pl.MoveToNotOverlap(qc.FlatBoxes(boxSequence, 16));
            town = town.Select(le => le.SetAreaType(AreaType.Room));
            town.ApplyGrammarStyleRules(houseStyleRules);
        }

        public LevelElement Tower()
        {
            var towerLayout = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));
            var tower = sgShapes.Tower(towerLayout, 3, 4);
            tower = sgShapes.AddBalcony(tower);
            tower.ApplyGrammarStyleRules(houseStyleRules);


            // add paths between floors
            var floors = tower.WithAreaType(AreaType.Room);
            var floorsAndRoof = floors.Concat(tower.WithAreaType(AreaType.FlatRoof));
            floors.ForEach(floor =>
            {
                var upperFloor = floor.NeighborsInDirection(Vector3Int.up, floorsAndRoof).FirstOrDefault();
                if (upperFloor == null)
                    return;

                var lower = floor.CG().BottomLayer().CubeGroupMaxLayer(Vector3Int.down);
                var upper = upperFloor.CG().BottomLayer().CubeGroupMaxLayer(Vector3Int.down);
                var searchSpace = new CubeGroup(grid, floor.CG().ExtrudeHor(false, true).Cubes.Concat(upper.Cubes).ToList());
                Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.StairsNeighbors(), searchSpace);
                var path = paths.ConnectByPath(lower, upper, neighbors);
                path.SetGrammarStyle(sgStyles.StairsPathStyle);
            });
            return tower;
        }

        public LevelElement TwoConnectedTowers()
        {
            // Create ground layout of the tower
            var towerLayout = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));
            
            // Create and set towers to the world
            var tower = sgShapes.Tower(towerLayout, 3, 4);
            tower.ApplyGrammarStyleRules(houseStyleRules);
            var tower2 = tower.MoveBy(new Vector3Int(20, 0, 10)).ApplyGrammarStyleRules(houseStyleRules);

            // Create path between the towers
            var path = paths.WalkableWallPathH(tower, tower2, 1).ApplyGrammarStyleRules(houseStyleRules);

            // Add balcony to one of the towers
            // house rules are applied to the entire house again...
            // todo: apply them only to the parts that were added
            sgShapes.AddBalcony(tower).ApplyGrammarStyleRules(houseStyleRules);

            return tower.Merge(tower2, path);
        }

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

            towers.ForEach(tower => tower.ApplyGrammarStyleRules(houseStyleRules));
            towers.ForEach2Cycle((t1, t2) => paths.WalkableWallPathH(t1, t2, 1).ApplyGrammarStyleRules(houseStyleRules));

            return towers.ToLevelGroupElement(grid);
        }

        public LevelElement Surrounded()
        {
            var root = new LevelGroupElement(grid, AreaType.WorldRoot);

            // Create ground layout of the tower
            var yard = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)).InflateY(0, 1)).LE(AreaType.Garden);


            var boxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(new Vector2Int(3, 3), new Vector2Int(7, 7)));
            var houses = qc.FlatBoxes(boxSequence, 6).Select(le => le.SetAreaType(AreaType.House));
            
            houses = pl.SurroundWith(yard, houses);
            houses = houses.ReplaceLeafsGrp(g => g.AreaType == AreaType.House, g => sgShapes.SimpleHouseWithFoundation(g.CG(), 3));

            var wall = sgShapes.WallAround(yard, 2).Minus(houses);

            root = root.AddAll(yard, houses, wall);
            root.ApplyGrammarStyleRules(houseStyleRules);

            return root;
        }

        public void TestingPaths()
        {
            // testing paths
            var box = qc.GetBox(new Box3Int(new Vector3Int(0, 0, 0), new Vector3Int(10, 10, 10)));
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

        public LevelElement CurveDesign()
        {
            var CurveDesign = new CurvesLevelDesign(this);
            return CurveDesign.CreateLevel();
        }

        public LevelElement ShapeGrammarDesign()
        {
            var ShapeGrammarLevelDesign = new ShapeGrammarLevelDesign(this);
            return ShapeGrammarLevelDesign.CreateLevel();
        }

        public LevelElement LanguageDesign(Libraries lib, World world)
        {
            var LanguageLevelDesign = new LanguageLevelDesign(this, lib, world);
            return LanguageLevelDesign.CreateLevel();
        }

        public void SplitRoom()
        {
            var houseBox = qc.GetFlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 8)), 0);
            
            var box = houseBox.CG().ExtrudeVer(Vector3Int.up, 10).LE(AreaType.Room);
            var splitBox = box.SplitRel(Vector3Int.right, AreaType.None, 0.5f).SplitRel(Vector3Int.up, AreaType.Room, 0.5f);

            var wall = splitBox.Leafs().ElementAt(0);

            var empty = splitBox.Leafs().ElementAt(3);

            splitBox
                .ReplaceLeafsGrp(0, le => le.SetAreaType(AreaType.Wall))
                .ReplaceLeafsGrp(1, le => le.SetAreaType(AreaType.OpenRoom))
                .ReplaceLeafsGrp(3, le => le.SetAreaType(AreaType.Empty))
                .ApplyGrammarStyleRules(houseStyleRules);
        }

        public LevelElement PartlyBrokenFloorHouse()
        {
            var floorBox = qc.GetFlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)), 0);

            Func<LevelGeometryElement, LevelElement> brokenFloor = (floor) =>
            {
                var floorPlan = tr.SplittingFloorPlan(floor, 4);
                var partlyBrokenFloor = tr.PickAndConnect(floorPlan, tr.PickWithChance(0.5f)).ReplaceLeafs(le => le.AreaType != AreaType.Empty, le => le.SetAreaType(AreaType.Platform));
                return partlyBrokenFloor;
            };

            var house = floorBox.CG().ExtrudeVer(Vector3Int.up, 15).LE(AreaType.Room);
            var houseFloors = house.SplitRel(Vector3Int.up, AreaType.None, 0.2f, 0.5f, 0.7f).ReplaceLeafsGrp(_ => true, le => brokenFloor(le));

            houseFloors.ApplyGrammarStyleRules(houseStyleRules);

            return houseFloors;
        }

        public LevelElement JustHouse()
        {
            var floorBox = qc.GetFlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)), 0);
            var house = sgShapes.TurnIntoHouse(floorBox.CG().ExtrudeVer(Vector3Int.up, 3));

            house.ApplyGrammarStyleRules(houseStyleRules);
            return house;
        }

        public LevelElement DebugPlatform()
        {
            var platform = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1)).SetAreaType(AreaType.Platform);

            platform.ApplyGrammarStyleRules(houseStyleRules);
            return platform;
        }

        public LevelElement ConnectByElevator()
        {
            var bottomPlatform = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5)).SetAreaType(AreaType.Platform);
            var topPlatform = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(5, 10)).SetAreaType(AreaType.Platform);

            bottomPlatform.ApplyGrammarStyleRules(houseStyleRules);
            topPlatform.ApplyGrammarStyleRules(houseStyleRules);

            var path = con.ConnectByElevator(bottomPlatform, topPlatform);

            path.ApplyGrammarStyleRules(houseStyleRules);

            return topPlatform;
        }

        public LevelElement ConnectByDoor()
        {
            var room1 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5)).SetAreaType(AreaType.Room);
            var room2 = sgShapes.Room(new Box2Int(new Vector2Int(4, 0), new Vector2Int(8, 4)).InflateY(0, 5)).SetAreaType(AreaType.Room);

            room1.ApplyGrammarStyleRules(houseStyleRules);
            room2.ApplyGrammarStyleRules(houseStyleRules);

            var connection = con.ConnectByDoor(room1, room2);

            connection.ApplyGrammarStyleRules(houseStyleRules);

            return room2;
        }

        public LevelElement ConnectByWallStairs()
        {
            var bottomPlatform = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5));
            var topPlatform = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(5, 10));

            bottomPlatform.ApplyGrammarStyleRules(houseStyleRules);
            topPlatform.ApplyGrammarStyleRules(houseStyleRules);
            
            var path = con.ConnectByWallStairsOut(LevelElement.Empty(grid))(bottomPlatform, topPlatform);

            path.ApplyGrammarStyleRules(houseStyleRules);
            
            return topPlatform;
        }

        public LevelElement ConnectByOutsideStairs()
        {
            var room1 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5) + new Vector3Int(0, 2, 0));
            var room2 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5) + new Vector3Int(8, 4, 0));

            room1.ApplyGrammarStyleRules(houseStyleRules);
            room2.ApplyGrammarStyleRules(houseStyleRules);

            var foundation = sgShapes.Foundation(room1.Merge(room2));
            foundation.ApplyGrammarStyleRules(houseStyleRules);

            var path = con.ConnectByBalconyStairsOutside(foundation)(room1, room2);

            path.ApplyGrammarStyleRules(houseStyleRules);
            
            return room1.Merge(room2).Merge(path).Merge(foundation);
        }

        public LevelElement ConnectByBridge()
        {
            var room1 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5) + new Vector3Int(0, 4, 0));
            var room2 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5) + new Vector3Int(8, 4, 0));

            room1.ApplyGrammarStyleRules(houseStyleRules);
            room2.ApplyGrammarStyleRules(houseStyleRules);

            var foundation = sgShapes.Foundation(room1.Merge(room2));
            foundation.ApplyGrammarStyleRules(houseStyleRules);

            var path = con.ConnectByBridge(foundation)(room1, room2);

            path.ApplyGrammarStyleRules(houseStyleRules);

            return room1.Merge(room2).Merge(path).Merge(foundation);
        }

        public void CompositeHouse()
        {
            var house = sgShapes.CompositeHouse(/*new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)),*/ 6);
            house.ApplyGrammarStyleRules(houseStyleRules);
        }

        public LevelElement TestMoveInDistXZ()
        {
            var room1 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5));
            var room2 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(1, 5));

            room1.ApplyGrammarStyleRules(houseStyleRules);
            room2.MovesInDistanceXZ(room1, 1).TryMove().ApplyGrammarStyleRules(houseStyleRules);

            return room1;
        }
    }
}

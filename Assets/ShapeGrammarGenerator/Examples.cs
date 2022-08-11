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
        public Grid<Cube> grid { get; }
        public QueryContext qc { get; }
        public GridPrimitivesPlacement sgStyles { get; }
        public ShapeGrammarShapes sgShapes { get; }
        public Placement pl { get; }
        public Paths paths { get; }
        public Transformations tr { get; }
        public WorldChanging wc { get; }
        public Connections con { get; }

        public LevelDevelopmentKit(GeometricPrimitives gp, Transform worldParent, Libraries lib)
        {
            grid = new Grid<Cube>(new Vector3Int(20, 10, 20), (grid, pos) => new Cube(grid, pos));
            qc = new QueryContext(grid);
            sgStyles = new GridPrimitivesPlacement(grid);
            sgShapes = new ShapeGrammarShapes(grid);
            pl = new Placement(grid);
            paths = new Paths(grid);
            con = new Connections(grid);
            tr = new Transformations(this);
            AreaStyles.Initialize(new GridPrimitives(gp), sgStyles);
            wc = new WorldChanging(this);
        }
    }

    public class Examples : LevelDevelopmentKit
    {
        public Examples(GeometricPrimitives gp, Transform worldParent, Libraries lib) : base(gp, worldParent, lib)
        { }
        /*
        public void IslandAndHouses()
        {
            // island
            var island = sgShapes.IslandExtrudeIter(grid[0, 0, 0].Group(), 13, 0.3f);
            island.SetGrammarStyle(sgStyles.PlatformStyle);

            // house
            var house = sgShapes.SimpleHouseWithFoundation(new Box2Int(new Vector2Int(2, 2), new Vector2Int(8, 5)), 5);

            var houseBottom = house.WithAreaType(AreaStyles.Foundation()).FirstOrDefault().CG().CubeGroupLayer(Vector3Int.down);
            var houseToIslandDir = island.MinkowskiMinus(houseBottom).GetRandom();
            house = house.MoveBy(houseToIslandDir);

            house.ApplyGrammarStyleRules(houseStyleRules);

            // wall
            var wallTop = island.ExtrudeHor(true, false).Minus(island).MoveBy(Vector3Int.up).SetGrammarStyle(sgStyles.FlatRoofStyle).LE(AreaStyles.Wall());
            sgShapes.Foundation(wallTop).SetAreaType(AreaStyles.Foundation()).ApplyStyle();

            // balcony
            var balconyRoom = house.WithAreaType(AreaStyles.Room()).FirstOrDefault();
            var balcony = sgShapes.BalconyWide(balconyRoom.CG()).LE(AreaStyles.Balcony());
            house = house.ReplaceLeafsGrp(le => le == balconyRoom, le => new LevelGroupElement(grid, AreaStyles.None(), balconyRoom, balcony));
            house.WithAreaType(AreaStyles.Balcony()).FirstOrDefault().SetGrammarStyle(cg => sgStyles.BalconyStyle(cg, house.WithAreaType(AreaStyles.Room()).FirstOrDefault().CG()));

            // house 2
            var house2 = house.MoveBy(Vector3Int.right * 8).ApplyGrammarStyleRules(houseStyleRules);

            var symmetryFace = house2.CG()
                .CubeGroupLayer(Vector3Int.back).Cubes.FirstOrDefault().Group()
                .MoveBy(Vector3Int.back)
                .BoundaryFacesH(Vector3Int.back).Facets.FirstOrDefault();
            var house3 = house2.CG().Symmetrize(symmetryFace).SetGrammarStyle(sgStyles.RoomStyle);
        }
        */

        public LevelElement Island()
        {
            var island = sgShapes.IslandExtrudeIter(grid[0, 0, 0].Group(), 13, 0.3f);

            // wall
            var wallTop = island.ExtrudeHor(true, false).Minus(island).MoveBy(Vector3Int.up).LE(AreaStyles.WallTop());
            var foundation = sgShapes.Foundation(wallTop);
            var total = new LevelGroupElement(grid, AreaStyles.None(), island.LE(AreaStyles.Garden()), foundation, wallTop);

            total.ApplyGrammarStyleRules();

            return total;
        }

        public LevelElement Houses()
        {
            // houses
            var flatBoxes = qc.FlatBoxes(3, 7, 5);
            var bounds = qc.GetFlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(15, 15)));
            var town = qc.RemoveOverlap(pl.PlaceInside(bounds, flatBoxes));
            town = qc.LiftRandomly(town, () => 2 + UnityEngine.Random.Range(1, 4));
            town = town.Select(g => sgShapes.SimpleHouseWithFoundation(g.CG(), 5).ApplyGrammarStyleRules());

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
                    .SetChildrenAreaType(AreaStyles.Room()));
            
            housesRoofs.ApplyGrammarStyleRules();
            

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
            town.ApplyGrammarStyleRules();

            return town;
        }

        public void RemovingOverlap()
        {
            var boxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(new Vector2Int(3, 3), new Vector2Int(5, 5)));
            var town = pl.MoveToNotOverlap(qc.FlatBoxes(boxSequence, 16));
            town = town.Select(le => le.SetAreaType(AreaStyles.Room()));
            town.ApplyGrammarStyleRules();
        }
        /*
        public LevelElement Tower()
        {
            var towerLayout = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));
            var tower = sgShapes.Tower(towerLayout, 3, 4);
            tower = sgShapes.AddBalcony(tower);
            tower.ApplyGrammarStyleRules(houseStyleRules);


            // add paths between floors
            var floors = tower.WithAreaType(AreaStyles.Room());
            var floorsAndRoof = floors.Concat(tower.WithAreaType(AreaStyles.FlatRoof()));
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
        }*/

        public LevelElement TwoConnectedTowers()
        {
            // Create ground layout of the tower
            var towerLayout = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));
            
            // Create and set towers to the world
            var tower = sgShapes.Tower(towerLayout, 3, 4);
            tower.ApplyGrammarStyleRules();
            var tower2 = tower.MoveBy(new Vector3Int(20, 0, 10)).ApplyGrammarStyleRules();

            // Create path between the towers
            var path = paths.WalkableWallPathH(tower, tower2, 1).ApplyGrammarStyleRules();

            // Add balcony to one of the towers
            // house rules are applied to the entire house again...
            // todo: apply them only to the parts that were added
            sgShapes.AddBalcony(tower).ApplyGrammarStyleRules();

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

            towers.ForEach(tower => tower.ApplyGrammarStyleRules());
            towers.ForEach2Cycle((t1, t2) => paths.WalkableWallPathH(t1, t2, 1).ApplyGrammarStyleRules());

            return towers.ToLevelGroupElement(grid);
        }

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
            root.ApplyGrammarStyleRules();

            return root;
        }

        /*
        public void TestingPaths()
        {
            // testing paths
            var box = qc.GetBox(new Box3Int(new Vector3Int(0, 0, 0), new Vector3Int(10, 10, 10)));
            var start = box.CubeGroupLayer(Vector3Int.left);
            var end = box.CubeGroupLayer(Vector3Int.right);
            Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.StairsNeighbors(), box);
            var path = paths.ConnectByPath(start, end, neighbors);
            path.SetGrammarStyle(sgStyles.StairsPathStyle);
        }*/
        /*
        public void ControlPointDesign()
        {
            var controlPointsDesign = new ControlPointsLevelDesign(this);
            controlPointsDesign.CreateLevel();
        }*/
        /*
        public LevelElement CurveDesign()
        {
            var CurveDesign = new CurvesLevelDesign(this);
            return CurveDesign.CreateLevel();
        }
        */
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
            
            var box = houseBox.CG().ExtrudeVer(Vector3Int.up, 10).LE(AreaStyles.Room());
            var splitBox = box.SplitRel(Vector3Int.right, AreaStyles.None(), 0.5f).SplitRel(Vector3Int.up, AreaStyles.Room(), 0.5f);

            var wall = splitBox.Leafs().ElementAt(0);

            var empty = splitBox.Leafs().ElementAt(3);

            splitBox
                .ReplaceLeafsGrp(0, le => le.SetAreaType(AreaStyles.Wall()))
                .ReplaceLeafsGrp(1, le => le.SetAreaType(AreaStyles.OpenRoom()))
                .ReplaceLeafsGrp(3, le => le.SetAreaType(AreaStyles.Empty()))
                .ApplyGrammarStyleRules();
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

            houseFloors.ApplyGrammarStyleRules();

            return houseFloors;
        }

        public LevelElement JustHouse()
        {
            var floorBox = qc.GetFlatBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)), 0);
            var house = sgShapes.TurnIntoHouse(floorBox.CG().ExtrudeVer(Vector3Int.up, 3));

            house.ApplyGrammarStyleRules();
            return house;
        }

        public LevelElement DebugPlatform()
        {
            var platform = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1)).SetAreaType(AreaStyles.Platform());

            platform.ApplyGrammarStyleRules();
            return platform;
        }
        /*
        public LevelElement ConnectByElevator()
        {
            var bottomPlatform = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5)).SetAreaType(AreaStyles.Platform());
            var topPlatform = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(5, 10)).SetAreaType(AreaStyles.Platform());

            bottomPlatform.ApplyGrammarStyleRules(houseStyleRules);
            topPlatform.ApplyGrammarStyleRules(houseStyleRules);

            var path = con.ConnectByElevator(bottomPlatform, topPlatform);

            path.ApplyGrammarStyleRules(houseStyleRules);

            return topPlatform;
        }*/

        public LevelElement ConnectByDoor()
        {
            var room1 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5)).SetAreaType(AreaStyles.Room());
            var room2 = sgShapes.Room(new Box2Int(new Vector2Int(4, 0), new Vector2Int(8, 4)).InflateY(0, 5)).SetAreaType(AreaStyles.Room());

            room1.ApplyGrammarStyleRules();
            room2.ApplyGrammarStyleRules();

            var connection = con.ConnectByDoor(null, null)(room1, room2);

            connection.ApplyGrammarStyleRules();

            return room2;
        }

        public LevelElement ConnectByWallStairs()
        {
            var bottomPlatform = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5));
            var topPlatform = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(5, 10));

            bottomPlatform.ApplyGrammarStyleRules();
            topPlatform.ApplyGrammarStyleRules();
            
            var path = con.ConnectByWallStairsOut(LevelElement.Empty(grid), /*LevelElement.Empty(grid)*/null)(bottomPlatform, topPlatform);

            path.ApplyGrammarStyleRules();
            
            return topPlatform;
        }

        public LevelElement ConnectByOutsideStairs()
        {
            var room1 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5) + new Vector3Int(0, 2, 0));
            var room2 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5) + new Vector3Int(8, 4, 0));

            room1.ApplyGrammarStyleRules();
            room2.ApplyGrammarStyleRules();

            var foundation = sgShapes.Foundation(room1.Merge(room2));
            foundation.ApplyGrammarStyleRules();

            var path = con.ConnectByStairsOutside(foundation, /*LevelElement.Empty(grid)*/null)(room1, room2);

            path.ApplyGrammarStyleRules();
            
            return room1.Merge(room2).Merge(path).Merge(foundation);
        }

        public LevelElement ConnectByBridge()
        {
            var room1 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5) + new Vector3Int(0, 4, 0));
            var room2 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5) + new Vector3Int(8, 4, 0));

            room1.ApplyGrammarStyleRules();
            room2.ApplyGrammarStyleRules();

            var foundation = sgShapes.Foundation(room1.Merge(room2));
            foundation.ApplyGrammarStyleRules();

            var path = con.ConnectByBridge(foundation, null/*LevelElement.Empty(grid)*/)(room1, room2);

            path.ApplyGrammarStyleRules();

            return room1.Merge(room2).Merge(path).Merge(foundation);
        }

        public void CompositeHouse()
        {
            var house = sgShapes.CompositeHouse(/*new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)),*/ 6);
            house.ApplyGrammarStyleRules();
        }

        public LevelElement TestMoveInDistXZ()
        {
            var room1 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 5));
            var room2 = sgShapes.Room(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(1, 5));

            room1.ApplyGrammarStyleRules();
            room2.MovesInDistanceXZ(room1, 1).TryMove().ApplyGrammarStyleRules();

            return room1;
        }
    }
}

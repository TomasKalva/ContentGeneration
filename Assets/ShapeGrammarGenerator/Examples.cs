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
        public ShapeGrammarObjectStyle FountainheadStyle { get; }
        public Grid grid { get; }
        public QueryContext qc { get; }
        public ShapeGrammarStyles sgStyles { get; }
        public ShapeGrammarShapes sgShapes { get; }
        public StyleRules houseStyleRules { get; }

        public LevelDevelopmentKit(ShapeGrammarObjectStyle objectStyle)
        {
            FountainheadStyle = objectStyle;
            grid = new Grid(new Vector3Int(20, 10, 20));
            qc = new QueryContext(grid);
            sgStyles = new ShapeGrammarStyles(grid, FountainheadStyle);
            sgShapes = new ShapeGrammarShapes(grid);
            houseStyleRules = new StyleRules(
                new StyleRule(g => g.WithAreaType(AreaType.Room), g => g.SetGrammarStyle(sgStyles.RoomStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Roof), g => g.SetGrammarStyle(sgStyles.FlatRoofStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Foundation), g => g.SetGrammarStyle(sgStyles.FoundationStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Path), g => g.SetGrammarStyle(sgStyles.StairsPathStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Garden), g => g.SetGrammarStyle(sgStyles.GardenStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.WallTop), g => g.SetGrammarStyle(sgStyles.FlatRoofStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Wall), g => g.SetGrammarStyle(sgStyles.FoundationStyle)),
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
            var house = sgShapes.House(new Box2Int(new Vector2Int(2, 2), new Vector2Int(8, 5)), 5);

            var houseBottom = house.WithAreaType(AreaType.Foundation).FirstOrDefault().CubeGroup().CubesLayer(Vector3Int.down);
            var houseToIslandDir = island.MinkowskiMinus(houseBottom).GetRandom();
            house = house.MoveBy(houseToIslandDir);

            house.ApplyGrammarStyleRules(houseStyleRules);

            // wall
            var wallTop = island.ExtrudeHor(true, false).Minus(island).MoveBy(Vector3Int.up).SetGrammarStyle(sgStyles.FlatRoofStyle).LevelElement(AreaType.Wall);
            sgShapes.Foundation(wallTop).SetGrammarStyle(sgStyles.FoundationStyle);

            // balcony
            var balconyRoom = house.WithAreaType(AreaType.Room).FirstOrDefault();
            var balcony = sgShapes.BalconyWide(balconyRoom.CubeGroup()).LevelElement(AreaType.Balcony);
            house = house.ReplaceLeafs(le => le == balconyRoom, le => new LevelGroupElement(grid, AreaType.None, balconyRoom, balcony));
            house.WithAreaType(AreaType.Balcony).FirstOrDefault().SetGrammarStyle(cg => sgStyles.BalconyStyle(cg, house.WithAreaType(AreaType.Room).FirstOrDefault().CubeGroup()));

            // house 2
            var house2 = house.MoveBy(Vector3Int.right * 8).ApplyGrammarStyleRules(houseStyleRules);

            var symmetryFace = house2.CubeGroup()
                .CubesLayer(Vector3Int.back).Cubes.FirstOrDefault().Group()
                .MoveBy(Vector3Int.back)
                .BoundaryFacesH(Vector3Int.back).Facets.FirstOrDefault();
            var house3 = house2.CubeGroup().Symmetrize(symmetryFace).SetGrammarStyle(sgStyles.RoomStyle);
        }

        public void Houses()
        {
            // houses
            var town = qc.GetOverlappingBoxes(new Box2Int(new Vector2Int(0, 0), new Vector2Int(15, 15)), 5);
            town = qc.LiftRandomly(town, () => 2 + UnityEngine.Random.Range(1, 4));
            town = town.Select(g => sgShapes.House(g.CubeGroup(), 5).ApplyGrammarStyleRules(houseStyleRules));

            // house roofs
            var housesRoofs = town.Select(le =>
                qc.Partition(
                    (g, boundingG) => qc.GetRandomHorConnected1(g, boundingG),
                        le.CubeGroup()
                        .CubesMaxLayer(Vector3Int.up),
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
                var upper = g.CubeGroup().WithFloor().CubesMaxLayer(Vector3Int.up).ExtrudeHor(false, true);
                var lower = g.CubeGroup().WithFloor().CubesMaxLayer(Vector3Int.down).ExtrudeHor(false, true);
                var searchSpace = g.CubeGroup().ExtrudeHor(false, true);
                Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.StairsNeighbors(), searchSpace);
                var path = sgShapes.ConnectByPath(lower, upper, neighbors);
                path.SetGrammarStyle(sgStyles.StairsPathStyle);
            });
        }

        public void NonOverlappingTown()
        {
            var town = qc.RemoveOverlap(qc.GetOverlappingBoxes(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)), 8));
            town = town.Select(le => le.SetAreaType(AreaType.Room));
            town.ApplyGrammarStyleRules(houseStyleRules);
        }

        public void RemovingOverlap()
        {
            var boxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(new Vector2Int(3, 3), new Vector2Int(5, 5)));
            var town = qc.RemoveOverlap(qc.FlatBoxes(boxSequence, 16));
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

                var lower = floor.CubeGroup().WithFloor().CubesMaxLayer(Vector3Int.down);
                var upper = upperFloor.CubeGroup().WithFloor().CubesMaxLayer(Vector3Int.down);
                var searchSpace = new CubeGroup(grid, floor.CubeGroup().ExtrudeHor(false, true).Cubes.Concat(upper.Cubes).ToList());
                Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.StairsNeighbors(), searchSpace);
                var path = sgShapes.ConnectByPath(lower, upper, neighbors);
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
            sgShapes.WallPathH(tower, tower2, 1).ApplyGrammarStyleRules(houseStyleRules);

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
            towers.ForEach2Cycle((t1, t2) => sgShapes.WallPathH(t1, t2, 3).ApplyGrammarStyleRules(houseStyleRules));
        }

        public void Surrounded()
        {
            var root = new LevelGroupElement(grid, AreaType.WorldRoot);

            // Create ground layout of the tower
            var yard = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)).InflateY(0, 1)).LevelElement(AreaType.Garden);


            var boxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(new Vector2Int(3, 3), new Vector2Int(7, 7)));
            var houses = qc.FlatBoxes(boxSequence, 6).Select(le => le.SetAreaType(AreaType.House));
            
            houses = qc.SurroundWith(yard, houses);
            houses = houses.ReplaceLeafsGrp(g => g.AreaType == AreaType.House, g => sgShapes.House(g.CubeGroup(), 3));

            var wall = sgShapes.WallAround(yard, 2).Minus(houses);

            root = root.AddAll(yard, houses, wall);
            root.ApplyGrammarStyleRules(houseStyleRules);
        }

        public void TestingPaths()
        {
            // testing paths
            var box = sgShapes.Room(new Box3Int(new Vector3Int(0, 0, 0), new Vector3Int(10, 10, 10)));
            var start = box.CubesLayer(Vector3Int.left);
            var end = box.CubesLayer(Vector3Int.right);
            Neighbors<PathNode> neighbors = PathNode.BoundedBy(PathNode.StairsNeighbors(), box);
            var path = sgShapes.ConnectByPath(start, end, neighbors);
            path.SetGrammarStyle(sgStyles.StairsPathStyle);
        }

        public void ControlPointDesign()
        {
            var controlPointsDesign = new ControlPointsLevelDesign(this);
            controlPointsDesign.CreateLevel();
        }
    }
}

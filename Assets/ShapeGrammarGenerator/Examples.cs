using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class Examples
    {
        public ShapeGrammarObjectStyle FountainheadStyle { get; }
        public Grid grid { get; }
        public QueryContext qc { get; }
        public ShapeGrammarStyles sgStyles { get; }
        public ShapeGrammarShapes sgShapes { get; }
        public StyleRules houseStyleRules { get; }

        public Examples(ShapeGrammarObjectStyle objectStyle)
        {
            FountainheadStyle = objectStyle;
            grid = new Grid(new Vector3Int(20, 10, 20));
            qc = new QueryContext(grid);
            sgStyles = new ShapeGrammarStyles(grid, FountainheadStyle);
            sgShapes = new ShapeGrammarShapes(grid);
            houseStyleRules = new StyleRules(
                new StyleRule(g => g.WithAreaType(AreaType.Room), g => g.SetGrammarStyle(sgStyles.RoomStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Roof), g => g.SetGrammarStyle(sgStyles.FlatRoofStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Foundation), g => g.SetGrammarStyle(sgStyles.FoundationStyle))
            );
        }
        
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
            var wallTop = island.ExtrudeHor(true, false).Minus(island).MoveBy(Vector3Int.up).SetGrammarStyle(sgStyles.FlatRoofStyle);
            sgShapes.Foundation(wallTop.MoveBy(Vector3Int.down)).SetGrammarStyle(sgStyles.FoundationStyle);

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

        public void Tower()
        {
            var towerLayout = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));
            var tower = sgShapes.Tower(towerLayout, 3, 4);
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

        public void ConnectedTowers()
        {
            var towerLayout = qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));
            var tower = sgShapes.Tower(towerLayout, 3, 4);
            tower.ApplyGrammarStyleRules(houseStyleRules);
            var tower2 = tower.MoveBy(new Vector3Int(20, 0, 10)).ApplyGrammarStyleRules(houseStyleRules);
            
            var starting = tower.CubeGroup().WithFloor();
            var ending = tower2.CubeGroup().WithFloor();
            var forbidden = tower;

            var x = starting.Cubes.Where(c => c.Position == new Vector3Int(4, 13, 4));
            Neighbors<PathNode> neighbors = PathNode.NotIn(PathNode.HorizontalNeighbors(), forbidden.CubeGroup());
            var path = sgShapes.ConnectByPath(starting, ending, neighbors);
            path.SetGrammarStyle(sgStyles.StairsPathStyle);
            

            // add paths between floors
            /*var floors = tower.WithAreaType(AreaType.Room);
            var floorsAndRoof = floors.Concat(tower.WithAreaType(AreaType.Roof));
            floors.ForEach(floor =>
            {
                var upperFloor = floor.NeighborsInDirection(Vector3Int.up, floorsAndRoof).FirstOrDefault();
                if (upperFloor == null)
                    return;

                var lower = floor.CubeGroup().WithFloor().CubesMaxLayer(Vector3Int.down);
                var upper = upperFloor.CubeGroup().WithFloor().CubesMaxLayer(Vector3Int.down);
                var searchSpace = new CubeGroup(grid, floor.CubeGroup().ExtrudeHor(false, true).Cubes.Concat(upper.Cubes).ToList());
                var path = sgShapes.ConnectByPath(lower, upper, searchSpace);
                path.SetGrammarStyle(sgStyles.StairsPathStyle);
            });*/
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
    }
}

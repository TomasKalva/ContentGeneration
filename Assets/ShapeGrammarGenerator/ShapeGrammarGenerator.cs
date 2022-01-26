using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ShapeGrammar.Grid;
using System.Linq;
using Assets.Util;

namespace ShapeGrammar
{
    public class ShapeGrammarGenerator : WorldGenerator
    {
        [SerializeField]
        Transform parent;

        [SerializeField]
        ShapeGrammarObjectStyle FountainheadStyle;

        private void Start()
        {
            // Keep scene view
            if (Application.isEditor)
            {
                UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
            }
            //UnityEngine.Random.InitState(13);

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Debug.Log("Generating world");
            
            var grid = new Grid(new Vector3Int(20, 10, 20));
            var qc = new QueryContext(grid);

            var sgStyles = new ShapeGrammarStyles(grid, FountainheadStyle);
            var sgShapes = new ShapeGrammarShapes(grid);
            
            var houseStyleRules = new StyleRules(
                new StyleRule(g => g.WithAreaType(AreaType.Room), g => g.SetGrammarStyle(sgStyles.RoomStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Roof), g => g.SetGrammarStyle(sgStyles.FlatRoofStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Foundation), g => g.SetGrammarStyle(sgStyles.FoundationStyle))
                );

            /*
            // island
            var island = sgShapes.IslandExtrudeIter(grid[0,0,0].Group(AreaType.Garden), 13, 0.3f);
            island.SetGrammarStyle(sgStyles.PlatformStyle);
            
            // house
            var house = sgShapes.House(new Box2Int(new Vector2Int(2, 2), new Vector2Int(8, 5)), 5);
            
            var houseBottom = house.WithAreaType(AreaType.Foundation).CubeGroup().CubesLayer(Vector3Int.down);
            var houseToIslandDir = island.MinkowskiMinus(houseBottom).GetRandom();
            house = house.MoveBy(houseToIslandDir);

            house.ApplyGrammarStyleRules(houseStyleRules);

            // wall
            var wallTop = island.ExtrudeHor(1, false).Minus(island).MoveBy(Vector3Int.up).SetGrammarStyle(sgStyles.FlatRoofStyle);
            sgShapes.Foundation(wallTop.MoveBy(Vector3Int.down)).SetGrammarStyle(sgStyles.FoundationStyle);

            // balcony
            var balcony = sgShapes.BalconyWide(house.WithAreaType(AreaType.Room).CubeGroup());
            house = house.Add(balcony);
            house.WithAreaType(AreaType.Balcony).SetGrammarStyle(cg => sgStyles.BalconyStyle(cg, house.WithAreaType(AreaType.Room).CubeGroup()));
            
            // house 2
            var house2 = house.MoveBy(Vector3Int.right * 8).ApplyGrammarStyleRules(houseStyleRules);

            var symmetryFace = house2.CubeGroup()
                .CubesLayer(Vector3Int.back).Cubes.FirstOrDefault().Group(AreaType.None)
                .MoveBy(Vector3Int.back)
                .BoundaryFacesH(Vector3Int.back).Facets.FirstOrDefault();
            var house3 = house2.CubeGroup().Symmetrize(symmetryFace).SetGrammarStyle(sgStyles.RoomStyle);
            */

            // houses
            /*
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
                    .Select(part => sgShapes.TurnToHouse(part.CubeGroup()))
                    .SetChildrenAreaType(AreaType.Room));

            housesRoofs.ApplyGrammarStyleRules(houseStyleRules);
            
            // paths from room to roof
            town.LevelElements.ForEach(g =>
            {
                var upper = g.CubeGroup().WithFloor().CubesMaxLayer(Vector3Int.up).ExtrudeHor(-1, true);
                var lower = g.CubeGroup().WithFloor().CubesMaxLayer(Vector3Int.down).ExtrudeHor(-1, true);
                var path = sgShapes.ConnectByPath(lower, upper, g.CubeGroup().ExtrudeHor(-1, true));
                path.SetGrammarStyle(sgStyles.StairsPathStyle);
            });*/

            /*
            var town = qc.GetNonOverlappingBoxes(new Box2Int(new Vector2Int(0, 0), new Vector2Int(15, 15)), 5);
            town = town.Select(g => g.CubeGroup().SetGrammarStyle(sgStyles.RoomStyle).LevelElement(AreaType.Room));
            */

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
                var path = sgShapes.ConnectByPath(lower, upper, searchSpace);
                path.SetGrammarStyle(sgStyles.StairsPathStyle);
            });

            /*var first = town.LevelElements.FirstOrDefault();
            var second = town.LevelElements.LastOrDefault();
            //var moves = first.Moves(second.CubeGroup().ExtrudeHor().LevelElement(AreaType.None), new LevelElement[1] { second });
            var moves = first.MovesToBeInside(second);
            first = first.MoveBy(moves.ElementAt(1));
            first.SetGrammarStyle(sgStyles.RoomStyle);
            second.SetGrammarStyle(sgStyles.RoomStyle);
            */

            /*
            // testing paths
            
            var box = sgShapes.Room(new Box3Int(new Vector3Int(0, 0, 0), new Vector3Int(10, 10, 10)));
            var start = box.CubesLayer(Vector3Int.left);
            var end = box.CubesLayer(Vector3Int.right);
            var path = sgShapes.ConnectByPath(start, end, box);
            path.SetGrammarStyle(sgStyles.StairsPathStyle);
            */

            grid.Generate(2f, parent);

            stopwatch.Stop();
            Debug.Log(stopwatch.ElapsedMilliseconds);
        }

        public override void Generate(World world)
        {
            //world.AddEnemy(libraries.Enemies.MayanSwordsman(), new Vector3(0, 1, 0));
            world.AddEnemy(libraries.Enemies.DragonMan(), new Vector3(0, 1, 0));

            Debug.Log("Generating world");

            world.AddInteractiveObject(interactiveObjects.bonfire, new Vector3(0, 0, 0));
        }


    }
}
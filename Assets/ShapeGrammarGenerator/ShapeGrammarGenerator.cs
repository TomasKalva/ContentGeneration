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
            //UnityEngine.Random.InitState(11);

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
            
            var town = qc.GetOverlappingBoxes(new Box2Int(new Vector2Int(0, 0), new Vector2Int(15, 15)), 5);
            town = qc.LiftRandomly(town, () => 2 + UnityEngine.Random.Range(1, 4));


            town = town.Select(g => sgShapes.House(g.CubeGroup(), 5).ApplyGrammarStyleRules(houseStyleRules));
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
            town.LevelElements.ForEach(g =>
            {
                var upper = g.CubeGroup().WithFloor().CubesMaxLayer(Vector3Int.up).ExtrudeHor(-1, true);
                var lower = g.CubeGroup().WithFloor().CubesMaxLayer(Vector3Int.down).ExtrudeHor(-1, true);
                var path = sgShapes.ConnectByPath(lower, upper, g.CubeGroup().ExtrudeHor(-1, true));
                path.SetGrammarStyle(sgStyles.StairsPathStyle);
            });

            /*
            var house = sgShapes.House(qc.GetBox(new Box3Int(new Vector3Int(0, 1, 0), new Vector3Int(4, 2, 4))), 8).ApplyGrammarStyleRules(houseStyleRules);
            var upper = house.CubeGroup().WithFloor().CubesMaxLayer(Vector3Int.up).ExtrudeHor(-1, true);
            var lower = house.CubeGroup().WithFloor().CubesMaxLayer(Vector3Int.down).ExtrudeHor(-1, true);
            
            var path = sgShapes.ConnectByPath(lower, upper, house.CubeGroup().ExtrudeHor(-1, true));
            path.SetGrammarStyle(sgStyles.StairsPathStyle);
            */
            

            /*var box = sgShapes.Room(new Box3Int(new Vector3Int(0, 0, 0), new Vector3Int(10, 10, 10)));
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
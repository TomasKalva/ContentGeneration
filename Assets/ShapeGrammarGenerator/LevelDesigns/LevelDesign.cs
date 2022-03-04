using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using static ShapeGrammar.WorldState;

namespace ShapeGrammar
{
    public abstract class LevelDesign
    {
        public LevelDevelopmentKit ldk { get; }

        protected LevelDesign(LevelDevelopmentKit ldk)
        {
            this.ldk = ldk;
        }

        public abstract LevelElement CreateLevel();
    }

    
    public class CurvesLevelDesign : LevelDesign
    {


        public CurvesLevelDesign(LevelDevelopmentKit ldk) : base(ldk)
        {
        }

        ChangeWorld LinearCurveDesign(LevelElement start, int length)
        {
            return addingState =>
            {
                var group = new LevelGroupElement(ldk.grid, AreaType.None);

                Func<LevelElement> smallBox = () =>
                {
                    var smallDistr = new SlopeDistr(center: 5, width: 3, rightness: 0.3f);
                    return ldk.sgShapes.TurnIntoHouse(ldk.qc.GetFlatBox(ExtensionMethods.RandomBox(smallDistr, smallDistr)).CubeGroup().ExtrudeVer(Vector3Int.up, 6));
                };

                Func<LevelElement> largeBox = () =>
                {
                    var largeDistr = new SlopeDistr(center: 8, width: 3, rightness: 0.3f);
                    return ldk.sgShapes.TurnIntoHouse(ldk.qc.GetFlatBox(ExtensionMethods.RandomBox(largeDistr, largeDistr)).CubeGroup().ExtrudeVer(Vector3Int.up, 6));
                };

                Func<LevelElement> balconyTower = () =>
                {
                    // Create ground layout of the tower
                    var towerLayout = ldk.qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(4, 4)).InflateY(0, 1));

                    // Create and set towers to the world
                    var tower = ldk.sgShapes.Tower(towerLayout, 3, 4);

                    tower = ldk.sgShapes.AddBalcony(tower);
                    return tower;
                };

                Func<LevelElement> tower = () =>
                {
                    var size = UnityEngine.Random.Range(3, 4);
                    var floorsCount = UnityEngine.Random.Range(3, 6);

                    // Create ground layout of the tower
                    var towerLayout = ldk.qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(size, size)).InflateY(0, 1));

                    // Create and set towers to the world
                    var tower = ldk.sgShapes.Tower(towerLayout, 3, floorsCount);
                    return tower;
                };


                Func<LevelElement> surrounded = () =>
                {
                    var surrounded = new LevelGroupElement(ldk.grid, AreaType.None);

                    // Create ground layout of the tower
                    var yard = ldk.qc.GetBox(new Box2Int(new Vector2Int(0, 0), new Vector2Int(10, 10)).InflateY(0, 1)).LevelElement(AreaType.Garden);

                    var boxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(new Vector2Int(3, 3), new Vector2Int(7, 7)));
                    var houses = ldk.qc.FlatBoxes(boxSequence, 6).Select(le => le.SetAreaType(AreaType.House));

                    houses = ldk.pl.SurroundWith(yard, houses);
                    houses = houses.ReplaceLeafsGrp(g => g.AreaType == AreaType.House, g => ldk.sgShapes.SimpleHouseWithFoundation(g.CubeGroup(), 3));

                    var wall = ldk.sgShapes.WallAround(yard, 2).Minus(houses);

                    surrounded = surrounded.AddAll(yard.CubeGroup().ExtrudeVer(Vector3Int.up, 2).Merge(yard.CubeGroup()).LevelElement(AreaType.Garden), houses, wall);
                    return surrounded;
                };

                Func<LevelElement> island = () =>
                {
                    var island = ldk.sgShapes.IslandExtrudeIter(ldk.grid[0, 0, 0].Group(), 13, 0.3f);

                    // wall
                    var wallTop = island.ExtrudeHor(true, false).Minus(island).MoveBy(Vector3Int.up).LevelElement(AreaType.WallTop);
                    var foundation = ldk.sgShapes.Foundation(wallTop).LevelElement(AreaType.Foundation);
                    return new LevelGroupElement(ldk.grid, AreaType.None, island.LevelElement(AreaType.Garden), foundation, wallTop);
                };

                var wc = ldk.wc;

                var adders = new List<ChangeWorld>()
                {
                     wc.AddNearXZ(smallBox),
                     wc.AddNearXZ(largeBox),
                     wc.AddNearXZ(balconyTower),
                     //wc.AddNearXZ(surrounded),
                     //wc.AddNearXZ(island),
                     //wc.AddRemoveOverlap(largeBox),
                     wc.PathTo(smallBox),
                     //wc.PathTo(tower),
                }.Shuffle().ToList();

                return addingState.ChangeAll(Enumerable.Range(0, length).Select(i => adders[i % adders.Count]));
            };
        }

        ChangeWorld SplittingPath(LevelElement start, int length)
        {
            return addingState =>
            {
                var pathMaker = LinearCurveDesign(start, length / 3);
                var first = pathMaker(addingState);
                var branches = pathMaker(first);
                branches = pathMaker(branches.SetElement(first.Current));
                return branches;
            };
        }

        public override LevelElement CreateLevel()
        {
            int length = 6;

            var start = ldk.qc.GetBox(new Box3Int(Vector3Int.zero, Vector3Int.one)).ExtrudeVer(Vector3Int.up, 10).LevelElement();
            WorldState state = new WorldState(start, ldk.grid, le => le.ApplyGrammarStyleRules(ldk.houseStyleRules)/*.MoveBy((heightDistr.Sample() - le.CubeGroup().CubeGroupMaxLayer(Vector3Int.down).Cubes.FirstOrDefault().Position.y)* Vector3Int.up)*/);
            

            var addedLine = LinearCurveDesign(start, length)(state);

            /*
            var changers = Enumerable.Repeat(SplitToFloors(), 100).Concat(
                    Enumerable.Repeat(SubdivideRoom(), 100)
                ).ToList();
            
            var subdividedRooms = addedLine.ChangeAll(changers);
            
            subdividedRooms.Added.ApplyGrammarStyleRules(ldk.houseStyleRules);

            var connectednessGraph = new Graph<LevelGeometryElement>(subdividedRooms.Added.Leafs().ToList(), new List<Edge<LevelGeometryElement>>());
            
            var connectedLine = subdividedRooms.AddUntilCan(ConnectTwoUnconnected(connectednessGraph, subdividedRooms.Added), 1000);
            */
            var levelElements = addedLine.Added;
            


            //levelElements.ApplyGrammarStyleRules(ldk.houseStyleRules);

            return levelElements;
        }
    }
}

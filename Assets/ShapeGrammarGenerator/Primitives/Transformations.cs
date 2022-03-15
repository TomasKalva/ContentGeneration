using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class Transformations
    {
        LevelDevelopmentKit ldk { get; }

        public Transformations(LevelDevelopmentKit ldk)
        {
            this.ldk = ldk;
        }

        public delegate LevelElement FloorConnector(LevelGeometryElement start, LevelGeometryElement middle, LevelGeometryElement end);
        public delegate LevelGroupElement FloorPartitioner(LevelGeometryElement floor);
        public delegate LevelGroupElement Picker(LevelGroupElement levelElement);

        public LevelGroupElement SubdivideRoom(LevelGeometryElement box, Vector3Int horDir, float width)
        {

            var splitBox = box.SplitRel(horDir, AreaType.None, width).SplitRel(Vector3Int.up, AreaType.Room, 0.5f);
            return splitBox
                .ReplaceLeafsGrp(0, le => le.SetAreaType(AreaType.Wall))
                .ReplaceLeafsGrp(1, le => le.SetAreaType(AreaType.OpenRoom))
                .ReplaceLeafsGrp(3, le => le.SetAreaType(AreaType.Empty));
        }

        public LevelGroupElement SplittingFloorPlan(LevelGeometryElement box, int maxRoomSize)
        {
            return ldk.qc.RecursivelySplitXZ(box, maxRoomSize).Leafs().ToLevelGroupElement(box.Grid);
        }

        public Picker PickWithChance(float pickProb)
        {
            return lge =>
                lge.LevelElements.Select(le => UnityEngine.Random.Range(0f, 1f) < pickProb ? le : le.SetAreaType(AreaType.Empty)).ToLevelGroupElement(lge.Grid);
        }

        public Picker Dropper(int dropCount)
        {
            return lge =>
                lge.LevelElements.Shuffle().Select((le, i) => i >= dropCount ? le : le.SetAreaType(AreaType.Empty)).ToLevelGroupElement(lge.Grid);
        }

        public Picker DropUntilDisconnected(LevelElement start, LevelElement middle, LevelElement end)
        {
            return lge =>
            {
                var path = ldk.con.ConnectByStairsInside(start, end, middle).CubeGroup();
                return lge.Leafs().Where(le => le.CubeGroup().Intersects(path)).ToLevelGroupElement(lge.Grid);
            };
        }

        public LevelGroupElement PickAndConnect(LevelGroupElement floorPlan, Picker picker)
        {
            var picked = picker(floorPlan);
            var nonEmpty = picked.NonEmpty().CubeGroup().SplitToConnected().ToLevelGroupElement(floorPlan.Grid);
            var empty = picked.Empty().CubeGroup().SplitToConnected().ToLevelGroupElement(floorPlan.Grid).ReplaceLeafs(_ => true, le => le.SetAreaType(AreaType.Empty));
            return nonEmpty.Merge(empty);
        }

        public LevelGroupElement FloorHouse(LevelGeometryElement box, Func<LevelGeometryElement, LevelElement> floorCreator, params int[] floors)
        {
            var floorBox = box.CubeGroup().CubeGroupMaxLayer(Vector3Int.down);
            var house = box;
            var allFloors = house.Split(Vector3Int.up, AreaType.None, floors);
            var houseFloors = allFloors.ReplaceLeafsGrp(le => le != allFloors.Leafs().ElementAt(0), le => floorCreator(le))
                    .ReplaceLeafsGrp(0, le => le.SetAreaType(AreaType.Room));
            
            return houseFloors;
        }

        public LevelGroupElement BrokenFloor(LevelGeometryElement box)
        {
            var floorPlan = SplittingFloorPlan(box, 3);
            var partlyBrokenFloor = PickAndConnect(floorPlan, Dropper(2)).ReplaceLeafsGrp(le => le.AreaType != AreaType.Empty, le => le.SetAreaType(AreaType.Platform));
            return partlyBrokenFloor;
        }

        public FloorConnector GetFloorConnector(FloorPartitioner partitioner) 
        {
            return (start, middle, end) => 
            {
                Picker picker = DropUntilDisconnected(start, middle, end);
                return PickAndConnect(partitioner(middle), picker).Leafs().Where(le => le.AreaType != AreaType.Empty).ToLevelGroupElement(start.Grid);
            };
        }
    }
}

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

        public delegate LevelGroupElement Picker(LevelGroupElement levelElement);

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
            var houseFloors = house.Split(Vector3Int.up, AreaType.None, floors).ReplaceLeafsGrp(_ => true, le => floorCreator(le));

            return houseFloors;
        }

        public LevelElement BrokenFloor(LevelGeometryElement box)
        {
            var floorPlan = SplittingFloorPlan(box, 3);
            var partlyBrokenFloor = PickAndConnect(floorPlan, PickWithChance(0.5f)).ReplaceLeafs(le => le.AreaType != AreaType.Empty, le => le.SetAreaType(AreaType.Platform));
            return partlyBrokenFloor;
        }
    }
}

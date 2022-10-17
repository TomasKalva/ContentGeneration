﻿using Assets.ShapeGrammarGenerator;
using System;
using System.Linq;
using UnityEngine;

namespace ShapeGrammar
{
    /// <summary>
    /// Doesn't throw correct exceptions.
    /// </summary>
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

            var splitBox = box.SplitRel(horDir, AreaStyles.None(), width).SplitRel(Vector3Int.up, AreaStyles.Room(), 0.5f);
            return splitBox
                .ReplaceLeafsGrp(0, le => le.SetAreaType(AreaStyles.Wall()))
                .ReplaceLeafsGrp(1, le => le.SetAreaType(AreaStyles.OpenRoom()))
                .ReplaceLeafsGrp(3, le => le.SetAreaType(AreaStyles.Empty()));
        }

        public LevelGroupElement SplittingFloorPlan(LevelGeometryElement box, int maxRoomSize)
        {
            return ldk.qc.RecursivelySplitXZ(box, maxRoomSize).Leafs().ToLevelGroupElement(box.Grid);
        }

        public Picker PickWithChance(float pickProb)
        {
            return lge =>
                lge.LevelElements.Select(le => MyRandom.Range(0f, 1f) < pickProb ? le : le.SetAreaType(AreaStyles.Empty())).ToLevelGroupElement(lge.Grid);
        }

        public Picker Dropper(int dropCount)
        {
            return lge =>
                lge.LevelElements.Shuffle().Select((le, i) => i >= dropCount ? le : le.SetAreaType(AreaStyles.Empty())).ToLevelGroupElement(lge.Grid);
        }

        public Picker DropUntilDisconnected(LevelElement start, LevelElement middle, LevelElement end)
        {
            return lge =>
            {
                var path = ldk.con.ConnectByStairsInside(null, null)(start, end/*, middle*/).CG();
                return lge.Leafs().Where(le => le.CG().Intersects(path)).ToLevelGroupElement(lge.Grid);
            };
        }

        public LevelGroupElement PickAndConnect(LevelGroupElement floorPlan, Picker picker)
        {
            var picked = picker(floorPlan);
            var nonEmpty = picked.NonEmpty().CG().SplitToConnected().ToLevelGroupElement(floorPlan.Grid);
            var empty = picked.Empty().CG().SplitToConnected().ToLevelGroupElement(floorPlan.Grid).ReplaceLeafs(_ => true, le => le.SetAreaType(AreaStyles.Empty()));
            return nonEmpty.Merge(empty);
        }

        public LevelGroupElement FloorHouse(LevelGeometryElement box, Func<LevelGeometryElement, LevelElement> floorCreator, params int[] floors)
        {
            var floorBox = box.CG().CubeGroupMaxLayer(Vector3Int.down);
            var house = box;
            var allFloors = house.Split(Vector3Int.up, AreaStyles.None(), floors);
            var houseFloors = allFloors.ReplaceLeafsGrp(le => le != allFloors.Leafs().ElementAt(0), le => floorCreator(le))
                    .ReplaceLeafsGrp(0, le => le.SetAreaType(AreaStyles.Room()));
            
            return houseFloors;
        }

        public LevelGroupElement BrokenFloor(LevelGeometryElement box)
        {
            var floorPlan = SplittingFloorPlan(box, 3);
            var partlyBrokenFloor = PickAndConnect(floorPlan, Dropper(2)).ReplaceLeafsGrp(le => le.AreaStyle != AreaStyles.Empty(), le => le.SetAreaType(AreaStyles.Platform()));
            return partlyBrokenFloor;
        }

        public FloorConnector GetFloorConnector(FloorPartitioner partitioner) 
        {
            return (start, middle, end) => 
            {
                Picker picker = DropUntilDisconnected(start, middle, end);
                return PickAndConnect(partitioner(middle), picker).Leafs().Where(le => le.AreaStyle != AreaStyles.Empty()).ToLevelGroupElement(start.Grid);
            };
        }
    }
}

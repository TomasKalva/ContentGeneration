using OurFramework.Environment.GridMembers;
using OurFramework.Environment.StylingAreas;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OurFramework.Environment.ShapeCreation
{
    public class LEShapes
    {
        Grid<Cube> QueriedGrid { get; }
        CGShapes cgs { get; }

        public LEShapes(Grid<Cube> grid)
        {
            QueriedGrid = grid;
            cgs = new CGShapes(grid);
        }

        public LevelElement Room(Box3Int roomArea) => cgs.Box(roomArea).LE(AreaStyles.Room());

        public LevelElement Room(int width, int depth, int height = 1) => Box(width, depth, height).SetAreaStyle(AreaStyles.Room());

        public LevelGeometryElement Box(int width, int depth, int height = 1) => cgs.Box(new Box2Int(0, 0, width, depth).InflateY(0, height)).LE();

        public LevelGroupElement Tower(CubeGroup belowFirstFloor, int floorHeight, int floorsCount)
        {
            var belowEl = belowFirstFloor.LE(AreaStyles.Foundation()); 
            var towerEl = new LevelGroupElement(belowFirstFloor.Grid, AreaStyles.None(), belowEl);
            // create floors of the tower
            var floors = Enumerable.Range(0, floorsCount).Aggregate(towerEl,
                            (fls, _) =>
                            {
                                var newFloor = fls.CG().ExtrudeVer(Vector3Int.up, floorHeight).LE(AreaStyles.Room());
                                return fls.Add(newFloor);
                            });
            // add roof
            var tower = floors.Add(floors.CG().ExtrudeVer(Vector3Int.up, 1).LE(AreaStyles.FlatRoof()));

            return tower;
        }

        public LevelGroupElement TurnIntoHouse(CubeGroup house)
        {
            var roof = house.CubeGroupMaxLayer(Vector3Int.up).LE(AreaStyles.GableRoof());
            var room = house.Minus(roof.CG()).LE(AreaStyles.Room());
            var foundation = Foundation(room);
            return new LevelGroupElement(QueriedGrid, AreaStyles.Room(), foundation, room, roof);
        }

        public LevelElement Foundation(LevelElement toBeFounded) => 
            cgs.MoveDownUntilGround(
                toBeFounded.CG()
                    .CubeGroupLayer(Vector3Int.down)
                    .MoveBy(Vector3Int.down))
            .LE(AreaStyles.Foundation());

        public LevelElement BridgeFoundation(LevelElement bridgeToBeFounded, Vector3Int bridgeDirection)
        {
            var foundation = cgs.MoveDownUntilGround(
                bridgeToBeFounded.CG()
                    .CubeGroupLayer(Vector3Int.down)
                    .MoveBy(Vector3Int.down));
            var topBottom = foundation.Split(Vector3Int.down, 2);
            var top = topBottom.ElementAt(0);
            if(topBottom.Count() == 2)
            {
                var bottom = topBottom.ElementAt(1);
                var bottomHole = bottom.OpSub()
                    .ExtrudeDir(bridgeDirection, -1)
                    .ExtrudeDir(-bridgeDirection, -1)
                    .OpNew();
                return new LevelGroupElement(QueriedGrid, AreaStyles.None(),
                    top.LE(AreaStyles.BridgeTopFoundation(bridgeDirection)),
                    bottom.Minus(bottomHole).LE(AreaStyles.BridgeBottomFoundation(bridgeDirection))
                    );
            }
            else
            {
                return top.LE(AreaStyles.Foundation());
            }
        }


        public LevelGroupElement WallAround(LevelElement inside, int height)
        {
            var insideCg = inside.CG();
            var wallTop = insideCg.ExtrudeHor(true, false).Minus(insideCg).MoveBy(height * Vector3Int.up).LE(AreaStyles.WallTop());
            var wall = Foundation(wallTop);
            return new LevelGroupElement(inside.Grid, AreaStyles.None(), wall, wallTop);
        }

        public LevelElement RecursivelySplit(LevelGeometryElement levelElement, int maxSide)
        {
            var cubeGroup = levelElement.CG();
            var maxLengthDir = ExtensionMethods.PositiveDirections().ArgMax(dir => cubeGroup.ExtentsDir(dir));
            var maxLength = cubeGroup.ExtentsDir(maxLengthDir);
            if (maxLength <= maxSide)
            {
                return levelElement;
            }
            else
            {
                var relDist = MyRandom.Range(0.3f, 0.7f);
                var splitGroup = levelElement.SplitRel(maxLengthDir, levelElement.AreaStyle, relDist);
                return splitGroup.Select(lg => RecursivelySplit((LevelGeometryElement)lg, maxSide));
            }
        }

        public LevelGroupElement Partition(Func<CubeGroup, CubeGroup, CubeGroup> groupGrower, CubeGroup boundingGroup, int groupN)
        {
            var groups = boundingGroup.Cubes
                .Select(cube => cube.Position)
                .Shuffle()
                .Take(groupN)
                .Select(pos => new CubeGroup(QueriedGrid, QueriedGrid[pos].Group().Cubes))
                .ToList();
            var totalSize = 0;
            boundingGroup = boundingGroup.Minus(new CubeGroup(QueriedGrid, groups.SelectManyNN(grp => grp.Cubes).ToList()));
            // Iterate until no group can be grown
            while (groups.Select(grp => grp.Cubes.Count).Sum() > totalSize)
            {
                totalSize = groups.Select(grp => grp.Cubes.Count).Sum();
                var newGroups = new List<CubeGroup>();
                // Update each group and remove newly added cube from bounding box
                foreach (var grp in groups)
                {
                    var newGrp = groupGrower(grp, boundingGroup);
                    boundingGroup = boundingGroup.Minus(newGrp);
                    newGroups.Add(newGrp);
                }
                groups = newGroups;
            }
            return new LevelGroupElement(QueriedGrid, AreaStyles.None(), groups.Select(g => g.LE()).ToList<LevelElement>());
        }
    }
}

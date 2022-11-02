using OurFramework.Environment.GridMembers;
using OurFramework.Environment.StylingAreas;
using System.Linq;
using UnityEngine;

namespace OurFramework.Environment.ShapeCreation
{
    public class ShapeGrammarShapes
    {
        Grid<Cube> Grid { get; }
        QueryContext qc { get; }

        public ShapeGrammarShapes(Grid<Cube> grid)
        {
            Grid = grid;
            qc = new QueryContext(grid);
        }

        public LevelElement Room(Box3Int roomArea) => qc.GetBox(roomArea).LE(AreaStyles.Room());

        public LevelGroupElement SimpleHouseWithFoundation(CubeGroup belowFirstFloor, int floorHeight)
        {
            var room = belowFirstFloor.ExtrudeVer(Vector3Int.up, floorHeight, false)
                .LE(AreaStyles.Room());
            var roof = room.CG().ExtrudeVer(Vector3Int.up, 1, false).LE(AreaStyles.FlatRoof());
            var foundation = Foundation(room);
            var house = new LevelGroupElement(Grid, AreaStyles.Room(), foundation, room, roof);
            return house;
        }

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
            var roof = house.CubeGroupMaxLayer(Vector3Int.up).LE(AreaStyles.FlatRoof());
            var room = house.Minus(roof.CG()).LE(AreaStyles.Room());
            var foundation = Foundation(room);
            return new LevelGroupElement(Grid, AreaStyles.Room(), foundation, room, roof);
        }

        public LevelElement Foundation(LevelElement toBeFounded) => 
            qc.MoveDownUntilGround(
                toBeFounded.CG()
                    .CubeGroupLayer(Vector3Int.down)
                    .MoveBy(Vector3Int.down))
            .LE(AreaStyles.Foundation());

        public LevelElement BridgeFoundation(LevelElement bridgeToBeFounded, Vector3Int bridgeDirection)
        {
            var foundation = qc.MoveDownUntilGround(
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
                return new LevelGroupElement(Grid, AreaStyles.None(),
                    top.LE(AreaStyles.BridgeTopFoundation(bridgeDirection)),
                    bottom.Minus(bottomHole).LE(AreaStyles.BridgeBottomFoundation(bridgeDirection))
                    );
            }
            else
            {
                return top.LE(AreaStyles.Foundation());
            }
        }

        public CubeGroup IslandExtrudeIter(CubeGroup cubeGroup, int iterCount, float extrudeKeptRatio)
        {
            return ExtensionMethods.ApplyNTimes(cg => qc.ExtrudeRandomly(cg, extrudeKeptRatio), cubeGroup, iterCount);
        }

        public LevelGroupElement WallAround(LevelElement inside, int height)
        {
            var insideCg = inside.CG();
            var wallTop = insideCg.ExtrudeHor(true, false).Minus(insideCg).MoveBy(height * Vector3Int.up).LE(AreaStyles.WallTop());
            var wall = Foundation(wallTop);
            return new LevelGroupElement(inside.Grid, AreaStyles.None(), wall, wallTop);
        }
    }
}

using Assets.ShapeGrammarGenerator;
using System.Linq;
using UnityEngine;

namespace ShapeGrammar
{

    public class ShapeGrammarShapes
    {
        Grid<Cube> Grid { get; }
        QueryContext qc { get; }
        Placement pl { get; }

        public ShapeGrammarShapes(Grid<Cube> grid)
        {
            Grid = grid;
            qc = new QueryContext(grid);
            pl = new Placement(grid);
        }

        public LevelElement Room(Box3Int roomArea) => qc.GetBox(roomArea).LE(AreaStyles.Room());

        public CubeGroup FlatRoof(Box2Int areaXZ, int posY) => qc.GetPlatform(areaXZ, posY);

        public LevelGroupElement SimpleHouseWithFoundation(Box2Int areaXZ, int posY)
        {
            var room = Room(areaXZ.InflateY(posY, posY + 2));
            var roof = FlatRoof(areaXZ, posY + 2).LE(AreaStyles.FlatRoof());
            var foundation = Foundation(room);
            var house = new LevelGroupElement(Grid, AreaStyles.Room(), foundation, room, roof);
            return house;
        }

        public LevelGroupElement SimpleHouseWithFoundation(CubeGroup belowFirstFloor, int floorHeight)
        {
            var room = belowFirstFloor.ExtrudeVer(Vector3Int.up, floorHeight, false)
                .LE(AreaStyles.Room());
                //.SplitRel(Vector3Int.up, AreaType.Room, 0.3f, 0.7f);
            var roof = room.CG().ExtrudeVer(Vector3Int.up, 1, false).LE(AreaStyles.FlatRoof());
            var foundation = Foundation(room);
            var house = new LevelGroupElement(Grid, AreaStyles.Room(), foundation, room, roof);
            return house;
        }

        public LevelGroupElement CompositeHouse(int height)
        {
            var flatBoxes = qc.FlatBoxes(2, 5, 4);
            var layout = qc.RemoveOverlap(pl.MoveToIntersectAll(flatBoxes));

            var boxes =  qc.RaiseRandomly(layout, () => MyRandom.Range(height / 2, height))
                    .Select(part => TurnIntoHouse(part.CG()))
                    .SetChildrenAreaType(AreaStyles.Room());
            var symBoxes = boxes.SymmetrizeGrp(boxes.CG().CubeGroupMaxLayer(Vector3Int.left).Cubes.GetRandom().FacesHor(Vector3Int.left));

            return boxes.Merge(symBoxes);
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

        public LevelGroupElement AddBalcony(LevelGroupElement house)
        {
            // add outside part of each floor
            var withBalcony = house.ReplaceLeafsGrp(
                le => le.AreaStyle == AreaStyles.Room(),
                le => new LevelGroupElement(le.Grid, AreaStyles.None(), le, le.CG().CubeGroupMaxLayer(Vector3Int.down).ExtrudeHor(true, true).LE(AreaStyles.FlatRoof()))
            );
            return withBalcony;
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

        public CubeGroup Platform(Box2Int areaXZ, int posY) => qc.GetPlatform(areaXZ, posY);

        public CubeGroup BalconyOne(CubeGroup house)
        {
            // Find a cube for the balcony
            var balcony = house.BottomLayer()
               .AllBoundaryFacesH()
               .Where(face => !face.OtherCube.Changed && !face.OtherCube.In(house))
               .Facets.GetRandom()
               .OtherCube.Group();
           
            return balcony;
        }

        public CubeGroup BalconyWide(CubeGroup house)
        {
            // Find a cube for the balcony
            var balcony = house.BottomLayer()
               .BoundaryFacesH(Vector3Int.left)
               .Where(face => !face.OtherCube.Changed && !face.OtherCube.In(house))
               .Extrude(1);
;
            return balcony;
        }

        public CubeGroup IslandIrregular(Box2Int boundingArea)
        {
            int cubesCount = boundingArea.Volume() / 2;
            var boundingBox = boundingArea.InflateY(0, 1);
            var cubeGroup = new CubeGroup(Grid, boundingBox.Select(coords => Grid[coords]).ToList());
            return qc.GetRandomHorConnected(boundingBox.Center(), cubeGroup, cubesCount);
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

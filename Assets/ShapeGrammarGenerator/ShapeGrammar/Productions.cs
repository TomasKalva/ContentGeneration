using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ShapeGrammar.Transformations;

namespace ShapeGrammar
{
    public class Productions
    {
        public LevelDevelopmentKit ldk { get; }
        public Symbols sym { get; } = new Symbols();

        public Productions(LevelDevelopmentKit ldk)
        {
            this.ldk = ldk;
        }

        public Production CreateNewHouse()
        {
            return new Production(
                "CreateNewHouse",
                new ProdParamsManager(),
                (state, pp) =>
                {
                    var root = state.Root;
                    var room = ldk.sgShapes.Room(new Box2Int(0, 0, 5, 5).InflateY(8, 10));
                    var movedRoom = ldk.pl.MoveToNotOverlap(state.WorldState.Added, room).GrammarNode(sym.Room());
                    var foundation = ldk.sgShapes.Foundation(movedRoom.LE).GrammarNode(sym.Foundation);
                    return new[]
                    {
                        state.Add(root).SetTo(movedRoom),
                        state.Add(movedRoom).SetTo(foundation)
                    };
                });
        }

        public Production ExtrudeTerrace()
        {
            return new Production(
                "ExtrudeTerrace",
                new ProdParamsManager().AddNodeSymbols(sym.Room()),
                (state, pp) =>
                {
                    var room = pp.Param;
                    //var room = state.WithActiveSymbols(sym.Room);
                    if (room.LE.CG().LengthY() <= 1)
                        return null;

                    var terraces =
                    // for give parametrization
                        ExtensionMethods.HorizontalDirections().Shuffle()
                        .Select(dir =>

                        // access the object
                        room.LE.CG()

                        // create a new object
                        .ExtrudeDir(dir, 2).LE(AreaType.Colonnade))

                        // fail if no such object exists
                        .Where(le => le.CG().AreAllNotTaken());
                    if (!terraces.Any())
                        return null;

                    // and modify the dag
                    var terraceSpace = terraces.FirstOrDefault();
                    var lge = terraceSpace.Split(Vector3Int.down, AreaType.None, 1);
                    var terrace = lge.LevelElements[1].SetAreaType(AreaType.Colonnade).GrammarNode(sym.Terrace);
                    var roof = lge.LevelElements[0].SetAreaType(AreaType.Roof).GrammarNode(sym.Roof);
                    return new[]
                    {
                        state.Add(room).SetTo(terrace),
                        state.Add(terrace).SetTo(roof),
                    };
                });
        }

        public Production CourtyardFromRoom()
        {
            return new Production(
                "CourtyardFromRoom",
                new ProdParamsManager().AddNodeSymbols(sym.Room()),
                (state, pp) =>
                {
                    var room = pp.Param;
                    var roomCubeGroup = room.LE.CG();

                    var courtyards =
                    // for give parametrization
                        ExtensionMethods.HorizontalDirections().Shuffle()
                        .Select(dir =>

                        // create a new object
                        roomCubeGroup
                        .CubeGroupMaxLayer(dir)
                        .OpAdd()
                        .ExtrudeHor().ExtrudeHor().Minus(roomCubeGroup)

                        .LE(AreaType.Yard))

                        // fail if no such object exists
                        .Where(le => le.CG().AreAllNotTaken() && state.CanBeFounded(le));
                    if (!courtyards.Any())
                        return null;

                    var courtyard = courtyards.FirstOrDefault().GrammarNode(sym.Courtyard);

                    courtyard.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);
                    var door = ldk.con.ConnectByDoor(room.LE, courtyard.LE).GrammarNode();

                    // and modify the dag
                    var foundation = ldk.sgShapes.Foundation(courtyard.LE).GrammarNode(sym.Foundation);
                    return new[]
                    {
                        state.Add(room).SetTo(courtyard),
                        state.Add(courtyard).SetTo(foundation),
                        state.Add(room, courtyard).SetTo(door),
                    };
                });
        }

        public Production CourtyardFromCourtyardCorner()
        {
            return new Production(
                "CourtyardFromCourtyardCorner",
                new ProdParamsManager().AddNodeSymbols(sym.Courtyard),
                (state, pp) =>
                {
                    var courtyard = pp.Param;
                    var courtyardGroup = courtyard.LE.CG();
                    var corners = courtyardGroup.AllSpecialCorners().CG().CubeGroupMaxLayer(Vector3Int.down).Where(cube => cube.Position.y >= 4);
                    if (!corners.Cubes.Any())
                        return null;

                    var newCourtyards = corners.Cubes
                        .Select(startCube =>
                        startCube.Group()
                        .OpAdd()
                            .ExtrudeHor().Minus(courtyardGroup)
                            .ExtrudeHor().Minus(courtyardGroup)
                            .ExtrudeVer(Vector3Int.up, 2)
                        .OpNew()
                            .MoveBy(2 * Vector3Int.down)
                        .LE(AreaType.Yard))
                        .Where(le => le.CG().AreAllNotTaken() && state.CanBeFounded(le)); ;
                    if (!newCourtyards.Any())
                        return null;

                    var newCourtyardLe = newCourtyards.FirstOrDefault();
                    // floor doesn't exist yet...
                    newCourtyardLe.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    // connecting by elevator always succeeds
                    var path = ldk.con.ConnectByStairsInside(courtyard.LE, newCourtyardLe);

                    // and modify the dag
                    var newCourtyardNode = newCourtyardLe.GrammarNode(sym.Courtyard);
                    var foundation = ldk.sgShapes.Foundation(newCourtyardNode.LE).GrammarNode(sym.Foundation);
                    var pathNode = path.GrammarNode();
                    return new[]
                    {
                        state.Add(courtyard).SetTo(newCourtyardNode),
                        state.Add(newCourtyardNode).SetTo(foundation),
                        state.Add(courtyard, newCourtyardNode).SetTo(pathNode),
                    };
                });
        }

        public Production BridgeFromCourtyard()
        {
            return new Production(
                "BridgeFromCourtyard",
                new ProdParamsManager().AddNodeSymbols(sym.Courtyard),
                (state, pp) =>
                {
                    var courtyard = pp.Param;
                    //var courtyard = state.WithActiveSymbols(sym.Courtyard);
                    var courtyardCubeGroup = courtyard.LE.CG();

                    var bridges =
                    // for give parametrization
                        ExtensionMethods.HorizontalDirections().Shuffle()
                        .Select(dir =>

                        // create a new object
                        courtyardCubeGroup
                        .ExtrudeDir(dir, 3)
                        .OpSub()
                            .ExtrudeDir(dir.OrthogonalHorizontalDirs().First(), -1)
                            .ExtrudeDir(dir.OrthogonalHorizontalDirs().Last(), -1)
                        .OpNew()

                        .LE(AreaType.Bridge).GrammarNode(sym.Bridge(dir)))

                        // fail if no such object exists
                        .Where(gn => gn.LE.CG().AreAllNotTaken() && state.CanBeFounded(gn.LE));
                    if (!bridges.Any())
                        return null;

                    var bridge = bridges.FirstOrDefault();

                    bridge.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    // and modify the dag
                    var foundation = ldk.sgShapes.Foundation(bridge.LE).GrammarNode(sym.Foundation);
                    return new[]
                    {
                        state.Add(courtyard).SetTo(bridge),
                        state.Add(bridge).SetTo(foundation),
                    };
                });
        }

        public Production ExtendBridge()
        {
            return new Production(
                "ExtendBridge",
                new ProdParamsManager().AddNodeSymbols(sym.Bridge()),
                (state, pp) =>
                {
                    var bridge = pp.Param;
                    var courtyardCubeGroup = bridge.LE.CG();

                    var dir = bridge.GetSymbol<Bridge>().Direction;
                    var maybeNewBridge =
                        // create a new object
                        courtyardCubeGroup
                        .ExtrudeDir(dir, 4)
                        .LE(AreaType.Bridge).GrammarNode(sym.Bridge(dir)).ToEnumerable()

                        // fail if no such object exists
                        .Where(gn => gn.LE.CG().AreAllNotTaken() && state.CanBeFounded(gn.LE));
                    if (!maybeNewBridge.Any())
                        return null;

                    var newBbridge = maybeNewBridge.FirstOrDefault();

                    newBbridge.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    // and modify the dag
                    var foundation = ldk.sgShapes.Foundation(newBbridge.LE).GrammarNode(sym.Foundation);
                    return new[]
                    {
                        state.Add(bridge).SetTo(newBbridge),
                        state.Add(newBbridge).SetTo(foundation),
                    };
                });
        }

        public Production CourtyardFromBridge()
        {
            return new Production(
                "CourtyardFromBridge",
                new ProdParamsManager().AddNodeSymbols(sym.Bridge()),
                (state, pp) =>
                {
                    var bridge = pp.Param;
                    var courtyardCubeGroup = bridge.LE.CG();

                    var dir = bridge.GetSymbol<Bridge>().Direction;
                    var maybeNewCourtyard =
                        // create a new object
                        courtyardCubeGroup
                        .ExtrudeDir(dir, 4)
                        .OpAdd()
                            .ExtrudeDir(dir.OrthogonalHorizontalDirs().First(), 1)
                            .ExtrudeDir(dir.OrthogonalHorizontalDirs().Last(), 1)
                        .OpNew()
                        .LE(AreaType.Yard).GrammarNode(sym.Courtyard).ToEnumerable()

                        // fail if no such object exists
                        .Where(gn => gn.LE.CG().AreAllNotTaken() && state.CanBeFounded(gn.LE));
                    if (!maybeNewCourtyard.Any())
                        return null;

                    var newCourtyard = maybeNewCourtyard.FirstOrDefault();

                    newCourtyard.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    // and modify the dag
                    var foundation = ldk.sgShapes.Foundation(newCourtyard.LE).GrammarNode(sym.Foundation);
                    return new[]
                    {
                        state.Add(bridge).SetTo(newCourtyard),
                        state.Add(newCourtyard).SetTo(foundation),
                    };
                });
        }

        public Production HouseFromCourtyard()
        {
            return new Production(
                "HouseFromCourtyard",
                new ProdParamsManager().AddNodeSymbols(sym.Courtyard),
                (state, pp) =>
                {
                    var courtyard = pp.Param;
                    var courtyardCubeGroup = courtyard.LE.CG();

                    var newHouses =
                        ExtensionMethods.HorizontalDirections().Shuffle()
                        .Select(dir =>
                        // create a new object
                        courtyardCubeGroup
                        .ExtrudeDir(dir, 4)
                        .LE(AreaType.Room).GrammarNode(sym.Room()))

                        // fail if no such object exists
                        .Where(gn => gn.LE.CG().AreAllNotTaken() && state.CanBeFounded(gn.LE));
                    if (!newHouses.Any())
                        return null;

                    var newHouse = newHouses.FirstOrDefault();

                    newHouse.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);
                    var door = ldk.con.ConnectByDoor(newHouse.LE, courtyard.LE).GrammarNode();

                    // and modify the dag
                    var foundation = ldk.sgShapes.Foundation(newHouse.LE).GrammarNode(sym.Foundation);
                    return new[]
                    {
                        state.Add(courtyard).SetTo(newHouse),
                        state.Add(newHouse).SetTo(foundation),
                        state.Add(newHouse, courtyard).SetTo(door),
                    };
                });
        }

        public Production RoomNextToCourtyard(Func<LevelElement> buildingF)
        {
            return new Production(
                "BuildingNextToCourtyard",
                new ProdParamsManager().AddNodeSymbols(sym.Courtyard),
                (state, pp) =>
                {
                    var courtyard = pp.Param;
                    var courtyardCG = courtyard.LE.CG();

                    var newBuilding = buildingF();
                    var newBuildingAtGround = newBuilding.MoveBottomTo(0);
                    var validNewBuilding = ldk.pl.MoveNearXZ(courtyard.LE.MoveBottomTo(0), newBuildingAtGround, state.VerticallyTaken);
                    if (validNewBuilding == null)
                        return null;
                    

                    var newBuildingGN = validNewBuilding.MoveBottomTo(courtyardCG.LeftBottomBack().y).GrammarNode(sym.Room());
                    
                    newBuildingGN.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);
                    /*
                    var door = ldk.con.ConnectByDoor(newBuildingGN.LE, courtyard.LE).GrammarNode();

                    // and modify the dag
                    var foundation = ldk.sgShapes.Foundation(newBuildingGN.LE).GrammarNode(sym.Foundation);*/
                    return new[]
                    {
                        state.Add(courtyard).SetTo(newBuildingGN),/*
                        state.Add(newBuildingGN).SetTo(foundation),
                        state.Add(newBuildingGN, courtyard).SetTo(door),
                        */
                    };
                });
        }

        public Production ExtendHouse(FloorConnector floorConnector)
        {
            return new Production(
                "ExtendHouse",
                new ProdParamsManager()
                    .AddNodeSymbols(sym.Room()),
                (state, pp) =>
                {
                    var room = pp.Param;
                    var roomCubeGroup = room.LE.CG();

                    var newHouses =
                        ExtensionMethods.HorizontalDirections().Shuffle()
                        .Select(dir =>
                        // create a new object
                        (dir, roomCubeGroup
                        .ExtrudeDir(dir, 6)
                        .LE(AreaType.Room)
                        .GrammarNode(sym.Room(false))))
                        // fail if no such object exists
                        .Where(dirNode => dirNode.Item2.LE.CG().AreAllNotTaken() && state.CanBeFounded(dirNode.Item2.LE));

                    if (!newHouses.Any())
                        return null;

                    var parametrizedHouse = newHouses.FirstOrDefault();
                    var newHouse = parametrizedHouse.Item2;

                    newHouse.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);
                    var dir = parametrizedHouse.Item1;

                    (LevelGeometryElement, LevelGeometryElement, LevelGeometryElement) startMiddleEnd()
                    {
                        var startMiddleEnd = newHouse.LE
                            .Split(dir, AreaType.None, 1)
                            .ReplaceLeafsGrp(1,
                                large => large.Split(-dir, AreaType.None, 1))
                            .ReplaceLeafsGrp(2,
                                middle => middle.SetAreaType(AreaType.Colonnade));
                        var les = startMiddleEnd.Leafs().ToList();
                        return (les[0], les[2], les[1]);
                    }
                    var (start, middle, end) = startMiddleEnd();

                    var middleGn = middle.SetAreaType(AreaType.NoFloor).GrammarNode();
                    var path = floorConnector(start, middle, end).SetAreaType(AreaType.Platform);
                    //ldk.con.ConnectByStairsInside(start, end, newHouse.LevelElement);

                    var door = ldk.con.ConnectByDoor(room.LE, start).GrammarNode();

                    // and modify the dag
                    var foundation = ldk.sgShapes.Foundation(newHouse.LE).GrammarNode();
                    var startFoundation = ldk.sgShapes.Foundation(start).GrammarNode();
                    var endFoundation = ldk.sgShapes.Foundation(end).GrammarNode();
                    return new[]
                    {
                        state.Add(room).SetTo(newHouse),

                        state.Add(newHouse).SetTo(foundation),
                        state.Add(foundation).SetTo(startFoundation),
                        state.Add(foundation).SetTo(endFoundation),


                        state.Add(newHouse, room).SetTo(door),
                        state.Add(newHouse).SetTo(start.GrammarNode(), middleGn, end.GrammarNode()),
                        state.Replace(middleGn).SetTo(path.GrammarNode())
                    };
                });
        }

        public Production AddNextFloor()
        {
            return new Production(
                "AddNextFloor",
                new ProdParamsManager()
                    .AddNodeSymbols(sym.Room())
                    .SetCondition((state, pp) => pp.Param.GetSymbol<Room>().Plain && pp.Param.GetSymbol<Room>().Floor <= 1),
                (state, pp) =>
                {
                    var room = pp.Param;
                    var roomCubeGroup = room.LE.CG();
                    var roomSymbol = room.GetSymbol<Room>();

                    var maybeNextFloor =
                        roomCubeGroup.ExtrudeDir(Vector3Int.up, 2).LE(AreaType.Room).GrammarNode(sym.Room(true, roomSymbol.Floor + 1)).ToEnumerable()
                        .Where(floor => floor.LE.CG().AreAllNotTaken());

                    if (!maybeNextFloor.Any())
                        return null;

                    var nextFloor = maybeNextFloor.FirstOrDefault();
                    nextFloor.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    var stairs = ldk.con.ConnectByWallStairsIn(room.LE, nextFloor.LE).GrammarNode();

                    // and modify the dag
                    return new[]
                    {
                        state.Add(room).SetTo(nextFloor),
                        state.Add(room, nextFloor).SetTo(stairs),
                    };
                });
        }

        public Production GardenFromCourtyard()
        {
            return new Production(
                "GardenFromCourtyard",
                new ProdParamsManager()
                    .AddNodeSymbols(sym.Courtyard),
                (state, pp) =>
                {
                    var courtyard = pp.Param;
                    var courtyardCubeGroup = courtyard.LE.CG();

                    var possibleStartCubes = courtyardCubeGroup.CubeGroupMaxLayer(Vector3Int.down).ExtrudeHor().MoveBy(3 * Vector3Int.down).NotTaken();
                    if (!possibleStartCubes.Cubes.Any())
                        return null;

                    var gardens =
                        ExtensionMethods.HorizontalDirections().Shuffle()
                        .Select(dir =>
                            ldk.sgShapes.IslandExtrudeIter(possibleStartCubes.CubeGroupMaxLayer(dir), 3, 0.7f)
                            .LE(AreaType.Garden).Minus(state.WorldState.Added)
                            .MapGeom(cg => cg
                                .SplitToConnected().ArgMax(cg => cg.Cubes.Count)
                                .OpAdd().ExtrudeVer(Vector3Int.up, 3))
                            .GrammarNode()
                        )
                        //todo: remove cubes that can't be founded directly instead of asking about the whole group at the end
                        .Where(garden => garden.LE.Cubes().Count() >= 8 && state.CanBeFounded(garden.LE));

                    if (!gardens.Any())
                        return null;

                    var garden = gardens.FirstOrDefault();

                    garden.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    var cliffFoundation = ldk.sgShapes.CliffFoundation(garden.LE).GrammarNode();
                    var stairs = ldk.con.ConnectByElevator(courtyard.LE, garden.LE).GrammarNode();

                    return new[]
                    {
                        state.Add(courtyard).SetTo(garden),
                        state.Add(garden).SetTo(cliffFoundation),
                        state.Add(garden, courtyard).SetTo(stairs),
                    };
                });
        }

        /*
        public Production ExtrudeRoof()
        {
            return new Production(
                state => state.WithActiveSymbols(sym.Terrace) != null,
                state =>
                {
                    var terrace = state.WithActiveSymbols(sym.Terrace);
                    if (terrace.Derived.Where(derNode => derNode.HasActiveSymbols(sym.Roof)).Any())
                        return null;

                    var roof = terrace.LevelElement.CubeGroup().ExtrudeDir(Vector3Int.up).LevelElement(AreaType.Roof);
                    return new[]
                    {
                        state.Add(terrace).SetTo(roof.GrammarNode(sym.Roof)),
                    };
                }
                );
        }*/

    }
}

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
                    var reservation = movedRoom.LE.CG().ExtrudeVer(Vector3Int.up, 2).LE(AreaType.RoomReservation).GrammarNode(sym.RoomReservation(movedRoom));
                    return new[]
                    {
                        state.Add(root).SetTo(movedRoom),
                        state.Add(movedRoom).SetTo(foundation),
                        state.Add(movedRoom).SetTo(reservation)
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
                        .Where(le => le.CG().AllAreNotTaken());
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
                        .OpNew()

                        .LE(AreaType.Yard))

                        // fail if no such object exists
                        .Where(le => le.CG().AllAreNotTaken() && state.CanBeFounded(le));
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
                        .Where(le => le.CG().AllAreNotTaken() && state.CanBeFounded(le)); ;
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
                        {
                            var orthDir = dir.OrthogonalHorizontalDirs().First();
                            var width = courtyardCubeGroup.ExtentsDir(orthDir);
                            var bridgeWidth = 3;
                            var shrinkL = (int)Mathf.Floor((width - bridgeWidth) / 2f);
                            var shrinkR = (int)Mathf.Ceil((width - bridgeWidth) / 2f);
                            // create a new object
                            return courtyardCubeGroup
                                .ExtrudeDir(dir, 4)
                                .OpSub()
                                    .ExtrudeDir(orthDir, -shrinkL)
                                    .ExtrudeDir(-orthDir, -shrinkR)
                                .OpNew()

                                .LE(AreaType.Bridge).GrammarNode(sym.Bridge(dir));
                        })
                        // fail if no such object exists
                        .Where(gn => gn.LE.Cubes().Any())
                        .Where(gn => gn.LE.CG().AllAreNotTaken() && state.CanBeFounded(gn.LE));
                    if (!bridges.Any())
                        return null;

                    var bridge = bridges.FirstOrDefault();

                    bridge.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    var brDir = bridge.GetSymbol<Bridge>().Direction;

                    // and modify the dag
                    var foundation = ldk.sgShapes.BridgeFoundation(bridge.LE, brDir).GrammarNode(sym.Foundation);
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
                        .Where(gn => gn.LE.CG().AllAreNotTaken() && state.CanBeFounded(gn.LE));
                    if (!maybeNewBridge.Any())
                        return null;

                    var newBridge = maybeNewBridge.FirstOrDefault();

                    newBridge.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    // and modify the dag
                    var foundation = ldk.sgShapes.BridgeFoundation(newBridge.LE, dir).GrammarNode(sym.Foundation);
                    return new[]
                    {
                        state.Add(bridge).SetTo(newBridge),
                        state.Add(newBridge).SetTo(foundation),
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
                        .Where(gn => gn.LE.CG().AllAreNotTaken() && state.CanBeFounded(gn.LE));
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

        /*
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
        }*/

        public Production RoomNextTo(Symbol nextToWhat, Func<LevelElement> roomF)
        {
            return new Production(
                $"RoomNextTo{nextToWhat.Name}",
                new ProdParamsManager().AddNodeSymbols(nextToWhat),
                (state, pp) =>
                {
                    var what = pp.Param;
                    var whatCG = what.LE.CG();

                    var newRoom = roomF();
                    //
                    var newRoomAtGround = newRoom.MoveBottomTo(0);
                    var validNewRoom = ldk.pl.MoveNearXZ(what.LE.MoveBottomTo(0), newRoomAtGround, state.VerticallyTaken);
                    if (validNewRoom == null)
                        return null;
                    

                    var newRoomGN = validNewRoom.MoveBottomTo(whatCG.LeftBottomBack().y).GrammarNode(sym.Room());
                    //
                    
                    newRoomGN.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);
                    
                    var door = ldk.con.ConnectByDoor(newRoomGN.LE, what.LE).GrammarNode();
                    Debug.Assert(door != null);

                    // and modify the dag
                    var foundation = ldk.sgShapes.Foundation(newRoomGN.LE).GrammarNode(sym.Foundation);
                    var reservation = newRoomGN.LE.CG().ExtrudeVer(Vector3Int.up, 2).LE(AreaType.RoomReservation).GrammarNode(sym.RoomReservation(newRoomGN));
                    return new[]
                    {
                        state.Add(what).SetTo(newRoomGN),
                        state.Add(newRoomGN).SetTo(foundation),
                        state.Add(newRoomGN).SetTo(reservation),
                        state.Add(newRoomGN, what).SetTo(door),
                        
                    };

                    /*
                    Maybe clearer syntax that requires more helper methods

                    Linq:

                    from obj in createObject(roomF(), sym.Room())
                    from movedObj in moveNear(what)
                    from _ in placeObjToWorld(movedObj)
                    from door in connect(ldk.con.ConnectByDoor(obj, what))
                    from foundation in foundation(obj)
                    from _2 in placeObjToWorld(door)
                    from _3 in placeObjToWorld(foundation)
                    select null

                    Ideal syntax:

                    obj <- createObject(roomF(), sym.Room())
                    movedObj <- moveNear(what)
                    placeObjToWorld(movedObj)
                    door <- connect(ldk.con.ConnectByDoor(obj, what))
                    foundation <- foundation(obj)
                    placeObjToWorld(door)
                    placeObjToWorld(foundation)
                    */
                });
        }

        public Production ExtendBridgeTo(Symbol from, Func<LevelElement> toF, PathGuide pathGuide = null, bool addFloorAbove = true)
        {
            pathGuide ??= new RandomPathGuide();
            return new Production(
                $"ExtendBridgeTo",
                new ProdParamsManager().AddNodeSymbols(from),
                (state, pp) =>
                {
                    var what = pp.Param;
                    var whatCG = what.LE.CG();

                    var newRoom = toF();
                    var newRoomAtGround = newRoom.MoveBottomTo(0);

                    var newRooms = pathGuide.SelectDirections(what.LE)
                        .SelectNN(dir =>
                        {
                            var boundingBox = whatCG.CubeGroupMaxLayer(Vector3Int.down).ExtrudeDir(dir, 10, false).LE();
                            if (boundingBox.CG().Intersects(state.WorldState.Added.CG()))
                                return null;

                            return boundingBox.MoveBottomTo(0);
                        })
                        .SelectNN(boundingBox =>
                        {
                            var validMoves = newRoomAtGround
                                .MovesToIntersect(boundingBox).XZ()
                                .DontIntersect(state.VerticallyTaken);
                            var moved = pathGuide.SelectMove(validMoves).TryMove();
                            return moved;
                        })
                        .Select(newRoomDown => newRoomDown.MoveBottomTo(whatCG.LeftBottomBack().y).GrammarNode(sym.Room()));

                    if (!newRooms.Any())
                        return null;

                    var newRoomGN = newRooms.First();

                    newRoomGN.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    var bridge = ldk.con.ConnectByBridge(what.LE, newRoomGN.LE, state.WorldState.Added).GrammarNode();
                    Debug.Assert(bridge != null);

                    // and modify the dag
                    var foundation = ldk.sgShapes.Foundation(newRoomGN.LE).GrammarNode(sym.Foundation);
                    if (addFloorAbove)
                    {
                        var reservation = newRoomGN.LE.CG().ExtrudeVer(Vector3Int.up, 2).LE(AreaType.RoomReservation).GrammarNode(sym.RoomReservation(newRoomGN));
                        return new[]
                        {
                        state.Add(what).SetTo(newRoomGN),
                        state.Add(newRoomGN).SetTo(foundation),
                        state.Add(newRoomGN).SetTo(reservation),
                        state.Add(what, newRoomGN).SetTo(bridge),
                        };
                    }
                    else
                    {
                        return new[]
                        {
                        state.Add(what).SetTo(newRoomGN),
                        state.Add(newRoomGN).SetTo(foundation),
                        state.Add(what, newRoomGN).SetTo(bridge),
                        };
                    }
                });
        }

        public Production RoomFallDown(Symbol nextToWhat, Func<LevelElement> roomFromToF)
        {
            return new Production(
                $"RoomNextTo{nextToWhat.Name}",
                new ProdParamsManager().AddNodeSymbols(nextToWhat),
                (state, pp) =>
                {
                    var what = pp.Param;
                    var whatCG = what.LE.CG();

                    var newRoom = roomFromToF();
                    //
                    var newRoomAtGround = newRoom.MoveBottomTo(0);
                    var validNewRoom = ldk.pl.MoveNearXZ(what.LE.MoveBottomTo(0), newRoomAtGround, state.VerticallyTaken);
                    if (validNewRoom == null)
                        return null;


                    var newRoomGN = validNewRoom.MoveBottomTo(whatCG.LeftBottomBack().y).GrammarNode(sym.Room(false, 1));
                    //

                    newRoomGN.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    var door = ldk.con.ConnectByDoor(newRoomGN.LE, what.LE).GrammarNode();
                    Debug.Assert(door != null);

                    // and modify the dag

                    var bottomRoom = newRoomGN.LE.CG().ExtrudeVer(Vector3Int.down, 2).LE(AreaType.Room).GrammarNode(sym.Room(false, 0));
                    bottomRoom.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    var fall = ldk.con.ConnectByFall(newRoomGN.LE, bottomRoom.LE).GrammarNode();
                    var foundation = ldk.sgShapes.Foundation(bottomRoom.LE).GrammarNode(sym.Foundation);
                    var reservation = newRoomGN.LE.CG().ExtrudeVer(Vector3Int.up, 2).LE(AreaType.RoomReservation).GrammarNode(sym.RoomReservation(newRoomGN));
                    return new[]
                    {
                        state.Add(what).SetTo(newRoomGN),
                        state.Add(newRoomGN).SetTo(bottomRoom),
                        state.Add(bottomRoom).SetTo(foundation),
                        state.Add(bottomRoom).SetTo(reservation),
                        state.Add(newRoomGN, what).SetTo(door),
                        state.Add(newRoomGN, bottomRoom).SetTo(fall),

                    };
                });
        }

        /// <summary>
        /// to has to have height at least 2
        /// </summary>
        public Production TowerFallDown(Symbol from, Symbol to, Func<LevelElement> roomFromF)
        {
            return new Production(
                $"RoomNextTo{from.Name}",
                new ProdParamsManager()
                    .AddNodeSymbols(from, sym.Garden)
                    .AddNodeSymbols(to, sym.Room())
                    .SetCondition((state, pp) => 
                    {
                        var (from, to) = pp;
                        return true
                            //to.GetSymbol<Room>().Plain
                            //&& from.LE.CG().MinkowskiMinus(to.LE.CG()).Min(v => v.AbsSum()) < 5
                            ;
                    }),
                (state, pp) =>
                {
                    var (from, to) = pp;
                    var fromCG = from.LE.CG();
                    var toCG = to.LE.CG();

                    var newRoom = roomFromF();
                    //
                    var newRoomAtGround = newRoom.MoveBottomTo(0);
                    var validNewRoom = ldk.pl.MoveNearXZ(to.LE.MoveBottomTo(0), newRoomAtGround, state.VerticallyTaken);
                    if (validNewRoom == null)
                        return null;


                    var newRoomGN = validNewRoom.MoveBottomTo(toCG.LeftBottomBack().y + 1).GrammarNode(sym.Room(false, 1));
                    //

                    newRoomGN.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);
                    var foundation = ldk.sgShapes.Foundation(newRoomGN.LE).GrammarNode(sym.Foundation);
                    var reservation = newRoomGN.LE.CG().ExtrudeVer(Vector3Int.up, 2).LE(AreaType.RoomReservation).GrammarNode(sym.RoomReservation(newRoomGN));

                    // might fail in some rare cases => todo: add a teleporter connection in case it fails
                    var stairs = ldk.con.ConnectByBalconyStairsOutside(from.LE, newRoomGN.LE, state.WorldState.Added.Merge(foundation.LE).Merge(reservation.LE)).GrammarNode();
                    Debug.Assert(stairs != null);

                    // and modify the dag


                    var fall = ldk.con.ConnectByFall(newRoomGN.LE, to.LE).GrammarNode();
                    Debug.Assert(fall != null);

                    return new[]
                    {
                        state.Add(from).SetTo(newRoomGN),
                        state.Add(newRoomGN).SetTo(foundation),
                        state.Add(newRoomGN).SetTo(reservation),
                        state.Add(newRoomGN, to).SetTo(fall),
                        state.Add(newRoomGN, from).SetTo(stairs),
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
                        .Where(dirNode => dirNode.Item2.LE.CG().AllAreNotTaken() && state.CanBeFounded(dirNode.Item2.LE));

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

                    //todo: test if the reservation is ok
                    var reservation = newHouse.LE.CG().ExtrudeVer(Vector3Int.up, 2).LE(AreaType.RoomReservation).GrammarNode(sym.RoomReservation(newHouse));
                    // and modify the dag
                    var foundation = ldk.sgShapes.Foundation(newHouse.LE).GrammarNode(sym.Foundation);
                    var startFoundation = ldk.sgShapes.Foundation(start).GrammarNode(sym.Foundation);
                    var endFoundation = ldk.sgShapes.Foundation(end).GrammarNode(sym.Foundation);
                    return new[]
                    {
                        state.Add(room).SetTo(newHouse),

                        state.Add(newHouse).SetTo(foundation),
                        state.Add(foundation).SetTo(startFoundation),
                        state.Add(foundation).SetTo(endFoundation),

                        state.Add(newHouse).SetTo(reservation),

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
                    .AddNodeSymbols(sym.RoomReservation(null))
                    .SetCondition((state, pp) =>
                    {
                        var roomBelow = pp.Param.GetSymbol<RoomReservation>().RoomBelow.GetSymbol<Room>();
                        return roomBelow.Plain && roomBelow.Floor <= 1;
                    })
                    ,
                (state, pp) =>
                {
                    var roomReservation = pp.Param;
                    var roomBelow = roomReservation.GetSymbol<RoomReservation>().RoomBelow;
                    var nextFloor = roomReservation.LE.SetAreaType(AreaType.Room).GrammarNode(sym.Room(true, roomBelow.GetSymbol<Room>().Floor + 1));

                    var maybeNewReservation =
                        nextFloor.LE.CG().ExtrudeDir(Vector3Int.up, 2).LE(AreaType.RoomReservation).GrammarNode(sym.RoomReservation(nextFloor)).ToEnumerable()
                        .Where(floor => floor.LE.CG().AllAreNotTaken());

                    if (!maybeNewReservation.Any())
                        return null;
                    
                    var newReservation = maybeNewReservation.FirstOrDefault();
                    nextFloor.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    var stairs = ldk.con.ConnectByWallStairsIn(roomBelow.LE, nextFloor.LE).GrammarNode();

                    // and modify the dag
                    return new[]
                    {
                        state.Replace(roomReservation).SetTo(nextFloor),
                        state.Add(roomBelow, nextFloor).SetTo(stairs),
                        state.Add(nextFloor).SetTo(newReservation),
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
                            .GrammarNode(sym.Garden)
                        )
                        //todo: remove cubes that can't be founded directly instead of asking about the whole group at the end
                        .Where(garden => garden.LE.Cubes().Count() >= 8 && state.CanBeFounded(garden.LE));

                    if (!gardens.Any())
                        return null;

                    var garden = gardens.FirstOrDefault();

                    garden.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    var cliffFoundation = ldk.sgShapes.CliffFoundation(garden.LE).GrammarNode(sym.Foundation);
                    var stairs = ldk.con.ConnectByElevator(courtyard.LE, garden.LE).GrammarNode();

                    return new[]
                    {
                        state.Add(courtyard).SetTo(garden),
                        state.Add(garden).SetTo(cliffFoundation),
                        state.Add(garden, courtyard).SetTo(stairs),
                    };
                });
        }

        public Production AddRoof()
        {
            return new Production(
                "AddRoof",
                new ProdParamsManager()
                    .AddNodeSymbols(sym.RoomReservation(null)),
                (state, pp) =>
                {
                    var roomReservation = pp.Param;
                    var roof = roomReservation.LE.SetAreaType(AreaType.Roof).GrammarNode(sym.Roof);

                    // and modify the dag
                    return new[]
                    {
                        state.Replace(roomReservation).SetTo(roof),
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

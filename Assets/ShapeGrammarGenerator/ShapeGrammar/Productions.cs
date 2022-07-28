using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ShapeGrammar.Connections;
using static ShapeGrammar.Transformations;

namespace ShapeGrammar
{
    public class Productions
    {
        public LevelDevelopmentKit ldk { get; }
        public Symbols sym { get; }

        public Productions(LevelDevelopmentKit ldk, Symbols sym)
        {
            this.ldk = ldk;
            this.sym = sym;
        }

        public Production CreateNewHouse(int bottomHeight)
        {
            return new Production(
                "CreateNewHouse",
                new ProdParamsManager(),
                (state, pp) =>
                {
                    var root = state.Root;
                    var room = ldk.sgShapes.Room(new Box2Int(0, 0, 5, 5).InflateY(bottomHeight, bottomHeight + 2));
                    var movedRoom = ldk.pl.MoveToNotOverlap(state.WorldState.Added, room).GN(sym.Room(), sym.FullFloorMarker);
                    var foundation = ldk.sgShapes.Foundation(movedRoom.LE).GN(sym.Foundation);
                    var reservation = movedRoom.LE.CG().ExtrudeVer(Vector3Int.up, 2).LE(AreaType.Reservation).GN(sym.UpwardReservation(movedRoom));


                    return state.NewProgramBadMethodDestroyItASAP(new[]
                    {
                        state.Add(root).SetTo(movedRoom),
                        state.Add(movedRoom).SetTo(foundation),
                        state.Add(movedRoom).SetTo(reservation)
                    });
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

                    // Reduces number of characters (withou spaces) from ~800 to ~480, from 34 lines to 22
                    return state.NewProgram()
                        .SelectOne(
                            state.NewProgram()
                                .Directional(ExtensionMethods.HorizontalDirections().Shuffle(),
                                    dir =>
                                        roomCubeGroup
                                        .CubeGroupMaxLayer(dir)
                                        .OpAdd()
                                            .ExtrudeHor().ExtrudeHor().Minus(roomCubeGroup)
                                        .OpNew()

                                        .LE(AreaType.Yard)
                                        .GN(sym.Courtyard, sym.FullFloorMarker)
                                )
                                .NotTaken()
                                .CanBeFounded(),
                            out var courtyard
                        )
                        .PlaceCurrentFrom(room)

                        .Found()
                        .PlaceCurrentFrom(courtyard)

                        .FindPath(() => ldk.con.ConnectByDoor(room.LE, courtyard.LE).GN(sym.ConnectionMarker), out var door)
                        .PlaceCurrentFrom(room, courtyard);
                     
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

                    return state.NewProgram()
                        .SelectOne(
                            state.NewProgram()
                                .Set(
                                    () => courtyardGroup
                                        .AllSpecialCorners().CG()
                                        .CubeGroupMaxLayer(Vector3Int.down)
                                        .Where(cube => cube.Position.y >= 4)
                                        .Cubes.Select(cube => cube.Group().LE().GN())
                                )
                                .Change(startCube =>
                                    startCube.LE.CG()
                                    .OpAdd()
                                        .ExtrudeHor().Minus(courtyardGroup)
                                        .ExtrudeHor().Minus(courtyardGroup)
                                        .ExtrudeVer(Vector3Int.up, 2)
                                    .OpNew()
                                        .MoveBy(2 * Vector3Int.down)
                                    .LE(AreaType.Yard).GN(sym.Courtyard, sym.FullFloorMarker)
                                )
                                .NotTaken()
                                .CanBeFounded(),
                            out var newCourtyard
                        )
                        .PlaceCurrentFrom(courtyard)

                        .Found()
                        .PlaceCurrentFrom(newCourtyard)

                        .FindPath(() => ldk.con.ConnectByStairsInside(courtyard.LE, newCourtyard.LE).GN(sym.ConnectionMarker), out var p)
                        .PlaceCurrentFrom(courtyard, newCourtyard);
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
                    var courtyardCG = courtyard.LE.CG();
                    
                    return state.NewProgram()
                        .SelectOne(
                            state.NewProgram()
                                .Directional(ExtensionMethods.HorizontalDirections().Shuffle(),
                                    dir =>
                                    {
                                        var orthDir = dir.OrthogonalHorizontalDirs().First();
                                        var width = courtyardCG.ExtentsDir(orthDir);
                                        var bridgeWidth = 3;
                                        var shrinkL = (int)Mathf.Floor((width - bridgeWidth) / 2f);
                                        var shrinkR = (int)Mathf.Ceil((width - bridgeWidth) / 2f);

                                        return courtyardCG
                                            .ExtrudeDir(dir, 4)
                                            .OpSub()
                                                .ExtrudeDir(orthDir, -shrinkL)
                                                .ExtrudeDir(-orthDir, -shrinkR)
                                            .OpNew()

                                            .LE(AreaType.Bridge).GN(sym.Bridge(dir), sym.FullFloorMarker);
                                    }
                                )
                                .NonEmpty()
                                .NotTaken()
                                .CanBeFounded(),
                            out var bridge
                        )
                        .PlaceCurrentFrom(courtyard)

                        .EmptyPath()
                        .PlaceCurrentFrom(courtyard, bridge)

                        .Set(() => ldk.sgShapes.BridgeFoundation(
                            bridge.LE,
                            bridge.GetSymbol<Bridge>().Direction
                            ).GN(sym.Foundation))
                        .PlaceCurrentFrom(bridge);
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

                    return state.NewProgram()
                        .Set(() => courtyardCubeGroup
                            .ExtrudeDir(dir, 4)
                            .LE(AreaType.Bridge).GN(sym.Bridge(dir), sym.FullFloorMarker),
                            out var newBridge
                        )
                        .NotTaken()
                        .CanBeFounded()
                        .PlaceCurrentFrom(bridge)

                        .EmptyPath()
                        .PlaceCurrentFrom(bridge, newBridge)

                        .Set(() => ldk.sgShapes.BridgeFoundation(
                            newBridge.LE,
                            newBridge.GetSymbol<Bridge>().Direction
                            ).GN(sym.Foundation))
                        .PlaceCurrentFrom(newBridge);
                     
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
                    var bridgeCubeGroup = bridge.LE.CG();
                    var dir = bridge.GetSymbol<Bridge>().Direction;

                    return state.NewProgram()
                        .Set(
                            () => bridgeCubeGroup
                            .ExtrudeDir(dir, 4)
                            .OpAdd()
                                .ExtrudeDir(dir.OrthogonalHorizontalDirs().First(), 1)
                                .ExtrudeDir(dir.OrthogonalHorizontalDirs().Last(), 1)
                            .OpNew()
                            .LE(AreaType.Yard).GN(sym.Courtyard, sym.FullFloorMarker),
                            out var newCourtyard
                        )
                        .NotTaken()
                        .CanBeFounded()
                        .PlaceCurrentFrom(bridge)

                        .Found()
                        .PlaceCurrentFrom(newCourtyard)

                        .EmptyPath()
                        .PlaceCurrentFrom(bridge, newCourtyard);
                    
                });
        }

        public Production RoomNextTo(Symbol nextToWhat, Func<LevelElement> roomF)
        {

            return FullFloorNextTo(nextToWhat, sym.Room(), roomF,
                (program, _) => program,
                (program, newRoom) => program
                        .Set(() => newRoom)
                        .ReserveUpward(2)
                        .PlaceCurrentFrom(newRoom),
                _ => ldk.con.ConnectByDoor
                );
            /*
            return new Production(
                $"RoomNextTo{nextToWhat.Name}",
                new ProdParamsManager().AddNodeSymbols(nextToWhat),
                (state, pp) =>
                {
                    var what = pp.Param;
                    var whatCG = what.LE.CG();

                    return state.NewProgram()
                        .SelectOne(
                            state.NewProgram()
                                .Set(() => roomF().GN())
                                .MoveNearTo(what)
                                .Change(node => node.LE.GN(sym.Room(), sym.FullFloorMarker)),
                            out var newRoom
                            )
                        .PlaceCurrentFrom(what)

                        .Found()
                        .PlaceCurrentFrom(newRoom)

                        .Set(() => newRoom)
                        .ReserveUpward(2)
                        .PlaceCurrentFrom(newRoom)

                        .FindPath(() => ldk.con.ConnectByDoor(newRoom.LE, what.LE).GN(sym.ConnectionMarker), out var door)
                        .PlaceCurrentFrom(what, newRoom);
                });*/
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

                    var createdRoom = toF();
                    
                    // reduced from 1450 to 1050 characters, from 80 lines to 34 lines
                    return state.NewProgram()
                        .SelectOne(
                            state.NewProgram()
                                .Directional(pathGuide.SelectDirections(what.LE),
                                    dir => whatCG.CubeGroupMaxLayer(Vector3Int.down).ExtrudeDir(dir, 10, false).LE().GN()
                                )
                                .DontIntersectAdded()
                                .Change(boundingBox => boundingBox.LE.MoveBottomTo(0).GN())
                                .Change(boundingBox =>
                                    {
                                        var validMoves = createdRoom.MoveBottomTo(0)
                                            .MovesToIntersect(boundingBox.LE).XZ()
                                            .DontIntersect(state.VerticallyTaken);
                                        return pathGuide.SelectMove(validMoves).TryMove()?.GN();
                                    })
                                .Change(
                                    newRoomDown => newRoomDown.LE.MoveBottomTo(whatCG.LeftBottomBack().y).GN(sym.Room(), sym.FullFloorMarker)
                                ),
                                out var newRoom
                        )
                        .PlaceCurrentFrom(what)

                        .Found()
                        .PlaceCurrentFrom(newRoom)

                        .RunIf(addFloorAbove,
                            thisProg => thisProg
                                .Set(() => newRoom)
                                .ReserveUpward(2)
                                .PlaceCurrentFrom(newRoom)
                        )

                        .FindPath(() => ldk.con.ConnectByBridge(state.WorldState.Added)(what.LE, newRoom.LE).GN(sym.ConnectionMarker), out var bridge)
                        .PlaceCurrentFrom(what, newRoom);
                     
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

                    return state.NewProgram()
                        .SelectOne(
                            state.NewProgram()
                                .Set(() => roomFromToF().GN())
                                .MoveNearTo(what)
                                .Change(node => node.LE.GN(sym.Room(false, 1), sym.FullFloorMarker))
                                ,
                            out var newRoom
                        )
                        .PlaceCurrentFrom(what)
                        
                        .ReserveUpward(2)
                        .PlaceCurrentFrom(what)
                        
                        .Set(() =>
                            newRoom.LE.CG().ExtrudeVer(Vector3Int.down, 2).LE(AreaType.Room).GN(sym.Room(false, 0), sym.FullFloorMarker),
                            out var bottomRoom
                        )
                        .PlaceCurrentFrom(newRoom)
                        
                        .Found()
                        .PlaceCurrentFrom(bottomRoom)

                        .FindPath(() => ldk.con.ConnectByDoor(newRoom.LE, what.LE).GN(sym.ConnectionMarker), out var door)
                        .PlaceCurrentFrom(newRoom, what)

                        // The door doesn't get overwritten by apply style only because it has higher priority, which doesn't feel robust enough
                        .FindPath(() => ldk.con.ConnectByFall(newRoom.LE, bottomRoom.LE).GN(), out var fall)
                        .PlaceCurrentFrom(bottomRoom, newRoom);

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
                    .AddNodeSymbols(from, sym.FullFloorMarker)
                    .AddNodeSymbols(to, sym.FullFloorMarker)
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

                    return state.NewProgram()
                        .SelectOne(
                            state.NewProgram()
                                .Set(() => roomFromF().GN())
                                .MoveNearTo(to)
                                .Change(node => node.LE.MoveBy(Vector3Int.up).GN(sym.Room(false, 1), sym.FullFloorMarker))
                                ,
                            out var newRoom
                        )
                        .PlaceCurrentFrom(from)

                        .Found(out var foundation)
                        .PlaceCurrentFrom(newRoom)

                        .Set(() => newRoom)
                        .ReserveUpward(2, out var reservation)
                        .PlaceCurrentFrom(newRoom)

                        .FindPath(() => ldk.con.ConnectByBalconyStairsOutside(state.WorldState.Added.Merge(foundation.LE).Merge(reservation.LE))(from.LE, newRoom.LE).GN(sym.ConnectionMarker), out var stairs)
                        .PlaceCurrentFrom(from, newRoom)

                        .FindPath(() => ldk.con.ConnectByFall(newRoom.LE, to.LE).GN(), out var fall)
                        .PlaceCurrentFrom(to, newRoom);
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
                        .GN(sym.Room(false))))
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

                    var middleGn = middle.SetAreaType(AreaType.NoFloor).GN();
                    var path = floorConnector(start, middle, end).SetAreaType(AreaType.Platform);
                    //ldk.con.ConnectByStairsInside(start, end, newHouse.LevelElement);

                    var door = ldk.con.ConnectByDoor(room.LE, start).GN();

                    //todo: test if the reservation is ok
                    var reservation = newHouse.LE.CG().ExtrudeVer(Vector3Int.up, 2).LE(AreaType.Reservation).GN(sym.UpwardReservation(newHouse));
                    // and modify the dag
                    var foundation = ldk.sgShapes.Foundation(newHouse.LE).GN(sym.Foundation);
                    var startFoundation = ldk.sgShapes.Foundation(start).GN(sym.Foundation);
                    var endFoundation = ldk.sgShapes.Foundation(end).GN(sym.Foundation);
                    return state.NewProgramBadMethodDestroyItASAP(new[]
                    {
                        state.Add(room).SetTo(newHouse),

                        state.Add(newHouse).SetTo(foundation),
                        state.Add(foundation).SetTo(startFoundation),
                        state.Add(foundation).SetTo(endFoundation),

                        state.Add(newHouse).SetTo(reservation),

                        state.Add(newHouse, room).SetTo(door),
                        state.Add(newHouse).SetTo(start.GN(), middleGn, end.GN()),
                        state.Replace(middleGn).SetTo(path.GN())
                    });
                    /*
                    return state.NewProgram()
                        .SelectOne(
                            state.NewProgram()
                                .Directional(ExtensionMethods.HorizontalDirections().Shuffle(),
                                    dir =>
                                        roomCubeGroup
                                        .ExtrudeDir(dir, 6)
                                        .LE(AreaType.Room)
                                        .GN(sym.DirectedRoom(0, dir))
                                )
                                .NotTaken()
                                .CanBeFounded(),
                            out var newRoom
                        )*/
                });
        }

        public Production AddNextFloor()
        {
            return new Production(
                "AddNextFloor",
                new ProdParamsManager()
                    .AddNodeSymbols(sym.UpwardReservation(null))
                    .SetCondition((state, pp) =>
                    {
                        var roomBelow = pp.Param.GetSymbol<UpwardReservation>().RoomBelow.GetSymbol<Room>();
                        return roomBelow != null && roomBelow.Plain && roomBelow.Floor <= 1;
                    })
                    ,
                (state, pp) =>
                {
                    var roomReservation = pp.Param;
                    var roomBelow = roomReservation.GetSymbol<UpwardReservation>().RoomBelow;

                    return state.NewProgram()
                        .Set(() => roomReservation.LE.SetAreaType(AreaType.Room)
                            .GN(
                                sym.Room(true, roomBelow.GetSymbol<Room>().Floor + 1),
                                sym.FullFloorMarker),
                                out var nextFloor
                        )
                        .ReplaceNodes(roomReservation)

                        .ReserveUpward(2)
                        .PlaceCurrentFrom(nextFloor)

                        .FindPath(() => ldk.con.ConnectByWallStairsIn(roomBelow.LE, nextFloor.LE).GN(sym.ConnectionMarker), out var stairs)
                        .PlaceCurrentFrom(roomBelow, nextFloor);
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
                            .GN(sym.Garden, sym.FullFloorMarker)
                        )
                        //todo: remove cubes that can't be founded directly instead of asking about the whole group at the end
                        .Where(garden => garden.LE.Cubes().Count() >= 8 && state.CanBeFounded(garden.LE));

                    if (!gardens.Any())
                        return null;

                    var garden = gardens.FirstOrDefault();

                    garden.LE.ApplyGrammarStyleRules(ldk.houseStyleRules);

                    var cliffFoundation = ldk.sgShapes.CliffFoundation(garden.LE).GN(sym.Foundation);
                    var stairs = ldk.con.ConnectByElevator(courtyard.LE, garden.LE).GN(sym.ConnectionMarker);

                    return state.NewProgramBadMethodDestroyItASAP(new[]
                    {
                        state.Add(courtyard).SetTo(garden),
                        state.Add(garden).SetTo(cliffFoundation),
                        state.Add(garden, courtyard).SetTo(stairs),
                    });
                    /*
                    return state.NewProgram()
                        .SelectOne(
                            state.NewProgram()
                                //.Set(() => courtyardCubeGroup.CubeGroupMaxLayer(Vector3Int.down).ExtrudeHor().MoveBy(3 * Vector3Int.down).LE().GN())
                                .Directional(ExtensionMethods.HorizontalDirections().Shuffle(),
                                    dir =>
                                        ldk.sgShapes.IslandExtrudeIter(possibleStartCubes.CubeGroupMaxLayer(dir), 3, 0.7f)
                                            .LE(AreaType.Garden).Minus(state.WorldState.Added)
                                            .MapGeom(cg => cg
                                                .SplitToConnected().ArgMax(cg => cg.Cubes.Count)
                                                .OpAdd().ExtrudeVer(Vector3Int.up, 3))
                                            .GN(sym.Garden, sym.FullFloorMarker)
                                )
                                .Where(garden => garden.LE.Cubes().Count() >= 8)
                                .CanBeFounded(),
                            out var garden
                        )
                        .PlaceNodes(courtyard)
                        
                        .Set(() => ldk.sgShapes.CliffFoundation(garden.LE).GN(sym.Foundation))
                        .PlaceNodes(garden)

                        .FindPath(() => ldk.con.ConnectByElevator(courtyard.LE, garden.LE).GN(), out var elevator)
                        .PlaceNodes(garden, courtyard)*/
                });
        }

        public Production Roof()
        {
            return new Production(
                "AddRoof",
                new ProdParamsManager()
                    .AddNodeSymbols(sym.UpwardReservation(null)),
                (state, pp) =>
                {
                    var roomReservation = pp.Param;
                    var roof = roomReservation.LE.SetAreaType(AreaType.GableRoof).GN(sym.Roof);

                    // and modify the dag
                    return state.NewProgramBadMethodDestroyItASAP(new[]
                    {
                        state.Replace(roomReservation).SetTo(roof),
                    });
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

        #region Graveyard

        public Production FullFloorNextTo(Symbol nextToWhat, Symbol newAreaSym, 
            Func<LevelElement> parkF, 
            Func<ProductionProgram, Node, ProductionProgram> fromFloorNodeAfterPositionedNear,
            Func<ProductionProgram, Node, ProductionProgram> fromFloorNodeAfterPlaced,
            ConnectionNotIntersecting connectionNotIntersecting)
        {
            return new Production(
                $"{newAreaSym}_NextTo_{nextToWhat.Name}",
                new ProdParamsManager().AddNodeSymbols(nextToWhat),
                (state, pp) =>
                {
                    var what = pp.Param;
                    var whatCG = what.LE.CG();

                    return state.NewProgram()
                        .SelectOne(
                            state.NewProgram()
                                .Set(() => parkF().GN())
                                .MoveNearTo(what)
                                .CurrentFirst(out var newArea)
                                .RunIf(true,
                                    thisProg => fromFloorNodeAfterPositionedNear(thisProg, newArea)
                                )
                                .Change(node => node.LE.GN(newAreaSym, sym.FullFloorMarker)),
                            out var newPark
                            )
                        .PlaceCurrentFrom(what)

                        .Found(out var foundation)
                        .PlaceCurrentFrom(newPark)

                        .RunIf(true,
                            thisProg => fromFloorNodeAfterPlaced(thisProg, newPark)
                        )

                        //Replace with open connection
                        .FindPath(() => 
                            connectionNotIntersecting
                                (state.WorldState.Added.Merge(foundation.LE))
                                (newPark.LE, what.LE)
                                .GN(sym.ConnectionMarker), out var door)
                        .PlaceCurrentFrom(what, newPark);
                });
        }

        public Production ParkNextTo(Symbol nextToWhat, Func<LevelElement> parkF)
        {
            return FullFloorNextTo(nextToWhat, sym.Park, () => parkF().SetAreaType(AreaType.Garden),
                (program, _) => program, 
                (program, _) => program,
                 _ => ldk.con.ConnectByDoor);
        }

        public Production ChapelNextTo(Symbol nextToWhat, Func<LevelElement> chapelEntranceF)
        {
            return FullFloorNextTo(nextToWhat, sym.ChapelEntrance, () => chapelEntranceF().SetAreaType(AreaType.Room),
                (program, _) => program,
                (program, chapelEntrance) => program
                                .Set(() => chapelEntrance.LE.CG().ExtrudeVer(Vector3Int.up, 2).LE(AreaType.CrossRoof).GN(sym.Roof))
                                .PlaceCurrentFrom(chapelEntrance),
                 _ => ldk.con.ConnectByDoor);
        }



        public Production ChapelHall(Symbol extrudeFrom, int length, PathGuide pathGuilde)
        {
            return new Production(
                $"ChapelHall",
                new ProdParamsManager().AddNodeSymbols(extrudeFrom),
                (state, pp) =>
                {
                    var entrance = pp.Param;
                    var whatCG = entrance.LE.CG();

                    return state.NewProgram()
                        .SelectOne(
                            state.NewProgram()
                                .Directional(pathGuilde.SelectDirections(entrance.LE),
                                    dir =>
                                        entrance.LE.CG().ExtrudeDir(dir, length).LE(AreaType.Room).GN(sym.ChapelHall(dir), sym.FullFloorMarker)
                                )
                                .NotTaken()
                                .CanBeFounded(),
                            out var newChapelHall
                            )
                        .PlaceCurrentFrom(entrance)

                        .Found()
                        .PlaceCurrentFrom(newChapelHall)

                        .Set(() => newChapelHall)
                        .ReserveUpward(2)
                        .PlaceCurrentFrom(newChapelHall)

                        .FindPath(() => ldk.con.ConnectByDoor(newChapelHall.LE, entrance.LE).GN(sym.ConnectionMarker), out var door)
                        .PlaceCurrentFrom(entrance, newChapelHall);
                });
        }

        public Production ChapelRoom(int extrusionLength)
        {
            return new Production(
                $"ChapelRoom",
                new ProdParamsManager().AddNodeSymbols(sym.ChapelHall(default)),
                (state, pp) =>
                {
                    var hall = pp.Param;

                    return state.NewProgram()
                        .Set(() => hall)
                        .Change(h => h.LE.CG()
                            .ExtrudeDir(hall.GetSymbol<ChapelHall>().Direction, extrusionLength).LE(AreaType.Room).GN(sym.ChapelRoom(true, 0), sym.FullFloorMarker))
                        .NotTaken()
                        .CanBeFounded()
                        .CurrentFirst(out var newRoom)
                        .PlaceCurrentFrom(hall)
                        
                        .Found()
                        .PlaceCurrentFrom(newRoom)

                        .Set(() => newRoom)
                        .ReserveUpward(2)
                        .PlaceCurrentFrom(newRoom)
                        
                        .FindPath(() => ldk.con.ConnectByDoor(newRoom.LE, hall.LE).GN(sym.ConnectionMarker), out var door)
                        .PlaceCurrentFrom(hall, newRoom);
                });
        }

        public Production ChapelNextFloor(int nextFloorHeight, int maxFloor)
        {
            return new Production(
                $"ChapelNextFloor",
                new ProdParamsManager()
                    .AddNodeSymbols(sym.UpwardReservation(default))
                    .SetCondition((state, pp) =>
                    {
                        var roomBelow = pp.Param.GetSymbol<UpwardReservation>().RoomBelow.GetSymbol<ChapelRoom>();
                        return 
                            roomBelow != null && 
                            roomBelow.Floor < maxFloor;
                    }),
                (state, pp) =>
                {
                    var reservation = pp.Param;
                    var reservationCG = reservation.LE.CG();
                    var resHeight = reservationCG.Extents().y;
                    var toExtrude = nextFloorHeight - resHeight;
                    var roomBelow = pp.Param.GetSymbol<UpwardReservation>().RoomBelow;
                    var roomBelowFloor = roomBelow.GetSymbol<ChapelRoom>().Floor;

                    return state.NewProgram()
                        .Condition(() => toExtrude >= 0)
                        .Set(() => reservation)
                        .Change(res => res.LE.CG()
                            .ExtrudeDir(Vector3Int.up, toExtrude).LE().GN())
                        .NotTaken()
                        .Change(extr => extr.LE.CG().Merge(reservationCG).LE(AreaType.Room).GN(sym.ChapelRoom(true, roomBelowFloor), sym.FullFloorMarker))
                        .CurrentFirst(out var nextFloor)
                        .ReplaceNodes(reservation)

                        .ReserveUpward(2)
                        .PlaceCurrentFrom(nextFloor)

                        .FindPath(() => ldk.con.ConnectByStairsInside(nextFloor.LE, roomBelow.LE).GN(sym.ConnectionMarker), out var stairs)
                        .PlaceCurrentFrom(roomBelow, nextFloor);
                });
        }

        public Production ChapelTowerTop(int roofHeight)
        {
            return new Production(
                $"ChapelNextFloor",
                new ProdParamsManager()
                    .AddNodeSymbols(sym.UpwardReservation(default))
                    .SetCondition((state, pp) =>
                    {
                        var roomBelow = pp.Param.GetSymbol<UpwardReservation>().RoomBelow.GetSymbol<ChapelRoom>();
                        return roomBelow != null;
                    }),
                (state, pp) =>
                {
                    var reservation = pp.Param;
                    var roomBelow = pp.Param.GetSymbol<UpwardReservation>().RoomBelow;

                    return state.NewProgram()
                        .Set(() => reservation.LE.SetAreaType(AreaType.Colonnade).GN(sym.ChapelTowerTop, sym.FullFloorMarker))
                        .CurrentFirst(out var towerTop)
                        .ReplaceNodes(reservation)

                        .Change(towerTop => towerTop.LE.CG().ExtrudeVer(Vector3Int.up, roofHeight).LE(AreaType.PointyRoof).GN(sym.Roof))
                        .PlaceCurrentFrom(towerTop)

                        .FindPath(() => ldk.con.ConnectByStairsInside(towerTop.LE, roomBelow.LE).GN(sym.ConnectionMarker), out var stairs)
                        .PlaceCurrentFrom(roomBelow, towerTop);
                });
        }

        public Production Park(Symbol nextToWhat, int heightChangeAmount, int minHeight, Func<LevelElement> parkF)
        {
            return FullFloorNextTo(nextToWhat, sym.Park, () => parkF().SetAreaType(AreaType.Garden),
                (program, park) => program
                        .Set(() => park)
                        .Change(park => park
                                .LE.MoveBottomBy(heightChangeAmount, minHeight).CG()
                                .LE(AreaType.Garden).GN()),
                (program, _) => program,
                 ldk.con.ConnectByBalconyStairsOutside);
        }
        #endregion
    }
}

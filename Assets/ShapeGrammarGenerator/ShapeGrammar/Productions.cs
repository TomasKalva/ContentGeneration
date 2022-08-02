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
            return Place(sym.Room, 3, () => ldk.sgShapes.Room(new Box2Int(0, 0, 5, 5).InflateY(0, 2)), Reserve(2, sym.UpwardReservation));
        }

        public Production CourtyardFromRoom()
        {
            return new Production(
                "CourtyardFromRoom",
                new ProdParamsManager().AddNodeSymbols(sym.Room),
                (state, pp) =>
                {
                    var room = pp.Param;
                    var roomCubeGroup = room.LE.CG();

                    // Reduces number of characters (withou spaces) from ~800 to ~480, from 34 lines to 22
                    return state.NewProgram(prog => prog
                        .SelectOne(
                            state.NewProgram(subProg => subProg
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
                                .CanBeFounded()
                                ),
                            out var courtyard
                        )
                        .PlaceCurrentFrom(room)

                        .Found()
                        .PlaceCurrentFrom(courtyard)

                        .FindPath(() => ldk.con.ConnectByDoor(room.LE, courtyard.LE).GN(sym.ConnectionMarker), out var door)
                        .PlaceCurrentFrom(room, courtyard)
                        );
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

                    return state.NewProgram(prog => prog
                        .SelectOne(
                            state.NewProgram(subProg => subProg
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
                                .CanBeFounded()
                                ),
                            out var newCourtyard
                        )
                        .PlaceCurrentFrom(courtyard)

                        .Found()
                        .PlaceCurrentFrom(newCourtyard)

                        .FindPath(() => ldk.con.ConnectByStairsInside(courtyard.LE, newCourtyard.LE).GN(sym.ConnectionMarker), out var p)
                        .PlaceCurrentFrom(courtyard, newCourtyard)
                        );
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
                    
                    return state.NewProgram(prog => prog
                        .SelectOne(
                            state.NewProgram(subProg => subProg
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
                                .CanBeFounded()
                                ),
                            out var bridge
                        )
                        .PlaceCurrentFrom(courtyard)

                        .EmptyPath()
                        .PlaceCurrentFrom(courtyard, bridge)

                        .Set(() => ldk.sgShapes.BridgeFoundation(
                            bridge.LE,
                            bridge.GetSymbol<Bridge>(sym.Bridge()).Direction
                            ).GN(sym.Foundation))
                        .PlaceCurrentFrom(bridge)
                        );
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
                    
                    var dir = bridge.GetSymbol<Bridge>(sym.Bridge()).Direction;

                    return state.NewProgram(prog => prog
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
                            newBridge.GetSymbol<Bridge>(sym.Bridge()).Direction
                            ).GN(sym.Foundation))
                        .PlaceCurrentFrom(newBridge)
                        );
                     
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
                    var dir = bridge.GetSymbol<Bridge>(sym.Bridge()).Direction;

                    return state.NewProgram(prog => prog
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
                        .PlaceCurrentFrom(bridge, newCourtyard)
                        );
                    
                });
        }

        public Production RoomNextTo(Symbol nextToWhat, Func<LevelElement> roomF)
        {
            return FullFloorPlaceNear(nextToWhat, sym.Room, roomF,
                (program, _) => program,
                (program, newRoom) => program
                        .Set(() => newRoom)
                        .ReserveUpward(2, sym.UpwardReservation)
                        .PlaceCurrentFrom(newRoom),
                _ => ldk.con.ConnectByDoor,
                1
                );
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
                    return state.NewProgram(prog => prog
                        .SelectOne(
                            state.NewProgram(subProg => subProg
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
                                    newRoomDown => newRoomDown.LE.MoveBottomTo(whatCG.LeftBottomBack().y).GN(sym.Room, sym.FullFloorMarker)
                                )
                                ),
                                out var newRoom
                        )
                        .PlaceCurrentFrom(what)

                        .Found()
                        .PlaceCurrentFrom(newRoom)

                        .RunIf(addFloorAbove,
                            thisProg => thisProg
                                .Set(() => newRoom)
                                .ReserveUpward(2, sym.UpwardReservation)
                                .PlaceCurrentFrom(newRoom)
                        )

                        .FindPath(() => ldk.con.ConnectByBridge(state.WorldState.Added)(what.LE, newRoom.LE).GN(sym.ConnectionMarker), out var bridge)
                        .PlaceCurrentFrom(what, newRoom)
                        );
                     
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

                    return state.NewProgram(prog => prog
                        .SelectOne(
                            state.NewProgram(subProg => subProg
                                .Set(() => roomFromToF().GN())
                                .MoveNearTo(what, 1)
                                .Change(node => node.LE.GN(sym.BrokenFloorRoom, sym.FullFloorMarker))
                                ),
                            out var newRoom
                        )
                        .PlaceCurrentFrom(what)
                        
                        .ReserveUpward(2, sym.UpwardReservation)
                        .PlaceCurrentFrom(what)
                        
                        .Set(() =>
                            newRoom.LE.CG().ExtrudeVer(Vector3Int.down, 2).LE(AreaType.Room).GN(sym.Room, sym.FullFloorMarker),
                            out var bottomRoom
                        )
                        .PlaceCurrentFrom(newRoom)
                        
                        .Found()
                        .PlaceCurrentFrom(bottomRoom)

                        .FindPath(() => ldk.con.ConnectByDoor(newRoom.LE, what.LE).GN(sym.ConnectionMarker), out var door)
                        .PlaceCurrentFrom(newRoom, what)

                        // The door doesn't get overwritten by apply style only because it has higher priority, which doesn't feel robust enough
                        .FindPath(() => ldk.con.ConnectByFall(newRoom.LE, bottomRoom.LE).GN(), out var fall)
                        .PlaceCurrentFrom(bottomRoom, newRoom)
                        );

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

                    return state.NewProgram(prog => prog
                        .SelectOne(
                            state.NewProgram(subProg => subProg
                                .Set(() => roomFromF().GN())
                                .MoveNearTo(to, 1)
                                .Change(node => node.LE.MoveBy(Vector3Int.up).GN(sym.Room, sym.FullFloorMarker))
                                ),
                            out var newRoom
                        )
                        .PlaceCurrentFrom(from)

                        .Found(out var foundation)
                        .PlaceCurrentFrom(newRoom)

                        .Set(() => newRoom)
                        .ReserveUpward(2, sym.UpwardReservation, out var reservation)
                        .PlaceCurrentFrom(newRoom)

                        .FindPath(() => ldk.con.ConnectByBalconyStairsOutside(state.WorldState.Added.Merge(foundation.LE).Merge(reservation.LE))(from.LE, newRoom.LE).GN(sym.ConnectionMarker), out var stairs)
                        .PlaceCurrentFrom(from, newRoom)

                        .FindPath(() => ldk.con.ConnectByFall(newRoom.LE, to.LE).GN(), out var fall)
                        .PlaceCurrentFrom(to, newRoom)
                        );
                });
        }

        public Production ExtendHouse(FloorConnector floorConnector)
        {
            return new Production(
                "ExtendHouse",
                new ProdParamsManager()
                    .AddNodeSymbols(sym.Room),
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
                        .GN(sym.Room)))
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
        /*
        public Production AddNextFloor()
        {
            return new Production(
                "AddNextFloor",
                new ProdParamsManager()
                    .AddNodeSymbols(sym.UpwardReservation(null))
                    .SetCondition((state, pp) =>
                    {
                        var roomBelow = pp.Param.GetSymbol<UpwardReservation>(sym.UpwardReservation(null)).SomethingBelow.GetSymbol<Room>(sym.Room());
                        return roomBelow != null && roomBelow.Plain && roomBelow.Floor <= 1;
                    })
                    ,
                (state, pp) =>
                {
                    var roomReservation = pp.Param;
                    var roomBelow = roomReservation.GetSymbol<UpwardReservation>(sym.UpwardReservation(null)).SomethingBelow;

                    return state.NewProgram(prog => prog
                        .Set(() => roomReservation.LE.SetAreaType(AreaType.Room)
                            .GN(
                                sym.Room(true, roomBelow.GetSymbol<Room>(sym.Room()).Floor + 1),
                                sym.FullFloorMarker),
                                out var nextFloor
                        )
                        .ReplaceNodes(roomReservation)

                        .ReserveUpward(2, sym.UpwardReservation)
                        .PlaceCurrentFrom(nextFloor)

                        .FindPath(() => ldk.con.ConnectByWallStairsIn(roomBelow.LE, nextFloor.LE).GN(sym.ConnectionMarker), out var stairs)
                        .PlaceCurrentFrom(roomBelow, nextFloor)
                        );
                });
        }*/

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
        #region Utility

        /// <summary>
        /// Moves the given current node by heightChange. The bottom layer will not go below minHeight.
        /// </summary>
        public Func<ProductionProgram, Node, ProductionProgram> MoveVertically(int heightChange, int minHeight)
        {
            return (program, node) => program
                        .Set(() => node)
                        .Change(node => node
                                .LE.MoveBottomBy(heightChange, minHeight).GN());
        }

        /// <summary>
        /// Just returns the program.
        /// </summary>
        public Func<ProductionProgram, Node, ProductionProgram> Empty()
        {
            return (program, node) => program;
        }

        public Func<ProductionProgram, Node, ProductionProgram> Reserve(int reservationHeight, Func<Node, Symbol> reservationSymbolF)
        {
            return (program, node) => program
                        .Set(() => node)
                        .ReserveUpward(reservationHeight, reservationSymbolF)
                        .PlaceCurrentFrom(node);
        }

        public Func<ProductionProgram, Node, ProductionProgram> Roof(AreaType roofType, int roofHeight)
        {
            return (program, towerTop) => program
                        .Set(() => towerTop)
                        .Change(towerTop => towerTop.LE.CG().ExtrudeVer(Vector3Int.up, roofHeight).LE(roofType).GN(sym.Roof))
                        .NotTaken()
                        .PlaceCurrentFrom(towerTop);
        }

        public CubeGroup ShrinkInOrthogonalDirection(CubeGroup cg, Vector3Int dir, int newWidth)
        {
            var orthDir = dir.OrthogonalHorizontalDirs().First();
            var width = cg.ExtentsDir(orthDir);
            var shrinkL = Math.Max(0, (int)Mathf.Floor((width - newWidth) / 2f));
            var shrinkR = Math.Max(0, (int)Mathf.Ceil((width - newWidth) / 2f));

            return cg
                .OpSub()
                    .ExtrudeDir(orthDir, -shrinkL)
                    .ExtrudeDir(-orthDir, -shrinkR)
                .OpNew();
        }

        LevelElement AllBlocking(ShapeGrammarState state, ProductionProgram prog, Grid<Cube> grid) 
            => state.WorldState.Added.Merge(prog.AppliedOperations.SelectMany(op => op.To.Select(n => n.LE)).ToLevelGroupElement(grid));

        #endregion

        #region Operations

        public Production Place(Symbol newSymbol, int bottomHeight, Func<LevelElement> leF, Func<ProductionProgram, Node, ProductionProgram> fromNodeAfterPlaced)
        {
            return new Production(
                $"Place {newSymbol.Name}",
                new ProdParamsManager(),
                (state, pp) =>
                {
                    return state.NewProgram(
                        prog => prog
                            .Set(() => leF()
                                .MoveBottomTo(bottomHeight)
                                .MovesToNotIntersectXZ(state.WorldState.Added.LevelElements)
                                .TryMove()
                                .GN(newSymbol, sym.FullFloorMarker))
                            .CurrentFirst(out var first)
                            .PlaceCurrentFrom(state.Root)

                            .Found()
                            .PlaceCurrentFrom(first)

                            .RunIf(true, prog => fromNodeAfterPlaced(prog, first))
                            );
                });
        }

        public Production FullFloorPlaceNear(
            Symbol nearWhat,
            Symbol newAreaSym,
            Func<LevelElement> newAreaF,
            Func<ProductionProgram, Node, ProductionProgram> fromFloorNodeAfterPositionedNear,
            Func<ProductionProgram, Node, ProductionProgram> fromFloorNodeAfterPlaced,
            ConnectionNotIntersecting connectionNotIntersecting,
            int dist)
        {
            return new Production(
                $"{newAreaSym.Name}_NextTo_{nearWhat.Name}",
                new ProdParamsManager().AddNodeSymbols(nearWhat),
                (state, pp) =>
                {
                    var what = pp.Param;
                    var whatCG = what.LE.CG();

                    return state.NewProgram(prog => prog
                        .SelectOne(
                            state.NewProgram(subProg => subProg
                                .Set(() => newAreaF().GN())
                                .MoveNearTo(what, dist)
                                .CurrentFirst(out var newArea)
                                .RunIf(true,
                                    thisProg => fromFloorNodeAfterPositionedNear(thisProg, newArea)
                                )
                                .Change(node => node.LE.GN(newAreaSym, sym.FullFloorMarker))
                                ),
                            out var newNode
                            )
                        .PlaceCurrentFrom(what)

                        .Found(out var foundation)
                        .PlaceCurrentFrom(newNode)

                        .RunIf(true,
                            thisProg => fromFloorNodeAfterPlaced(thisProg, newNode)
                        )

                        //Replace with open connection
                        .FindPath(() =>
                            connectionNotIntersecting
                                (AllBlocking(state, prog, what.LE.Grid))
                                (newNode.LE, what.LE)
                                .GN(sym.ConnectionMarker), out var door)
                        .PlaceCurrentFrom(what, newNode)
                        );
                });
        }

        public Production Extrude(
            Symbol extrudeFrom,
            Func<Node, IEnumerable<Vector3Int>> directionsFromSelected,
            Func<CubeGroup, Vector3Int, Node> nodeFromExtrudedDirection,
            int length,
            Func<ProductionProgram, Node, ProductionProgram> fromFloorNodeAfterPlaced,
            ConnectionNotIntersecting connectionNotIntersecting)
        {
            return new Production(
                $"Extrude{extrudeFrom}",
                new ProdParamsManager().AddNodeSymbols(extrudeFrom),
                (state, pp) =>
                {
                    var from = pp.Param;
                    var fromCG = from.LE.CG();

                    return state.NewProgram(prog => prog
                        .SelectOne(
                            state.NewProgram(subProg => subProg
                                .Directional(directionsFromSelected(from),
                                    dir =>
                                        nodeFromExtrudedDirection(from.LE.CG().ExtrudeDir(dir, length), dir)
                                )
                                .NotTaken()
                                .CanBeFounded()
                                ),
                            out var newChapelHall
                            )
                        .PlaceCurrentFrom(from)

                        .Found(out var foundation)
                        .PlaceCurrentFrom(newChapelHall)

                        .RunIf(true, newProg =>
                            fromFloorNodeAfterPlaced(newProg, newChapelHall))

                        .FindPath(() =>
                        connectionNotIntersecting(state.WorldState.Added.Merge(foundation.LE))
                            (newChapelHall.LE, from.LE).GN(sym.ConnectionMarker), out var door)
                        .PlaceCurrentFrom(from, newChapelHall)
                        );
                });
        }

        public Production TakeUpwardReservation(
            Symbol reservationSymbol,
            Func<CubeGroup, Node> nodeFromExtrudedUp,
            int nextFloorHeight,
            int maxBottomHeight,
            Func<ProductionProgram, Node, ProductionProgram> fromFloorNodeAfterPlaced,
            ConnectionNotIntersecting connection)
        {
            return new Production(
                $"TakeUpwardReservation_{reservationSymbol.Name}",
                new ProdParamsManager()
                    .AddNodeSymbols(sym.UpwardReservation(null))
                    .SetCondition((state, pp) =>
                    {
                        bool correctBelowSymbol = 
                            pp.Param.GetSymbol(sym.UpwardReservation(null))
                            .SomethingBelow.GetSymbol(reservationSymbol) != null;
                        return correctBelowSymbol && pp.Param.LE.CG().RightTopFront().y + 1 <= maxBottomHeight;
                    }),
                (state, pp) =>
                {
                    var reservation = pp.Param;
                    var reservationCG = reservation.LE.CG();
                    var toExtrude = nextFloorHeight - 1;
                    var roomBelow = pp.Param.GetSymbol<UpwardReservation>(sym.UpwardReservation(null)).SomethingBelow;

                    return state.NewProgram(prog => prog
                        .Condition(() => toExtrude >= 0)
                        .Set(() => reservation)
                        .Change(res => res.LE.CG().BottomLayer()
                            .OpAdd().ExtrudeDir(Vector3Int.up, toExtrude).OpNew().LE().GN())
                        .CurrentFirst(out var extendedReservation)

                        // Only check if the part outside of the reservation was not taken yet
                        .Set(() => extendedReservation.LE.Minus(reservation.LE).GN())
                        .NotTaken()
                        .Set(() => extendedReservation)

                        .Change(extr => nodeFromExtrudedUp(extr.LE.CG())/*.GN(nodeFromExtrudedUp(0), sym.FullFloorMarker)*/)
                        .CurrentFirst(out var nextFloor)
                        .ReplaceNodes(reservation)

                        .RunIf(true, prog => fromFloorNodeAfterPlaced(prog, nextFloor))

                        .FindPath(() => connection(AllBlocking(state, prog, reservation.LE.Grid))
                            (nextFloor.LE, roomBelow.LE).GN(sym.ConnectionMarker), out var stairs)
                        .PlaceCurrentFrom(roomBelow, nextFloor)
                        );
                });
        }
        /*
        IEnumerable<Symbol> SymbolsUnder(Node node)
        {
            return node.LE.CG().ExtrudeDir(Vector3Int.down, 1).
        }*/
        Node FindFoundation(Node node)
        {
            return node.Derived.Where(child => child.HasSymbols(sym.Foundation)).FirstOrDefault();
        }


        public Production FromDownwardFoundation(
            Symbol fromSymbol,
            Func<CubeGroup, Node> nodeFromExtrudedDown,
            int floorHeight,
            int minBottomHeight,
            ConnectionNotIntersecting connection)
        {
            return new Production(
                $"FromDownwardFoundation_{fromSymbol.Name}",
                new ProdParamsManager()
                    .AddNodeSymbols(fromSymbol)
                    .SetCondition((state, pp) =>
                    {
                        var foundationBelow = FindFoundation(pp.Param);
                        return 
                            foundationBelow != null &&
                            pp.Param.LE.CG().LeftBottomBack().y - floorHeight >= minBottomHeight;
                    }),
                (state, pp) =>
                {
                    var upFloor = pp.Param;
                    var foundationBelow = FindFoundation(pp.Param);

                    return state.NewProgram(prog => prog
                        .Set(() => upFloor)
                        .Change(res => res.LE.CG().BottomLayer()
                            .ExtrudeDir(Vector3Int.down, floorHeight).LE().GN())
                        .CurrentFirst(out var extendedDown)

                        // Only check if the part outside of the reservation was not taken yet
                        .Set(() => extendedDown.LE.Minus(foundationBelow.LE).GN())
                        .NotTaken()
                        .Set(() => extendedDown)

                        .Change(extr => nodeFromExtrudedDown(extr.LE.CG()))
                        .CurrentFirst(out var downFloor)
                        .ReplaceNodes(foundationBelow)

                        .Found()
                        .PlaceCurrentFrom(downFloor)

                        .FindPath(() => connection(AllBlocking(state, prog, upFloor.LE.Grid))
                            (downFloor.LE, upFloor.LE).GN(sym.ConnectionMarker), out var stairs)
                        .PlaceCurrentFrom(upFloor, downFloor)
                        );
                });
        }

        #endregion

        public Production BridgeFrom(Symbol from, PathGuide pathGuide)
        {
            return Extrude(
                    from,
                    node => pathGuide.SelectDirections(node.LE),
                    (cg, dir) => ShrinkInOrthogonalDirection(cg, dir, 3).LE(AreaType.Colonnade).GN(sym.Bridge(dir), sym.FullFloorMarker),
                    6,
                    Empty(),
                    ldk.con.ConnectByBalconyStairsOutside
                );
        }

        public Production RoomDown(Symbol from)
        {
            return FromDownwardFoundation(
                    from,
                    cg => cg.LE(AreaType.Room).GN(sym.FullFloorMarker, sym.ChapelRoom),
                    2,
                    3,
                    _ => ldk.con.ConnectByWallStairsIn
                );
        }

        #region Graveyard



        public Production ParkNextTo(Symbol nextToWhat, Func<LevelElement> parkF)
        {
            return FullFloorPlaceNear(nextToWhat, sym.Park, () => parkF().SetAreaType(AreaType.Garden),
                (program, _) => program, 
                (program, _) => program,
                 _ => ldk.con.ConnectByDoor,
                 1);
        }

        public Production ChapelNextTo(Symbol nextToWhat, Func<LevelElement> chapelEntranceF)
        {
            return FullFloorPlaceNear(nextToWhat, sym.ChapelEntrance, () => chapelEntranceF().SetAreaType(AreaType.Room),
                (program, _) => program,
                (program, chapelEntrance) => program
                                .Set(() => chapelEntrance.LE.CG().ExtrudeVer(Vector3Int.up, 2).LE(AreaType.CrossRoof).GN(sym.Roof))
                                .NotTaken()
                                .PlaceCurrentFrom(chapelEntrance),
                 _ => ldk.con.ConnectByDoor,
                 1);
        }

        public Production Park(Symbol nextToWhat, int heightChangeAmount, int minHeight, Func<LevelElement> parkF)
        {
            return FullFloorPlaceNear(nextToWhat, sym.Park, () => parkF().SetAreaType(AreaType.Garden),
                (program, park) => program
                        .Set(() => park)
                        .Change(park => park
                                .LE.MoveBottomBy(heightChangeAmount, minHeight).CG()
                                .LE(AreaType.Garden).GN()),
                (program, _) => program,
                 ldk.con.ConnectByBalconyStairsOutside,
                 1);
        }



        public Production ChapelHall(Symbol extrudeFrom, int length, PathGuide pathGuide)
        {
            return Extrude(
                extrudeFrom,
                node => pathGuide.SelectDirections(node.LE),
                (extrLE, dir) => extrLE.LE(AreaType.Room).GN(sym.ChapelHall(dir), sym.FullFloorMarker),
                length,
                Reserve(2, sym.UpwardReservation),
                _ => ldk.con.ConnectByDoor
                );
        }

        public Production ChapelRoom(int extrusionLength)
        {
            return Extrude(
                sym.ChapelHall(default),
                node => node.GetSymbol(sym.ChapelHall(default)).Direction.ToEnumerable(),
                (extrLE, dir) => extrLE.LE(AreaType.Room).GN(sym.ChapelRoom, sym.FullFloorMarker),
                extrusionLength,
                Reserve(2, sym.UpwardReservation),
                _ => ldk.con.ConnectByDoor
                );
        }

        public Production ChapelNextFloor(int nextFloorHeight, int maxFloor)
        {
            return TakeUpwardReservation(
                    sym.ChapelRoom,
                    nextFloor => nextFloor.LE(AreaType.Room).GN(sym.ChapelRoom, sym.FullFloorMarker),
                    nextFloorHeight,
                    maxFloor,
                    Reserve(2, sym.UpwardReservation),
                    _ => ldk.con.ConnectByWallStairsIn);
        }

        /*
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

                    return state.NewProgram(prog => prog
                        .Condition(() => toExtrude >= 0)
                        .Set(() => reservation)
                        .Change(res => res.LE.CG()
                            .ExtrudeDir(Vector3Int.up, toExtrude).LE().GN())
                        .NotTaken()
                        .Change(extr => extr.LE.CG().Merge(reservationCG).LE(AreaType.Room).GN(sym.ChapelRoom(true, roomBelowFloor), sym.FullFloorMarker))
                        .CurrentFirst(out var nextFloor)
                        .ReplaceNodes(reservation)

                        .ReserveUpward(2, sym.UpwardReservation)
                        .PlaceCurrentFrom(nextFloor)

                        .FindPath(() => ldk.con.ConnectByStairsInside(nextFloor.LE, roomBelow.LE).GN(sym.ConnectionMarker), out var stairs)
                        .PlaceCurrentFrom(roomBelow, nextFloor)
                        );
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

                    return state.NewProgram(prog => prog
                        .Set(() => reservation.LE.SetAreaType(AreaType.Colonnade).GN(sym.ChapelTowerTop, sym.FullFloorMarker))
                        .CurrentFirst(out var towerTop)
                        .ReplaceNodes(reservation)

                        .Change(towerTop => towerTop.LE.CG().ExtrudeVer(Vector3Int.up, roofHeight).LE(AreaType.PointyRoof).GN(sym.Roof))
                        .PlaceCurrentFrom(towerTop)

                        .FindPath(() => ldk.con.ConnectByStairsInside(towerTop.LE, roomBelow.LE).GN(sym.ConnectionMarker), out var stairs)
                        .PlaceCurrentFrom(roomBelow, towerTop)
                        );
                });
        }*/

        #endregion

        #region Castle
        /*
        public Production TowerBottomNextTo(Symbol nextToWhat, Func<LevelElement> towerBottomF)
        {
            return FullFloorNextTo(nextToWhat, sym.Park, () => parkF().SetAreaType(AreaType.Garden),
                (program, _) => program,
                (program, _) => program,
                 _ => ldk.con.ConnectByDoor);
        }*/

        #endregion
    }
}

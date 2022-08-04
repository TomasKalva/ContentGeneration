using Assets.ShapeGrammarGenerator;
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

        #region Town

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

                                        .LE(AreaStyles.Yard())
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
                                    .LE(AreaStyles.Yard()).GN(sym.Courtyard, sym.FullFloorMarker)
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

        public Production BridgeFrom(Symbol from, PathGuide pathGuide)
        {
            return Extrude(
                    from,
                    node => pathGuide.SelectDirections(node.LE),
                    (cg, dir) => AlterInOrthogonalDirection(cg.ExtrudeDir(dir, 6), dir, 3).LE(AreaStyles.Colonnade()).GN(sym.Bridge(dir), sym.FullFloorMarker),
                    Empty(),
                    Empty(),
                    _ => ldk.con.ConnectByDoor
                );
        }

        public Production ExtendBridge()
        {
            return Extrude(
                    sym.Bridge(),
                    node => node.GetSymbol(sym.Bridge()).Direction.ToEnumerable(),
                    (cg, dir) => AlterInOrthogonalDirection(cg.ExtrudeDir(dir, 6), dir, 7).LE(AreaStyles.Yard()).GN(sym.Courtyard, sym.FullFloorMarker),
                    Empty(),
                    Empty(),
                    _ => ldk.con.ConnectByDoor
                );
        }

        public Production CourtyardFromBridge()
        {
            return Extrude(
                    sym.Bridge(),
                    node => node.GetSymbol(sym.Bridge()).Direction.ToEnumerable(),
                    (cg, dir) => cg.ExtrudeDir(dir, 5).LE(AreaStyles.Colonnade()).GN(sym.Bridge(dir), sym.FullFloorMarker),
                    Empty(),
                    Empty(),
                    _ => ldk.con.ConnectByDoor
                );
        }

        public Production RoomNextTo(Symbol nextToWhat, Func<LevelElement> roomF)
        {
            return FullFloorPlaceNear(
                nextToWhat,
                sym.Room,
                () => roomF().SetAreaType(AreaStyles.Room()),
                (program, _) => program,
                (program, newRoom) => program
                        .Set(() => newRoom)
                        .ReserveUpward(2, sym.UpwardReservation)
                        .PlaceCurrentFrom(newRoom),
                _ => ldk.con.ConnectByDoor,
                1
                );
        }

        public Production ExtendBridgeToRoom(Symbol from, Func<LevelElement> leF, PathGuide pathGuide)
            => ExtendBridgeTo(
                from,
                sym.Room,
                10,
                () => leF().SetAreaType(AreaStyles.Room()),
                pathGuide,
                Reserve(2, sym.UpwardReservation));

        public Production ExtendBridgeToGarden(Symbol from, Func<LevelElement> leF, PathGuide pathGuide)
            => ExtendBridgeTo(
                from,
                sym.Garden,
                10,
                () => leF().SetAreaType(AreaStyles.Garden()),
                pathGuide,
                Empty());



        public Production GardenFrom(Symbol from, Func<LevelElement> leF)
        {
            return FullFloorPlaceNear(
                from,
                sym.Garden,
                leF,
                (prog, node) => MoveVertically(-2, 3)(prog, node)
                    .Change(node =>
                        ldk.sgShapes.IslandExtrudeIter(node.LE.CG().BottomLayer(), 2, 0.7f)
                            .LE(AreaStyles.Garden()).Minus(prog.State.WorldState.Added)
                            //To remove disconnected we have to make sure that the or
                            .MapGeom(cg => cg
                                .SplitToConnected().First(cg => cg.Intersects(node.LE.CG()))//node..ArgMax(cg => cg.Cubes.Count)
                                .OpAdd().ExtrudeVer(Vector3Int.up, 3))

                            .GN(sym.Garden, sym.FullFloorMarker)
                        ),
                Empty(),
                ldk.con.ConnectByBalconyStairsOutside,
                1
                );
        }

        public Production TerraceFrom(Symbol from)
            => Extrude(
                from,
                // accept only nodes that have height at least 3
                node => node.LE.CG().ExtentsDir(Vector3Int.up) >= 3 ? 
                    ExtensionMethods.HorizontalDirections() : 
                    new Vector3Int[0],
                (cg, dir) 
                    => AlterInOrthogonalDirection(cg.ExtrudeDir(dir, 2).BottomLayer()
                            .OpAdd().ExtrudeDir(Vector3Int.up).OpNew(), dir, 3)
                                .LE(AreaStyles.Colonnade()).GN(sym.Terrace(dir), sym.FullFloorMarker),
                Empty(),
                Reserve(2, sym.UpwardReservation),
                _ => ldk.con.ConnectByDoor
                );

        #endregion

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
                            newRoom.LE.CG().ExtrudeVer(Vector3Int.down, 2).LE(AreaStyles.Room()).GN(sym.Room, sym.FullFloorMarker),
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
        /*
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
                        .LE(AreaStyles.Room())
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
                            .Split(dir, AreaStyles.None(), 1)
                            .ReplaceLeafsGrp(1,
                                large => large.Split(-dir, AreaStyles.None(), 1))
                            .ReplaceLeafsGrp(2,
                                middle => middle.SetAreaType(AreaStyles.Colonnade()));
                        var les = startMiddleEnd.Leafs().ToList();
                        return (les[0], les[2], les[1]);
                    }
                    var (start, middle, end) = startMiddleEnd();

                    var middleGn = middle.SetAreaType(AreaStyles.NoFloor()).GN();
                    var path = floorConnector(start, middle, end).SetAreaType(AreaStyles.Platform());
                    //ldk.con.ConnectByStairsInside(start, end, newHouse.LevelElement);

                    var door = ldk.con.ConnectByDoor(room.LE, start).GN();

                    //todo: test if the reservation is ok
                    var reservation = newHouse.LE.CG().ExtrudeVer(Vector3Int.up, 2).LE(AreaStyles.Reservation()).GN(sym.UpwardReservation(newHouse));
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
                });
        }*/

        public Production Roof()
        {
            return new Production(
                "AddRoof",
                new ProdParamsManager()
                    .AddNodeSymbols(sym.UpwardReservation(null)),
                (state, pp) =>
                {
                    var roomReservation = pp.Param;
                    var roof = roomReservation.LE.SetAreaType(AreaStyles.GableRoof()).GN(sym.Roof);

                    // and modify the dag
                    return state.NewProgramBadMethodDestroyItASAP(new[]
                    {
                        state.Replace(roomReservation).SetTo(roof),
                    });
                });
        }


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

        public Func<ProductionProgram, Node, ProductionProgram> Roof(AreaStyle roofType, int roofHeight)
        {
            return (program, towerTop) => program
                        .Set(() => towerTop)
                        .Change(towerTop => towerTop.LE.CG().ExtrudeVer(Vector3Int.up, roofHeight).LE(roofType).GN(sym.Roof))
                        .NotTaken()
                        .PlaceCurrentFrom(towerTop);
        }

        public CubeGroup AlterInOrthogonalDirection(CubeGroup cg, Vector3Int dir, int newWidth)
        {
            var orthDir = dir.OrthogonalHorizontalDirs().First();
            var width = cg.ExtentsDir(orthDir);
            var shrinkL = (int)Mathf.Floor((width - newWidth) / 2f);
            var shrinkR = (int)Mathf.Ceil((width - newWidth) / 2f);

            return newWidth < width ?
                cg.OpSub()
                    .ExtrudeDir(orthDir, -shrinkL)
                    .ExtrudeDir(-orthDir, -shrinkR)
                .OpNew()
                :
                cg.OpAdd()
                    .ExtrudeDir(orthDir, -shrinkL)
                    .ExtrudeDir(-orthDir, -shrinkR)
                .OpNew()
                ;
        }
        /*
        public CubeGroup ExtendInOrthogonalDirection(CubeGroup cg, Vector3Int dir, int newWidth)
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
        }*/

        LevelElement AllBlocking(ShapeGrammarState state, ProductionProgram prog, Grid<Cube> grid)
            => state.WorldState.Added.Merge(prog.AppliedOperations.SelectMany(op => op.To.Select(n => n.LE)).ToLevelGroupElement(grid));

        Node FindFoundation(Node node)
        {
            return node.Derived.Where(child => child.HasSymbols(sym.Foundation)).FirstOrDefault();
        }

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
                                .TryMove(1)
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
            Func<CubeGroup, Vector3Int, Node> extrudeNodeFromDirection,
            Func<ProductionProgram, Node, ProductionProgram> afterNodeCreated,
            Func<ProductionProgram, Node, ProductionProgram> fromFloorNodeAfterPlaced,
            ConnectionNotIntersecting connectionNotIntersecting,
            bool placeFoundation = true)
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
                                        extrudeNodeFromDirection(from.LE.CG(), dir)
                                )
                                .NotTaken()
                                .CurrentFirst(out var created)

                                .RunIf(true, p => afterNodeCreated(p, created))
                                .CanBeFounded()
                                ),
                            out var newChapelHall
                            )
                        .PlaceCurrentFrom(from)

                        .RunIf(placeFoundation, newProg => newProg
                            .Found()
                            .PlaceCurrentFrom(newChapelHall)
                        )

                        .RunIf(true, newProg =>
                            fromFloorNodeAfterPlaced(newProg, newChapelHall))

                        .FindPath(() =>
                        connectionNotIntersecting(AllBlocking(state, prog, from.LE.CG().Grid))
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

        public Production ExtendBridgeTo(
            Symbol from,
            Symbol to,
            int distance,
            Func<LevelElement> toF,
            PathGuide pathGuide,
            Func<ProductionProgram, Node, ProductionProgram> fromFloorNodeAfterPlaced
            )
        {
            return new Production(
                $"ExtendBridgeFromTo_{from.Name}_{to.Name}",
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
                                    dir => whatCG.CubeGroupMaxLayer(Vector3Int.down).ExtrudeDir(dir, distance, false).LE().GN()
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
                                    newRoomDown => newRoomDown.LE.MoveBottomTo(whatCG.LeftBottomBack().y).GN(to, sym.FullFloorMarker)
                                )
                                ),
                                out var newNode
                        )
                        .PlaceCurrentFrom(what)

                        .Found()
                        .PlaceCurrentFrom(newNode)

                        .RunIf(true,
                            thisProg => fromFloorNodeAfterPlaced(thisProg, newNode)
                        )

                        .FindPath(() => ldk.con.ConnectByBridge(state.WorldState.Added)(what.LE, newNode.LE).GN(sym.ConnectionMarker), out var bridge)
                        .PlaceCurrentFrom(what, newNode)
                        );

                });
        }
        /*
        public Production LayerAroundFrom(
            Symbol from,
            Symbol to,
            int layerSize,
            Func<Node, LevelElement> layerToTo,
            Func<ProductionProgram, Node, ProductionProgram> fromFloorNodeAfterPlaced,
            ConnectionNotIntersecting connection
            )
        {
            return new Production(
                $"ExtendBridgeFromTo_{from.Name}_{to.Name}",
                new ProdParamsManager().AddNodeSymbols(from),
                (state, pp) =>
                {
                    var what = pp.Param;
                    var whatCG = what.LE.CG();

                    // reduced from 1450 to 1050 characters, from 80 lines to 34 lines
                    return state.NewProgram(prog => prog
                        .SelectOne(
                            
                            state.NewProgram(subProg => subProg
                                .Directional(ExtensionMethods.HorizontalDirections(),
                                    dir => what.LE.CG().BottomLayer().ExtrudeDir(dir, layerSize).LE().GN()
                                )
                                .DontIntersectAdded()
                                .Change(node => layerToTo(node).Minus(state.WorldState.Added).GN(to))
                                ),
                                out var newNode
                        )
                        .PlaceCurrentFrom(what)

                        .Found()
                        .PlaceCurrentFrom(newNode)

                        .RunIf(true,
                            thisProg => fromFloorNodeAfterPlaced(thisProg, newNode)
                        )

                        .FindPath(() => connection(AllBlocking(state, prog, what.LE.Grid))
                            (what.LE, newNode.LE).GN(sym.ConnectionMarker), out var stairs)
                        .PlaceCurrentFrom(what, newNode)
                        );

                });
        }*/

        #endregion

        public Production RoomDown(Symbol from, Symbol to, int belowRoomHeight, int minFloorHeight)
        {
            return FromDownwardFoundation(
                    from,
                    cg => cg.LE(AreaStyles.Room()).GN(sym.FullFloorMarker, to),
                    belowRoomHeight,
                    minFloorHeight,
                    _ => ldk.con.ConnectByWallStairsIn
                );
        }

        public Production RoomNextFloor(Symbol from, Symbol to, int nextFloorHeight, int maxFloorHeight)
        {
            return TakeUpwardReservation(
                    from,
                    nextFloor => nextFloor.LE(AreaStyles.Room()).GN(to, sym.FullFloorMarker),
                    nextFloorHeight,
                    maxFloorHeight,
                    Reserve(2, sym.UpwardReservation),
                    _ => ldk.con.ConnectByWallStairsIn);
        }

        #region Graveyard

        public Production ChapelHall(Symbol extrudeFrom, int length, PathGuide pathGuide)
        {
            return Extrude(
                extrudeFrom,
                node => pathGuide.SelectDirections(node.LE),
                (cg, dir) => cg.ExtrudeDir(dir, length).LE(AreaStyles.Room()).GN(sym.ChapelHall(dir), sym.FullFloorMarker),
                Empty(),
                Reserve(2, sym.UpwardReservation),
                _ => ldk.con.ConnectByDoor
                );
        }

        public Production ChapelRoom(int extrusionLength)
        {
            return Extrude(
                sym.ChapelHall(default),
                node => node.GetSymbol(sym.ChapelHall(default)).Direction.ToEnumerable(),
                (cg, dir) => cg.ExtrudeDir(dir, extrusionLength).LE(AreaStyles.Room()).GN(sym.ChapelRoom, sym.FullFloorMarker),
                Empty(),
                Reserve(2, sym.UpwardReservation),
                _ => ldk.con.ConnectByDoor
                );
        }

        public Production ChapelNextFloor(int nextFloorHeight, int maxFloor)
            => RoomNextFloor(sym.ChapelRoom, sym.ChapelRoom, nextFloorHeight, maxFloor);


        public Production ChapelTowerTop(int towerTopHeight, int roofHeight, int maxHeight = 100)
        {
            return TakeUpwardReservation(
                    sym.ChapelRoom,
                    nextFloor => nextFloor.LE(AreaStyles.Colonnade()).GN(sym.ChapelTowerTop, sym.FullFloorMarker),
                    towerTopHeight,
                    maxHeight,
                    Roof(AreaStyles.PointyRoof(), roofHeight),
                    _ => ldk.con.ConnectByWallStairsIn);
        }

        public Production ChapelEntranceNextTo(Symbol nextToWhatSymbol, int roofHeight, Func<LevelElement> leF)
            => FullFloorPlaceNear(
                    nextToWhatSymbol,
                    sym.ChapelEntrance,
                    () => leF().SetAreaType(AreaStyles.Room()),
                    Empty(),
                    Roof(AreaStyles.CrossRoof(), roofHeight),
                    ldk.con.ConnectByBalconyStairsOutside,
                    1);

        public Production ParkNear(Symbol nearWhatSym, int heighChange, int minHeight, Func<LevelElement> leF)
            => FullFloorPlaceNear(
                    nearWhatSym,
                    sym.Park,
                    () => leF().SetAreaType(AreaStyles.Garden()),
                    MoveVertically(heighChange, minHeight),
                    Empty(),
                    ldk.con.ConnectByBalconyStairsOutside,
                    1);

        public Production ChapelSides(int width)
            => Extrude(
                sym.ChapelHall(default),
                node => node.GetSymbol(sym.ChapelHall(default)).Direction.ToEnumerable(),
                (cg, dir) =>
                {
                    var orthDir = ExtensionMethods.OrthogonalHorizontalDir(dir);
                    return cg.ExtrudeDir(orthDir, width)
                                .OpSub().ExtrudeDir(Vector3Int.up, -1).OpNew()
                                .LE(AreaStyles.Room()).GN(sym.ChapelSide(orthDir), sym.FullFloorMarker);
                },
                Empty(),
                Reserve(2, sym.UpwardReservation),
                _ => ldk.con.ConnectByDoor
                );

        public Production BalconyFrom(Symbol from)
            => Extrude(
                from,
                node => ExtensionMethods.HorizontalDirections(),
                (cg, dir) => cg.ExtrudeDir(dir, 1).LE(AreaStyles.FlatRoof()).GN(sym.Balcony(dir), sym.FullFloorMarker),
                Empty(),
                Empty(),
                _ => ldk.con.ConnectByDoor,
                false
                );
        #endregion

        #region Castle

        public Production TowerBottomNear(Symbol nearWhatSym, Func<LevelElement> towerBottomF)
            => FullFloorPlaceNear(
                    nearWhatSym,
                    sym.TowerBottom,
                    () => towerBottomF().SetAreaType(AreaStyles.Room()),
                    Empty(),
                    Reserve(2, sym.UpwardReservation),
                    ldk.con.ConnectByBalconyStairsOutside,
                    3);

        public Production UpwardTowerTop(int nextFloorHeight)
            => RoomNextFloor(sym.TowerBottom, sym.TowerTop, nextFloorHeight, 100);

        public Production WallTop(Symbol extrudeFrom, int length, int width, PathGuide pathGuide)
            => Extrude(
                extrudeFrom,
                node => pathGuide.SelectDirections(node.LE),
                (cg, dir) => AlterInOrthogonalDirection(cg.ExtrudeDir(dir, length), dir, 2).LE(AreaStyles.FlatRoof()).GN(sym.WallTop(dir), sym.FullFloorMarker),
                Empty(),
                Empty(),
                _ => ldk.con.ConnectByDoor
                );

        public Production TowerTopFromWallTop(int length, int width)
            => Extrude(
                sym.WallTop(default),
                node => node.GetSymbol(sym.WallTop(default)).Direction.ToEnumerable(),
                (cg, dir) => AlterInOrthogonalDirection(cg.ExtrudeDir(dir, length), dir, width).LE(AreaStyles.Room()).GN(sym.TowerTop, sym.FullFloorMarker),
                Empty(),
                Reserve(2, sym.UpwardReservation),
                _ => ldk.con.ConnectByDoor
                );

        public Production TowerTopNear(Symbol nearWhatSym, int distance, int heightChange, int minBottomHeight, Func<LevelElement> towerTopF)
            => FullFloorPlaceNear(
                    nearWhatSym,
                    sym.TowerTop,
                    () => towerTopF().SetAreaType(AreaStyles.Room()),
                    MoveVertically(heightChange, minBottomHeight),
                    Reserve(2, sym.UpwardReservation),
                    ldk.con.ConnectByBalconyStairsOutside,
                    distance
                );

        public Production GardenFrom(Symbol from)
            => Extrude(
                    from,
                    _ => ExtensionMethods.HorizontalDirections(),
                    (cg, dir) => cg.ExtrudeDir(dir, 1).LE(AreaStyles.Garden()).GN(sym.Garden, sym.FullFloorMarker),
                    (prog, node) => 
                        prog.Set(() => ldk.sgShapes
                            .IslandExtrudeIter(node.LE.CG().BottomLayer(), 3, 0.7f)
                            .LE(AreaStyles.Garden())
                            .Minus(prog.State.WorldState.Added)
                            .MapGeom(cg => cg
                                .SplitToConnected().First(cg => cg.Intersects(node.LE.CG()))
                            )
                            .GN(sym.Garden, sym.FullFloorMarker)
                        )
                        .NotTaken()
                        .CanBeFounded(),
                    Empty(),
                    ldk.con.ConnectByBalconyStairsOutside
                    );

        /// <summary>
        /// Todo: make the pathfinding work correctly.
        /// </summary>
        public Production SideWall(int width)
            => Extrude(
                sym.ChapelHall(default),
                node => node.GetSymbol(sym.ChapelHall(default)).Direction.ToEnumerable(),
                (cg, dir) =>
                {
                    var orthDir = ExtensionMethods.OrthogonalHorizontalDir(dir);
                    return cg.ExtrudeDir(orthDir, width)
                                .LE(AreaStyles.FlatRoof()).GN(sym.SideWallTop(orthDir), sym.FullFloorMarker);
                },
                Empty(),//MoveVertically(0, 5),
                Empty(),
                _ => ldk.con.ConnectByDoor
                );

        public Production WatchPostNear(Symbol nearWhatSym, int distance, int heightChange, int minBottomHeight, Func<LevelElement> towerTopF)
            => FullFloorPlaceNear(
                    nearWhatSym,
                    sym.WatchPost,
                    () => towerTopF().SetAreaType(AreaStyles.FlatRoof()),
                    MoveVertically(heightChange, minBottomHeight),
                    Empty(),
                    ldk.con.ConnectByBalconyStairsOutside,
                    distance
                );

        #endregion
    }
}

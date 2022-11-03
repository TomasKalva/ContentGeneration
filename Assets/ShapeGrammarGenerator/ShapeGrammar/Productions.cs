using OurFramework.Environment.GridMembers;
using OurFramework.Environment.ShapeCreation;
using OurFramework.Environment.ShapeGrammar;
using OurFramework.Environment.StylingAreas;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static OurFramework.Environment.ShapeCreation.Connections;

namespace OurFramework.Environment.ShapeGrammar
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
            return Place(sym.Room, 3, () => ldk.les.Room(5, 5, 2), Reserve(2, sym.UpwardReservation));
        }

        public Production CourtyardFromRoom(PathGuide pathGuide)
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
                        .SelectFirstOne(
                            state.NewProgram(subProg => subProg
                                .Directional(pathGuide.SelectDirections(room.LE),
                                    dir =>
                                        roomCubeGroup
                                        .CubeGroupMaxLayer(dir)
                                        .OpAdd()
                                            .ExtrudeHor().ExtrudeHor().Minus(roomCubeGroup)
                                        .OpNew()

                                        .LE(AreaStyles.Yard(AreaStyles.YardStyle))
                                        .GN(sym.Courtyard, sym.FullFloorMarker)
                                )
                                .DiscardTaken()
                                .CanBeFounded()
                                ),
                            out var courtyard
                        )
                        .PlaceCurrentFrom(room)

                        .Found()
                        .PlaceCurrentFrom(courtyard)

                        .FindPath(() => 
                        ldk.con.ConnectByDoor(AllAlreadyExisting(state, prog), AllExistingPaths(state, prog))
                            (room.LE, courtyard.LE).GN(sym.ConnectionMarker), out var door)
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
                        .SelectRandomOne(
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
                                    .LE(AreaStyles.Yard(AreaStyles.YardStyle)).GN(sym.Courtyard, sym.FullFloorMarker)
                                )
                                .DiscardTaken()
                                .CanBeFounded()
                                ),
                            out var newCourtyard
                        )
                        .PlaceCurrentFrom(courtyard)

                        .Found()
                        .PlaceCurrentFrom(newCourtyard)

                        .FindPath(() => 
                        ldk.con.ConnectByStairsInside(AllAlreadyExisting(state, prog), AllExistingPaths(state, prog))
                            (courtyard.LE, newCourtyard.LE).GN(sym.ConnectionMarker), out var p)
                        .PlaceCurrentFrom(courtyard, newCourtyard)
                        );
                });
        }

        public Production BridgeFrom(Symbol from, PathGuide pathGuide)
        {
            return Extrude(
                    from,
                    node => pathGuide.SelectDirections(node.LE),
                    (cg, dir) => AlterInOrthogonalDirection(cg.ExtrudeDir(dir, 6), dir, 3).LE(AreaStyles.BridgeTop(dir)).GN(sym.Bridge(dir), sym.FullFloorMarker),
                    Empty(),
                    (program, bridgeTop) => BridgeFoundation(bridgeTop.GetSymbol(sym.Bridge(default)).Direction)(program, bridgeTop),
                    ldk.con.ConnectByDoor,
                    false
                );
        }

        public Production ExtendBridge()
        {
            return Extrude(
                    sym.Bridge(),
                    node => node.GetSymbol(sym.Bridge()).Direction.ToEnumerable(),
                    (cg, dir) => cg.ExtrudeDir(dir, 5).LE(AreaStyles.BridgeTop(dir)).GN(sym.Bridge(dir), sym.FullFloorMarker),
                    Empty(),
                    (program, bridgeTop) => BridgeFoundation(bridgeTop.GetSymbol(sym.Bridge(default)).Direction)(program, bridgeTop),
                    ldk.con.ConnectByDoor,
                    false
                );
        }

        public Production CourtyardFromBridge()
        {
            return Extrude(
                    sym.Bridge(),
                    node => node.GetSymbol(sym.Bridge()).Direction.ToEnumerable(),
                    (cg, dir) => AlterInOrthogonalDirection(cg.ExtrudeDir(dir, 6), dir, 7).LE(AreaStyles.Yard(AreaStyles.YardStyle)).GN(sym.Courtyard, sym.FullFloorMarker),
                    Empty(),
                    Empty(),
                    ldk.con.ConnectByDoor
                );
        }

        public Production RoomNextTo(Symbol nextToWhat, Func<LevelElement> roomF)
        {
            return FullFloorPlaceNear(
                nextToWhat,
                sym.Room,
                () => roomF().SetAreaStyle(AreaStyles.Room()),
                (program, _) => program,
                (program, newRoom) => program
                        .Set(() => newRoom)
                        .ReserveUpward(2, sym.UpwardReservation)
                        .PlaceCurrentFrom(newRoom),
                ldk.con.ConnectByDoor,
                1
                );
        }

        public Production ExtendBridgeToRoom(Symbol from, Symbol roomSymbol, Func<LevelElement> leF, PathGuide pathGuide)
            => ExtendBridgeTo(
                from,
                roomSymbol,
                5,
                () => leF().SetAreaStyle(AreaStyles.Room()),
                pathGuide,
                Reserve(2, sym.UpwardReservation));

        public Production ExtendBridgeToGarden(Symbol from, Symbol gardenSymbol, Func<LevelElement> leF, PathGuide pathGuide)
            => ExtendBridgeTo(
                from,
                gardenSymbol,
                5,
                () => leF().SetAreaStyle(AreaStyles.Garden()),
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
                        ldk.cgs.IslandExtrudeIter(node.LE.CG().BottomLayer(), 2, 0.7f)
                            .LE(AreaStyles.Garden()).Minus(prog.State.WorldState.Added)
                            //To remove disconnected we have to make sure that the or
                            .MapGeom(cg => cg
                                .SplitToConnected().First(cg => cg.Intersects(node.LE.CG()))//node..ArgMax(cg => cg.Cubes.Count)
                                .OpAdd().ExtrudeVer(Vector3Int.up, 3))

                            .GN(sym.Garden, sym.FullFloorMarker)
                        ),
                Empty(),
                ldk.con.ConnectByStairsOutside,
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
                (program, terrace) => program
                        .Set(() => terrace.LE.CG().ExtrudeVer(Vector3Int.up, 1).LE(AreaStyles.DirectionalRoof(terrace.GetSymbol(sym.Terrace(default)).Direction)).GN(sym.Roof))
                        .DiscardTaken()
                        .PlaceCurrentFrom(terrace),
                ldk.con.ConnectByDoor
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
                        .SelectFirstOne(
                            state.NewProgram(subProg => subProg
                                .Set(() => roomFromToF().GN())
                                .MoveNearTo(what, 1)
                                .Change(node => node.LE.GN(sym.BrokenFloorRoom))
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

                        .FindPath(() => 
                        ldk.con.ConnectByDoor(AllAlreadyExisting(state, prog), AllExistingPaths(state, prog))
                            (newRoom.LE, what.LE).GN(sym.ConnectionMarker), out var door)
                        .PlaceCurrentFrom(newRoom, what)

                        // The door doesn't get overwritten by apply style only because it has higher priority, which doesn't feel robust enough
                        .FindPath(() => 
                        ldk.con.ConnectByFall(AllAlreadyExisting(state, prog), AllExistingPaths(state, prog))
                            (newRoom.LE, bottomRoom.LE).GN(), out var fall)
                        .PlaceCurrentFrom(bottomRoom, newRoom)
                        );

                });
        }

        public Production ConnectByRoom(
            Symbol from, 
            Symbol to, 
            Func<LevelElement> roomFromF,
            Func<ProductionProgram, Node, ProductionProgram> fromFloorNodeAfterPositionedNear,
            ConnectionFromAddedAndPaths connectFrom,
            ConnectionFromAddedAndPaths connectTo,
            int distance,
            Func<Node, Node, bool> fromToCondition)
        {
            return new Production(
                $"ConnectByRoom",
                new ProdParamsManager()
                    .AddNodeSymbols(from, sym.FullFloorMarker)
                    .AddNodeSymbols(to, sym.FullFloorMarker)
                    .SetCondition((state, pp) =>
                    {
                        var (from, to) = pp;
                        return fromToCondition(from, to);
                    }),
                (state, pp) =>
                {
                    var (from, to) = pp;
                    var fromCG = from.LE.CG();
                    var toCG = to.LE.CG();

                    return state.NewProgram(prog => prog
                        .SelectRandomOne(
                            state.NewProgram(subProg => subProg
                                .Set(() => roomFromF().GN())
                                .MoveNearTo(to, distance)
                                .CurrentFirst(out var newArea)
                                .RunIf(true,
                                    thisProg => fromFloorNodeAfterPositionedNear(thisProg, newArea)
                                )
                                .Change(node => node.LE.GN(sym.Room, sym.FullFloorMarker))
                                ),
                            out var newRoom
                        )
                        .PlaceCurrentFrom(from)

                        .Found(out var foundation)
                        .PlaceCurrentFrom(newRoom)

                        .Set(() => newRoom)
                        .ReserveUpward(2, sym.UpwardReservation, out var reservation)
                        .PlaceCurrentFrom(newRoom)

                        .FindPath(() =>
                        connectFrom(AllAlreadyExisting(state, prog), AllExistingPaths(state, prog))
                            (from.LE, newRoom.LE).GN(sym.ConnectionMarker), out var stairs)
                        .PlaceCurrentFrom(from, newRoom)

                        .FindPath(() =>
                        connectTo(AllAlreadyExisting(state, prog), AllExistingPaths(state, prog))
                            (newRoom.LE, to.LE).GN(), out var fall)
                        .PlaceCurrentFrom(to, newRoom)
                        );
                });
        }

        public Production Roof(Symbol reservationSymbol, int roofHeight, AreaStyle roofAreaStyle)
        {
            return TakeUpwardReservation(
                reservationSymbol,
                cg => cg.LE(roofAreaStyle).GN(sym.Roof),
                roofHeight,
                int.MaxValue,
                Empty(),
                ldk.con.NoConnection);
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
                                .LE.MoveBottomBy(heightChange, minHeight).GN(node.GetSymbols.ToArray()));
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
            return (program, roof) => program
                        .Set(() => roof)
                        .Change(towerTop => towerTop.LE.CG().ExtrudeVer(Vector3Int.up, roofHeight).LE(roofType).GN(sym.Roof))
                        .DiscardTaken()
                        .PlaceCurrentFrom(roof);
        }

        public Func<ProductionProgram, Node, ProductionProgram> BridgeFoundation(Vector3Int bridgeDirection)
        {
            return (program, bridgeTop) => program
                        .Set(() => ldk.les.BridgeFoundation(bridgeTop.LE, bridgeDirection).GN(sym.Foundation))
                        .DiscardTaken()
                        .PlaceCurrentFrom(bridgeTop);
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

        LevelElement AllAlreadyExisting(ShapeGrammarState state, ProductionProgram prog)
            => state.WorldState.Added.Merge(prog.AppliedOperations.SelectMany(op => op.To.Select(n => n.LE)).ToLevelGroupElement(ldk.grid));

        LevelGroupElement AllExistingPaths(ShapeGrammarState state, ProductionProgram prog)
        {
            var alreadyDerivedNodes = state.Root.AllDerived();
            var programNodes = prog.AppliedOperations.SelectMany(op => op.To);
            var allNodes = alreadyDerivedNodes.Concat(programNodes);
            return allNodes.Where(node => node.HasSymbols(sym.ConnectionMarker)).Select(node => node.LE).ToLevelGroupElement(ldk.grid);
        }

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
                                .TryMove(1)?
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
            ConnectionFromAddedAndPaths connectionNotIntersecting,
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
                        .Set(() => newAreaF().GN())
                        .MoveNearTo(what, dist)
                        .CurrentFirst(out var newArea)
                        .Run(thisProg => fromFloorNodeAfterPositionedNear(thisProg, newArea))
                        .Change(node => node.LE.GN(newAreaSym, sym.FullFloorMarker))
                        .CurrentFirst(out var newNode)
                        .PlaceCurrentFrom(what)

                        .Found(out var foundation)
                        .PlaceCurrentFrom(newNode)

                        .RunIf(true,
                            thisProg => fromFloorNodeAfterPlaced(thisProg, newNode)
                        )

                        //Replace with open connection
                        .FindPath(() =>
                            connectionNotIntersecting
                                (AllAlreadyExisting(state, prog), AllExistingPaths(state, prog))
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
            ConnectionFromAddedAndPaths connectionNotIntersecting,
            bool placeFoundation = true)
        {
            return new Production(
                $"ExtrudeFrom_{extrudeFrom.Name}",
                new ProdParamsManager().AddNodeSymbols(extrudeFrom),
                (state, pp) =>
                {
                    var from = pp.Param;
                    var fromCG = from.LE.CG();

                    return state.NewProgram(prog => prog
                        .SelectFirstOne(
                            state.NewProgram(subProg => subProg
                                .Directional(directionsFromSelected(from),
                                    dir =>
                                        extrudeNodeFromDirection(from.LE.CG(), dir)
                                )
                                .DiscardTaken()
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
                        connectionNotIntersecting(AllAlreadyExisting(state, prog), AllExistingPaths(state, prog))
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
            ConnectionFromAddedAndPaths connection)
        {
            return new Production(
                $"TakeUpwardReservation_{reservationSymbol.Name}",
                new ProdParamsManager()
                    .AddNodeSymbols(sym.UpwardReservation(null))
                    .SetCondition((state, pp) =>
                    {
                        bool correctBelowSymbol =
                            pp.Param.GetSymbol(sym.UpwardReservation(null))
                            .NodeReference.GetSymbol(reservationSymbol) != null;
                        return correctBelowSymbol && pp.Param.LE.CG().RightTopFront().y + 1 <= maxBottomHeight;
                    }),
                (state, pp) =>
                {
                    var reservation = pp.Param;
                    var reservationCG = reservation.LE.CG();
                    var toExtrude = nextFloorHeight - 1;
                    var roomBelow = pp.Param.GetSymbol<ReferenceSymbol>(sym.UpwardReservation(null)).NodeReference;

                    return state.NewProgram(prog => prog
                        .Condition(() => toExtrude >= 0)
                        .Set(() => reservation)
                        .Change(res => res.LE.CG().BottomLayer()
                            .OpAdd().ExtrudeDir(Vector3Int.up, toExtrude).OpNew().LE().GN())
                        .CurrentFirst(out var extendedReservation)

                        // Only check if the part outside of the reservation was not taken yet
                        .Condition(() => extendedReservation.LE.Minus(reservation.LE).CG().AllAreNotTaken())

                        .Change(extr => nodeFromExtrudedUp(extr.LE.CG()))
                        .CurrentFirst(out var nextFloor)
                        .ReplaceNodes(reservation)

                        .RunIf(true, prog => fromFloorNodeAfterPlaced(prog, nextFloor))

                        .FindPath(() => connection(AllAlreadyExisting(state, prog), AllExistingPaths(state, prog))
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
            ConnectionFromAddedAndPaths connection)
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
                        .DiscardTaken()
                        .Set(() => extendedDown)

                        .Change(extr => nodeFromExtrudedDown(extr.LE.CG()))
                        .CurrentFirst(out var downFloor)
                        .ReplaceNodes(foundationBelow)

                        .Found()
                        .PlaceCurrentFrom(downFloor)

                        .FindPath(() => connection(AllAlreadyExisting(state, prog), AllExistingPaths(state, prog))
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

                    return state.NewProgram(prog => prog
                        .SelectFirstOne(
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

                        .FindPath(() => 
                        ldk.con.ConnectByBridge(AllAlreadyExisting(state, prog), AllExistingPaths(state, prog))
                            (what.LE, newNode.LE).GN(sym.ConnectionMarker), out var bridge)
                        .PlaceCurrentFrom(what, newNode)
                        );

                });
        }

        #endregion

        public Production RoomDown(Symbol from, Symbol to, AreaStyle roomStyle, int belowRoomHeight, int minFloorHeight)
        {
            return FromDownwardFoundation(
                    from,
                    cg => cg.LE(roomStyle).GN(sym.FullFloorMarker, to),
                    belowRoomHeight,
                    minFloorHeight,
                    ldk.con.ConnectByWallStairsIn
                );
        }

        public Production RoomNextFloor(Symbol from, Symbol to, AreaStyle roomStyle, int nextFloorHeight, int maxFloorHeight, ConnectionFromAddedAndPaths connection)
        {
            return TakeUpwardReservation(
                    from,
                    nextFloor => nextFloor.LE(roomStyle).GN(to, sym.FullFloorMarker),
                    nextFloorHeight,
                    maxFloorHeight,
                    Reserve(2, sym.UpwardReservation),
                    connection
                    );
        }

        #region Graveyard

        public Production ChapelHall(Symbol extrudeFrom, int length, PathGuide pathGuide)
        {
            return Extrude(
                extrudeFrom,
                node => pathGuide.SelectDirections(node.LE),
                (cg, dir) => cg.ExtrudeDir(dir, length).LE(AreaStyles.Room(AreaStyles.ChapelStyle)).GN(sym.ChapelHall(dir), sym.FullFloorMarker),
                Empty(),
                Reserve(2, sym.UpwardReservation),
                ldk.con.ConnectByDoor
                );
        }

        public Production ChapelRoom(int extrusionLength)
        {
            return Extrude(
                sym.ChapelHall(default),
                node => node.GetSymbol(sym.ChapelHall(default)).Direction.ToEnumerable(),
                (cg, dir) => cg.ExtrudeDir(dir, extrusionLength).LE(AreaStyles.Room(AreaStyles.ChapelStyle)).GN(sym.ChapelRoom, sym.FullFloorMarker),
                Empty(),
                Reserve(2, sym.UpwardReservation),
                ldk.con.ConnectByDoor
                );
        }

        public Production ChapelNextFloor(int nextFloorHeight, int maxFloor)
            => TakeUpwardReservation(
                    sym.ChapelRoom,
                    nextFloor => nextFloor.LE(AreaStyles.Room(AreaStyles.ChapelStyle)).GN(sym.ChapelRoom, sym.FullFloorMarker),
                    nextFloorHeight,
                    maxFloor,
                    Reserve(2, sym.UpwardReservation),
                    ldk.con.ConnectByWallStairsIn
                    );


        public Production ChapelTowerTop(int towerTopHeight, int roofHeight, int maxHeight = 100)
        {
            return TakeUpwardReservation(
                    sym.ChapelRoom,
                    nextFloor => nextFloor.LE(AreaStyles.Colonnade(AreaStyles.ChapelStyle)).GN(sym.ChapelTowerTop, sym.FullFloorMarker),
                    towerTopHeight,
                    maxHeight,
                    Roof(AreaStyles.PointyRoof(AreaStyles.ChapelStyle), roofHeight),
                    ldk.con.ConnectByWallStairsIn);
        }

        public Production ChapelEntranceNextTo(Symbol nextToWhatSymbol, int roofHeight, Func<LevelElement> leF)
            => FullFloorPlaceNear(
                    nextToWhatSymbol,
                    sym.ChapelEntrance,
                    () => leF().SetAreaStyle(AreaStyles.Room(AreaStyles.ChapelStyle)),
                    Empty(),
                    Roof(AreaStyles.CrossRoof(AreaStyles.ChapelStyle), roofHeight),
                    ldk.con.ConnectByStairsOutside,
                    1);

        public Production ParkNear(Symbol nearWhatSym, int heighChange, int minHeight, Func<LevelElement> leF)
            => FullFloorPlaceNear(
                    nearWhatSym,
                    sym.Park,
                    () => leF().SetAreaStyle(AreaStyles.Garden()),
                    MoveVertically(heighChange, minHeight),
                    Empty(),
                    ldk.con.ConnectByStairsOutside,
                    1);

        public Production ChapelSide(int width)
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
                (program, chapelSide) => program
                        .Set(() => chapelSide.LE.CG().ExtrudeVer(Vector3Int.up, 1).LE(AreaStyles.DirectionalRoof(chapelSide.GetSymbol(sym.ChapelSide(default)).Direction, AreaStyles.ChapelStyle)).GN(sym.Roof))
                        .DiscardTaken()
                        .PlaceCurrentFrom(chapelSide),
                ldk.con.ConnectByDoor
                );

        public Production BalconyFrom(Symbol from)
            => Extrude(
                from,
                node => ExtensionMethods.HorizontalDirections(),
                (cg, dir) => cg.ExtrudeDir(dir, 1).LE(AreaStyles.FlatRoof()).GN(sym.Balcony(dir), sym.FullFloorMarker),
                Empty(),
                Empty(),
                ldk.con.ConnectByDoor,
                false
                );
        #endregion

        #region Castle

        public Production TowerBottomNear(Symbol nearWhatSym, Func<LevelElement> towerBottomF)
            => FullFloorPlaceNear(
                    nearWhatSym,
                    sym.TowerBottom,
                    () => towerBottomF().SetAreaStyle(AreaStyles.Room(AreaStyles.CastleStyle)),
                    Empty(),
                    Reserve(2, sym.UpwardReservation),
                    ldk.con.ConnectByStairsOutside,
                    3);

        public Production UpwardTowerTop(int nextFloorHeight)
            => RoomNextFloor(sym.TowerBottom, sym.TowerTop, AreaStyles.Room(AreaStyles.CastleStyle), nextFloorHeight, 100, ldk.con.ConnectByWallStairsIn);

        public Production WallTop(Symbol extrudeFrom, int length, int width, PathGuide pathGuide)
            => Extrude(
                extrudeFrom,
                node => pathGuide.SelectDirections(node.LE),
                (cg, dir) => AlterInOrthogonalDirection(cg.ExtrudeDir(dir, length), dir, 2).LE(AreaStyles.FlatRoof(AreaStyles.YardStyle)).GN(sym.WallTop(dir), sym.FullFloorMarker),
                Empty(),
                Empty(),
                ldk.con.ConnectByDoor
                );

        public Production TowerTopFromWallTop(int length, int width)
            => Extrude(
                sym.WallTop(default),
                node => node.GetSymbol(sym.WallTop(default)).Direction.ToEnumerable(),
                (cg, dir) => AlterInOrthogonalDirection(cg.ExtrudeDir(dir, length), dir, width).LE(AreaStyles.Room(AreaStyles.CastleStyle)).GN(sym.TowerTop, sym.FullFloorMarker),
                Empty(),
                Reserve(2, sym.UpwardReservation),
                ldk.con.ConnectByDoor
                );

        public Production TowerTopNear(Symbol nearWhatSym, int distance, int heightChange, int minBottomHeight, Func<LevelElement> towerTopF)
            => FullFloorPlaceNear(
                    nearWhatSym,
                    sym.TowerTop,
                    () => towerTopF().SetAreaStyle(AreaStyles.Room(AreaStyles.CastleStyle)),
                    MoveVertically(heightChange, minBottomHeight),
                    Reserve(2, sym.UpwardReservation),
                    ldk.con.ConnectByStairsOutside,
                    distance
                );

        public Production GardenFrom(Symbol from)
            => Extrude(
                    from,
                    _ => ExtensionMethods.HorizontalDirections(),
                    (cg, dir) => cg.ExtrudeDir(dir, 1).LE(AreaStyles.Garden()).GN(sym.Garden, sym.FullFloorMarker),
                    (prog, node) => 
                        prog.Set(() => ldk.cgs
                            .IslandExtrudeIter(node.LE.CG().BottomLayer(), 3, 0.7f)
                            .LE(AreaStyles.Garden())
                            .Minus(prog.State.WorldState.Added)
                            .MapGeom(cg => cg
                                .SplitToConnected().First(cg => cg.Intersects(node.LE.CG()))
                            )
                            .GN(sym.Garden, sym.FullFloorMarker)
                        )
                        .DiscardTaken()
                        .CanBeFounded(),
                    Empty(),
                    ldk.con.ConnectByDoor
                    );

        public Production SideWall(int width)
            => Extrude(
                sym.WallTop(default),
                node => node.GetSymbol(sym.WallTop(default)).Direction.ToEnumerable(),
                (cg, dir) =>
                {
                    var orthDir = ExtensionMethods.OrthogonalHorizontalDir(dir);
                    return cg.ExtrudeDir(orthDir, width)
                                .LE(AreaStyles.FlatRoof(AreaStyles.YardStyle)).GN(sym.SideWallTop(orthDir), sym.FullFloorMarker);
                },
                MoveVertically(-2, 5),
                Empty(),
                ldk.con.ConnectByStairsOutside
                );

        public Production WatchPostNear(Symbol nearWhatSym, int distance, int heightChange, int minBottomHeight, Func<LevelElement> towerTopF)
            => FullFloorPlaceNear(
                    nearWhatSym,
                    sym.WatchPost,
                    () => towerTopF().SetAreaStyle(AreaStyles.FlatRoof(AreaStyles.CastleStyle)),
                    MoveVertically(heightChange, minBottomHeight),
                    Empty(),
                    ldk.con.ConnectByStairsOutside,
                    distance
                );

        #endregion

        #region NewGrammar

        public Production NewStart()
        {
            return new Production(
                "Place New Start",
                new ProdParamsManager(),
                (state, _) =>
                {
                    return state.NewProgram(
                        prog => prog
                            .Set(() => ldk.les.Room(5, 5, 2)
                                .MoveBottomTo(4)
                                .GN(sym.NewRoom, sym.FullFloorMarker, sym.LevelStartMarker))
                            .CurrentFirst(out var first)
                            .PlaceCurrentFrom(state.Root)
                            
                            .Found()
                            .PlaceCurrentFrom(first)

                            .Set(() => first)
                            .ReserveUpward(2, sym.UpwardReservation)
                            .PlaceCurrentFrom(first)
                            );
                });
        }

        public Production NewRoomNear()
        {
            return new Production(
                "New Room Near",
                new ProdParamsManager().AddNodeSymbols(sym.NewRoom),
                (state, pp) =>
                {
                    var what = pp.Param;
                    var whatCG = what.LE.CG();

                    return state.NewProgram(prog => prog
                        .Set(() => ldk.les.Box(3, 3, 2).SetAreaStyle(AreaStyles.Room()).GN())
                        .MoveNearTo(what, 1)
                        .Change(node => node.LE.GN(sym.NewRoom, sym.FullFloorMarker))
                        .CurrentFirst(out var newNode)
                        .PlaceCurrentFrom(what)

                        .Found(out var foundation)
                        .PlaceCurrentFrom(newNode)

                        .Set(() => newNode)
                        .ReserveUpward(2, sym.UpwardReservation)
                        .PlaceCurrentFrom(newNode)

                        //Replace with open connection
                        .FindPath(() => ldk.con
                            .ConnectByDoor(AllAlreadyExisting(state, prog), AllExistingPaths(state, prog))
                                (newNode.LE, what.LE)
                                .GN(sym.ConnectionMarker), out var door)
                        .PlaceCurrentFrom(what, newNode)
                        );
                });
        }

        public Production ExtrudeNewCorridor()
        {
            return new Production(
                $"Extrude New Corridor",
                new ProdParamsManager().AddNodeSymbols(sym.NewRoom),
                (state, pp) =>
                {
                    var from = pp.Param;
                    var fromCG = from.LE.CG();

                    return state.NewProgram(prog => prog
                        .SelectFirstOne(
                            state.NewProgram(subProg => subProg
                                .Directional(ExtensionMethods.HorizontalDirections(),
                                    dir =>
                                        fromCG.ExtrudeDir(dir, 5).LE(AreaStyles.Room()).GN(sym.NewCorridor(dir), sym.FullFloorMarker)
                                )
                                .DiscardTaken()
                                .CanBeFounded()
                                ),
                            out var newNode
                            )
                        .PlaceCurrentFrom(from)

                        .Found()
                        .PlaceCurrentFrom(newNode)


                        .Set(() => newNode)
                        .ReserveUpward(2, sym.UpwardReservation)
                        .PlaceCurrentFrom(newNode)

                        .FindPath(() =>
                        ldk.con
                            .ConnectByDoor(AllAlreadyExisting(state, prog), AllExistingPaths(state, prog))
                            (newNode.LE, from.LE).GN(sym.ConnectionMarker), out var door)
                        .PlaceCurrentFrom(from, newNode)
                        );
                });
        }

        public Production NewRoomNextFloor(
            Symbol reservationSymbol,
            Func<CubeGroup, Node> nodeFromExtrudedUp,
            int nextFloorHeight,
            int maxBottomHeight,
            Func<ProductionProgram, Node, ProductionProgram> fromFloorNodeAfterPlaced,
            ConnectionFromAddedAndPaths connection)
        {
            return new Production(
                "New Room Next Floor",
                new ProdParamsManager()
                    .AddNodeSymbols(sym.UpwardReservation(null))
                    .SetCondition((state, pp) =>
                    {
                        bool correctBelowSymbol =
                            pp.Param.GetSymbol(sym.UpwardReservation(null))
                            .NodeReference.GetSymbol(sym.NewRoom) != null;
                        return correctBelowSymbol && pp.Param.LE.CG().RightTopFront().y + 1 <= 10;
                    }),
                (state, pp) =>
                {
                    var reservation = pp.Param;
                    var reservationCG = reservation.LE.CG();
                    var toExtrude = nextFloorHeight - 1;
                    var roomBelow = pp.Param.GetSymbol(sym.UpwardReservation(null)).NodeReference;

                    return state.NewProgram(prog => prog
                        .Condition(() => toExtrude >= 0)
                        .Set(() => reservation)
                        .Change(res => res.LE.CG().BottomLayer()
                            .OpAdd().ExtrudeDir(Vector3Int.up, toExtrude).OpNew().LE().GN())
                        .CurrentFirst(out var extendedReservation)

                        // Only check if the part outside of the reservation was not taken yet
                        .Condition(() => extendedReservation.LE.Minus(reservation.LE).CG().AllAreNotTaken())

                        .Change(extr => nodeFromExtrudedUp(extr.LE.CG()))
                        .CurrentFirst(out var nextFloor)
                        .ReplaceNodes(reservation)

                        .Set(() => extendedReservation)
                        .ReserveUpward(2, sym.UpwardReservation)
                        .PlaceCurrentFrom(extendedReservation)

                        .FindPath(() => ldk.con.ConnectByWallStairsIn(AllAlreadyExisting(state, prog), AllExistingPaths(state, prog))
                            (nextFloor.LE, roomBelow.LE).GN(sym.ConnectionMarker), out var stairs)
                        .PlaceCurrentFrom(roomBelow, nextFloor)
                        );
                });
        }

        #endregion
    }
}

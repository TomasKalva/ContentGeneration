using OurFramework.Environment.ShapeCreation;
using OurFramework.Environment.StylingAreas;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OurFramework.Environment.ShapeGrammar
{
    public class ProductionProgram
    {
        public static LevelDevelopmentKit Ldk { get; set; }
        public static Productions Pr { get; set; }


        public ShapeGrammarState State { get; }
        public bool Failed { get; private set; }
        public string FailMessage { get; private set; }

        /// <summary>
        /// The grammar currently operates on these nodes.
        /// 
        /// Not IEnumerable to prevent multiple evaluations.
        /// </summary>
        List<Node> CurrentNodes { get; set; }
        public List<Operation> AppliedOperations { get; set; }


        public ProductionProgram(ShapeGrammarState state)
        {
            AppliedOperations = new List<Operation>();
            this.State = state;
        }

        private ProductionProgram Bind(Action programChange)
        {
            if (!Failed)
            {
                programChange();
            }

            return this;
        }

        private ProductionProgram Bind<OutT>(out OutT t, Func<OutT> programChange)
        {
            t = default;
            if (!Failed)
            {
                t = programChange();
            }

            return this;
        }

        T SetFailedReturn<T>(bool value, string failMessage) where T : class
        {
            SetFailed(value, failMessage);
            return null;
        }

        void SetFailed(bool value, string failMessage)
        {
            Failed = value;
            FailMessage = failMessage;
            Debug.Log($"Production failed: {failMessage}");
        }

        /// <summary>
        /// Keeps only one of the current nodes. Fails if no current nodes exist.
        /// </summary>
        public ProductionProgram SelectRandomOne(ProductionProgram program, out Node result) => Bind(out result, () =>
        {
            if (!program.CurrentNodes.Any())
                return SetFailedReturn<Node>(true, $"{nameof(SelectRandomOne)}: no current nodes exist");

            var node = program.CurrentNodes.GetRandom();
            CurrentNodes = node.ToEnumerable().ToList();
            return node;
        });

        /// <summary>
        /// Keeps only one of the current nodes. Fails if no current nodes exist.
        /// </summary>
        public ProductionProgram SelectFirstOne(ProductionProgram program, out Node result) => Bind(out result, () =>
        {
            if (!program.CurrentNodes.Any())
                return SetFailedReturn<Node>(true, $"{nameof(SelectFirstOne)}: no current nodes exist");

            var node = program.CurrentNodes.First();
            CurrentNodes = node.ToEnumerable().ToList();
            return node;
        });
        
        
        public ProductionProgram FindPath(Func<Node> pathFinder, out Node path) => Bind(out path, () =>
        {
            Node foundPath;
            try
            {
                foundPath = pathFinder();
            }
            catch (PathNotFoundException ex)
            {
                return SetFailedReturn<Node>(true, $"Can't find path: {ex.Message}");
            }
            CurrentNodes = foundPath.ToEnumerable().ToList();
            return foundPath;
        });

        /// <summary>
        /// Places the current nodes to the level. Each will be child of all nodes contained in from. 
        /// </summary>
        public ProductionProgram PlaceCurrentFrom(params Node[] from) => Bind(() =>
        {
            if (!CurrentNodes.Any())
            {
                SetFailed(true, $"{nameof(PlaceCurrentFrom)}: no current nodes exist");
                return;
            }

            var op = State.Add(from).SetTo(CurrentNodes.ToArray());
            AppliedOperations.Add(op);
        });

        public ProductionProgram ReplaceNodes(params Node[] from) => Bind(() =>
        {
            if (!CurrentNodes.Any())
            {
                SetFailed(true, $"{nameof(ReplaceNodes)}: no current nodes exist");
                return;
            }

            var op = State.Replace(from).SetTo(CurrentNodes.ToArray());
            AppliedOperations.Add(op);
        });

        public ProductionProgram Found() => Found(out var _);

        /// <summary>
        /// Returned node is not in derivation. It is just a container of newly created level elements.
        /// </summary>
        public ProductionProgram Found(out Node foundation) =>  Bind(out foundation, () =>
        {
            CurrentNodes = CurrentNodes.Select(node => Ldk.les.Foundation(node.LE).GN(Pr.sym.Foundation)).ToList();
            return CurrentNodes.Select(node => node.LE).ToLevelGroupElement(Ldk.grid).GN();
        });

        public ProductionProgram ReserveUpward(int height, Func<Node, Symbol> reservationSymbolF) => ReserveUpward(height, reservationSymbolF, out var _);

        /// <summary>
        /// Returned node is not in derivation. It is just a container of newly created level elements.
        /// </summary>
        public ProductionProgram ReserveUpward(int height, Func<Node, Symbol> reservationSymbolF, out Node reservation)
            => Bind(out reservation, () =>
        {
            var reservations = CurrentNodes
                .Select(node =>
                    State.NewProgram(prog => prog
                        .Set(() => node.LE.CG().ExtrudeVer(Vector3Int.up, height).LE(AreaStyles.Reservation()).GN(reservationSymbolF(node)))
                        .KeepNotTaken()
                    )
                );
            if (reservations.Any(prog => prog.Failed))
            {
                return SetFailedReturn<Node>(true, $"{nameof(ReserveUpward)}: subprogram failed");
            }

            CurrentNodes = reservations
                .Where(prog => !prog.Failed)
                .SelectMany(prog => prog.CurrentNodes).ToList();

            return CurrentNodes.Select(node => node.LE).ToLevelGroupElement(Ldk.grid).GN();
        });

        public ProductionProgram Directional(IEnumerable<Vector3Int> directions, Func<Vector3Int, Node> nodeCreator)
            => Bind(() => CurrentNodes = directions.Select(dir => nodeCreator(dir)).ToList());

        /// <summary>
        /// Remove the current nodes which are overlapping already added nodes.
        /// </summary>
        public ProductionProgram KeepNotTaken()
            => Bind(() => CurrentNodes = CurrentNodes.Where(node => node.LE.CG().AllAreNotTaken()).ToList());

        public ProductionProgram Keep(Func<Node, bool> condition)
            => Bind(() => CurrentNodes = CurrentNodes.Where(condition).ToList());

        /// <summary>
        /// Remove the current nodes for which foundation can't be created.
        /// </summary>
        public ProductionProgram CanBeFounded() => Keep(node => State.CanBeFounded(node.LE));

        /// <summary>
        /// Remove the current nodes which contain no cubes.
        /// </summary>
        public ProductionProgram NonEmpty() => Keep(node => node.LE.Cubes().Any());

        /// <summary>
        /// todo: what is difference from NotTaken?
        /// </summary>
        public ProductionProgram DontIntersectAdded() => Keep(node => !node.LE.CG().Intersects(State.WorldState.Added.CG()));

        public ProductionProgram Change(Func<Node, Node> changer)
            => Bind(() => CurrentNodes = CurrentNodes.SelectNN(changer).ToList());

        public ProductionProgram CurrentFirst(out Node first) => Bind(out first, () =>
        {
            var first = CurrentNodes.FirstOrDefault();
            return first ?? SetFailedReturn<Node>(true, $"{nameof(CurrentFirst)}: no current nodes exist");
        });

        public ProductionProgram Condition(Func<bool> condF) => Bind(() =>
        {
            if(!condF())
                SetFailed(true, "Condition failed");
        });

        /// <summary>
        /// Runs the program with the current State and CurrentNodes and its state after being run is applied to this.
        /// programF takes new program with the same state as argument.
        /// </summary>
        public ProductionProgram RunIf(bool condition, Func<ProductionProgram, ProductionProgram> programF) => Bind(() =>
        {
            if (condition)
            {
                var startingProgram = State.NewProgram(prog => prog);
                startingProgram.CurrentNodes = CurrentNodes;
                startingProgram.Failed = Failed;

                var program = programF(startingProgram);
                AppliedOperations.AddRange(program.AppliedOperations);

                CurrentNodes = program.CurrentNodes;
                Failed = program.Failed;
            }
        });

        public ProductionProgram Set(Func<Node> nodesF, out Node result) => Bind(out result, () =>
        {
            Set(() => nodesF().ToEnumerable());
            return CurrentNodes.First();
        });

        public ProductionProgram Set(Func<Node> nodesF)
            => Bind(() => Set(() => nodesF().ToEnumerable()));

        public ProductionProgram Set(Func<IEnumerable<Node>> nodesF)
            => Bind(() => CurrentNodes = nodesF().ToList());

        /// <summary>
        /// Moves current node near to targetNode. None of the cubes of the moved node is vertically taken yet.
        /// Doesn't work correctly for multiple current nodes.
        /// </summary>
        public ProductionProgram MoveNearTo(Node targetNode, int dist) => Bind(() =>
        {
            if (CurrentNodes.Count() != 1)
            {
                throw new InvalidOperationException($"{nameof(MoveNearTo)} only works for 1 node.");
            }

            Change(node =>
            {
                var nodeOnGround = node.LE.MoveBottomTo(0);
                var targeOnGround = targetNode.LE.MoveBottomTo(0);

                var movesInDistance = nodeOnGround
                    .MovesInDistanceXZ(targeOnGround, dist);

                var validMoves = movesInDistance
                    .DontIntersect(State.VerticallyTaken);

                return validMoves
                    .TryMove()?.GN();
            })
            // Move node to level of target node
            .Change(validNewRoom =>
            {
                return validNewRoom.LE.MoveBottomTo(targetNode.LE.CG().LeftBottomBack().y).GN();
            });
        });
    }
}

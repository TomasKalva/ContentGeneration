using OurFramework.Environment.GridMembers;
using OurFramework.Environment.ShapeCreation;
using OurFramework.Environment.StylingAreas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace OurFramework.Environment.ShapeGrammar
{
    public static class ShapeGrammarExtensions
    {
        /// <summary>
        /// Raises level element to grammar node.
        /// </summary>
        public static Node GN(this LevelElement le, params Symbol[] symbols)
        {
            return new Node(le, symbols.ToList());
        }
    }

    /// <summary>
    /// Keeps all the neccessary information about the shape grammar.
    /// </summary>
    public class ShapeGrammarState : IPrintable
    {
        public Node Root { get; }

        /// <summary>
        /// For operations that require querying already created world.
        /// </summary>
        public WorldGeometryState WorldState { get; set; }

        public Grid<bool> OffersFoundation { get; }
        /// <summary>
        /// Contains the node which has the cube. Doesn't include paths.
        /// </summary>
        public Grid<Node> CubeToNode { get; }
        /// <summary>
        /// Projection of the entire world to y == 0. 
        /// </summary>
        public LevelElement VerticallyTaken { get; set; }
        /// <summary>
        /// Nodes created by the last applied production rule.
        /// </summary>
        public IEnumerable<Node> LastCreated { get; private set; }
        /// <summary>
        /// Nodes that can be used as targets for production rules.
        /// </summary>
        public IEnumerable<Node> ActiveNodes { get; set; }

        /// <summary>
        /// Information about evaluation of the grammar.
        /// </summary>
        public class GrammarStats
        {
            /// <summary>
            /// Information about try of application of a production rule.
            /// </summary>
            public class ProductionInstanceStats : IPrintable
            {
                public string Name { get; }
                public long TimeMs { get; }
                public List<Operation> Operations { get; }
                public int TriedParameters { get; }
                public bool Applied => Operations != null;
                public int NumAddedNodes { get; }

                public ProductionInstanceStats(string name, List<Operation> operations, long timeMs, int triedParameters, int numAddedNodes)
                {
                    Name = name;
                    TimeMs = timeMs;
                    Operations = operations;
                    TriedParameters = triedParameters;
                    NumAddedNodes = numAddedNodes;
                }

                public PrintingState Print(PrintingState state)
                {
                    state.PrintLine($"{(Applied  ? "Success:" : "Fail:\t")}\t{TimeMs}ms\t\t{TriedParameters} pars\t\t{NumAddedNodes} added\t\t{Name}");
                    return state;
                }
            }

            /// <summary>
            /// All tried production instances.
            /// </summary>
            public List<ProductionInstanceStats> ProductionInstances { get; }
            public IEnumerable<ProductionInstanceStats> AppliedProductions() => ProductionInstances.Where(production => production.Applied);

            public GrammarStats()
            {
                ProductionInstances = new List<ProductionInstanceStats>();
            }

            public void AddApplied(string name, IEnumerable<Operation> operations, long timeMs, int triedParameters, int numAddedNodes)
            {
                ProductionInstances.Add(new ProductionInstanceStats(name, operations.ToList(), timeMs, triedParameters, numAddedNodes));
            }

            public void AddFailed(string name, long timeMs, int triedParameters)
            {
                ProductionInstances.Add(new ProductionInstanceStats(name, null, timeMs, triedParameters, 0));
            }

            public void Print()
            {
                var printingState = new PrintingState();
                ProductionInstances.ForEach(p => p.Print(printingState));
                printingState.Show();
            }
        }

        public GrammarStats Stats { get; }

        public ShapeGrammarState(LevelDevelopmentKit ldk)
        {
            var grid = ldk.grid;
            var empty = LevelElement.Empty(grid);
            Root = new Node(empty, new List<Symbol>());
            // Applying style after every level element so that face types can be referenced
            //  -usefull for locking door
            WorldState = new WorldGeometryState(empty, grid, le =>
            {
                le.CG().Cubes.ForEach(cube => cube.Changed = true);
                return le.ApplyGrammarStyles();
            });
            WorldState.Add(empty);
            OffersFoundation = new Grid<bool>(new Vector3Int(10, 1, 10), (_1, _2) => true);
            CubeToNode = new Grid<Node>(new Vector3Int(10, 10, 10), (_1, _2) => null);
            VerticallyTaken = LevelElement.Empty(grid);
            Stats = new GrammarStats();
            ActiveNodes = Root.AllDerived();
        }

        /// <summary>
        /// Tries to apply the production to the currently derived level. 
        /// Returns newly created nodes or null if it fails.
        /// </summary>
        public IEnumerable<Node> ApplyProduction(Production production)
        {
            var sw = new Stopwatch();
            sw.Start();
            var operations = production.TryApply(this, out int triedParameters);
            sw.Stop();
            var elapsedMs = sw.ElapsedMilliseconds;

            if (operations == null)
            {
                // Production application failed
                Stats.AddFailed(production.Name, elapsedMs, triedParameters);
                return null;
            }
            else
            {
                // Production application succeeded
                var dagBeforeCount = Root.AllDerived().Count();
                var newNodes = operations.SelectMany(operation => operation.ChangeState(this)).Evaluate();
                var dagAfterCount = Root.AllDerived().Count();
                Stats.AddApplied(production.Name, operations.ToList(), elapsedMs, triedParameters, dagAfterCount - dagBeforeCount);
                LastCreated = newNodes;
                return newNodes;
            }
        }

        /// <summary>
        /// Returns the active nodes which contail all the symbols.
        /// </summary>
        public IEnumerable<Node> ActiveWithSymbols(params Symbol[] symbols)
        {
            return ActiveNodes.Where(node => !node.Terminal && node.HasSymbols(symbols));
        }

        /// <summary>
        /// Returns true iff le can create foundation.
        /// </summary>
        public bool CanBeFounded(LevelElement le)
        {
            return le.Cubes().All(cube => OffersFoundation[new Vector3Int(cube.Position.x, 0, cube.Position.z)]);
        }

        /// <summary>
        /// ChangeProgram allows us to access the production program inside its declaration.
        /// </summary>
        public ProductionProgram NewProgram(Func<ProductionProgram, ProductionProgram> changeProgram)
        {
            return changeProgram(new ProductionProgram(this));
        }

        #region Operation factories
        public Operation Add(params Node[] from)
        {
            return new AddNew()
            {
                From = from,
            };
        }

        public Operation Replace(params Node[] from)
        {
            return new Replace()
            {
                From = from,
            };
        }

        #endregion

        #region Visualization
        /// <summary>
        /// Prints stats.
        /// </summary>
        public PrintingState Print(PrintingState state)
        {
            Stats.AppliedProductions().ForEach(appliedPr =>
            {
                state.PrintLine(appliedPr.Name).ChangeIndent(1);
                appliedPr.Operations.ForEach(op => op.Print(state).PrintLine());
                state.ChangeIndent(-1);
            });
            return state;
        }

        /// <summary>
        /// Prints all nodes.
        /// </summary>
        public void ShowAllNodes()
        {
            var printingState = new PrintingState();
            Root.AllDerived().ForEach(node =>
            {
                node.Print(printingState);
                printingState.PrintLine();
            });
            printingState.Show();
        }

        /// <summary>
        /// Shows VerticallyTaken.
        /// </summary>
        public void ShowVerticallyTaken()
        {
            VerticallyTaken.SetAreaStyle(AreaStyles.Garden()).ApplyGrammarStyles();
        }
        #endregion

        /// <summary>
        /// Returns node corresponding to the position. Returns null if there is no such node.
        /// </summary>
        public Node GetNode(Vector3Int cubePos)
        {
            return CubeToNode[cubePos];
        }
    }
}

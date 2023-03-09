using OurFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static OurFramework.LevelDesignLanguage.AsynchronousEvaluator;
using OurFramework.Environment.GridMembers;
using OurFramework.Environment.StylingAreas;
using OurFramework.Gameplay.RealWorld;

namespace OurFramework.Environment.ShapeGrammar
{
    /// <summary>
    /// Represents the nodes in the world.
    /// </summary>
    public class WorldGeometryState
    {
        /// <summary>
        /// Contains all level elements created.
        /// </summary>
        public LevelGroupElement Added { get; private set; }
        /// <summary>
        /// Grid to which level elements are put.
        /// </summary>
        public Grid<Cube> Grid { get; }
        /// <summary>
        /// Transforms the level element before its added.
        /// </summary>
        public Func<LevelElement, LevelElement> AfterPushed { get; }

        public WorldGeometryState(LevelElement last, Grid<Cube> grid, Func<LevelElement, LevelElement> afterPushed)
        {
            Grid = grid;
            Added = new LevelGroupElement(grid, AreaStyles.None());
            AfterPushed = afterPushed;
        }

        /// <summary>
        /// Adds le to the world.
        /// </summary>
        public void Add(LevelElement le)
        {
            Debug.Assert(le != null, "Trying to add null level element to WorldState");

            var newLe = AfterPushed(le);
            Added = Added.Merge(newLe);
        }

        public IEnumerable<TaskSteps> CreateGeometry(IGridGeometryOwner world)
        {
            var cubeSide = world.WorldGeometry.WorldScale;
            int iteration = 0;
            //var iterChunks = Grid.chunks.Values.ToList();
            foreach (var chunk in Grid.Chunks().ToList())
            {
                foreach (var cube in chunk)
                {
                    if (iteration++ % 10 == 0)
                    {
                        yield return TaskSteps.One();
                    }
                    cube.CreateGeometry(cubeSide, world);
                }
            }

            var interactiveArchitecture = world.ArchitectureParent.GetComponentsInChildren<InteractiveObject>().Select(io => io.State);
            interactiveArchitecture.ForEach(el => world.AddInteractivePersistentObject(el));
        }
    }
}

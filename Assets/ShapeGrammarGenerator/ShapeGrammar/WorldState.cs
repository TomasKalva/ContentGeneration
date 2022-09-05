using Assets.ShapeGrammarGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    /// <summary>
    /// Represents the nodes in the world.
    /// </summary>
    public class WorldState
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

        public WorldState(LevelElement last, Grid<Cube> grid, Func<LevelElement, LevelElement> afterPushed)
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
    }
}

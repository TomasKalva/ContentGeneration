using ContentGeneration.Assets.UI.Model;
using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions
{

    /// <summary>
    /// Persistent over the entire game.
    /// </summary>
    class Faction
    {
        /// <summary>
        /// Affinity of player with the faction.
        /// </summary>
        public int Affinity { get; set; }

        public FactionManifestation GetFactionManifestation()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Sequence of environments.
    /// </summary>
    class FactionManifestation
    {
        public Faction Faction { get; }

        public FactionEnvironment GetFactionEnvironment()
        {
            throw new NotImplementedException();
        }

        public InteractiveObjectState ContinueManifestation()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// One environment put in one level.
    /// </summary>
    class FactionEnvironment
    {
        public FactionManifestation FactionManifestation { get; }

        public ProductionList ProductionList()
        {
            throw new NotImplementedException();
        }

        public Placer<Areas, CharacterState> EnemyPlacer()
        {
            throw new NotImplementedException();
        }

        public Placer<Areas, ItemState> ItemPlacer()
        {
            throw new NotImplementedException();
        }

        public Placer<Areas, InteractiveObjectState> InteractiveObjectPlacer()
        {
            throw new NotImplementedException();
        }

        public Placer<Areas, ObjectState> ObjectPlacer()
        {
            throw new NotImplementedException();
        }

        public void PlaceEverything(LinearPath path)
        {
            EnemyPlacer().Place(path);
            ItemPlacer().Place(path);
            InteractiveObjectPlacer().Place(path);
            ObjectPlacer().Place(path);
        }
    }



    class FactionConcepts
    {
        List<ProductionList> productionLists;

        public FactionConcepts TakeSubset(float sizeRatio)
        {
            throw new NotImplementedException();
        }

        

        /*public Environment(int progress)
        {

        }*/
    }
}

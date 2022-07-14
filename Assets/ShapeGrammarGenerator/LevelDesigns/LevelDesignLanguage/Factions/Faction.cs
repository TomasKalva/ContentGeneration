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
        public FactionConcepts Concepts { get; }

        /// <summary>
        /// Affinity of player with the faction.
        /// </summary>
        public int Affinity { get; set; }

        public int StartingBranchProgress => Math.Min(3, Affinity / 4);

        public Faction(FactionConcepts concepts)
        {
            Concepts = concepts;
            Affinity = 0;
        }

        public FactionManifestation GetFactionManifestation()
        {
            return new FactionManifestation(Concepts, this);
        }
    }

    /// <summary>
    /// Sequence of environments.
    /// </summary>
    class FactionManifestation
    {
        public FactionConcepts Concepts { get; }
        public Faction Faction { get; }
        /// <summary>
        /// How many environments player already went through.
        /// </summary>
        public int Progress { get; set; }

        public FactionManifestation(FactionConcepts concepts, Faction faction)
        {
            Concepts = concepts;
            Faction = faction;
            Progress = 0;
        }

        public FactionEnvironment GetFactionEnvironment()
        {
            return new FactionEnvironment(Concepts, this);
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
        public FactionConcepts Concepts { get; }
        public FactionManifestation FactionManifestation { get; }

        public FactionEnvironment(FactionConcepts concepts, FactionManifestation factionManifestation)
        {
            Concepts = concepts;
            FactionManifestation = factionManifestation;
        }

        public ProductionList ProductionList()
        {
            return Concepts.ProductionLists.GetRandom()();
        }

        /// <summary>
        /// Returns a factory that returns similar items.
        /// </summary>
        public ProgressFactory<ItemState> CreateItemFactory()
        {
            var affinity = FactionManifestation.Faction.Affinity;
            var progress = FactionManifestation.Progress;

            // Add effects to the item


            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a factory that returns similar enemies.
        /// </summary>
        public ProgressFactory<CharacterState> CreateEnemyFactory()
        {
            var affinity = FactionManifestation.Faction.Affinity;
            var progress = FactionManifestation.Progress;

            // Create stats of the enemy

            // Create weapon for the enemy

            // Create items for the enemy

            return progress =>
            {
                var character = Concepts.CharacterStates.GetRandom()();
                return character;
            };
        }


        /// <summary>
        /// Returns a factory that returns interactive objects.
        /// </summary>
        public ProgressFactory<InteractiveObjectState> CreateInteractiveObjectFactory()
        {
            var affinity = FactionManifestation.Faction.Affinity;
            var progress = FactionManifestation.Progress;
            
            // Create interaction of the interactive object

            throw new NotImplementedException();
        }
    }

    class FactionConcepts
    {
        public List<Func<ProductionList>> ProductionLists { get; }
        public List<Func<CharacterState>> CharacterStates { get; }

        public FactionConcepts(List<Func<ProductionList>> productionLists, List<Func<CharacterState>> characterStates)
        {
            ProductionLists = productionLists;
            CharacterStates = characterStates;
        }

        public FactionConcepts TakeSubset(float sizeRatio)
        {
            throw new NotImplementedException();
        }

        

        /*public Environment(int progress)
        {

        }*/
    }
}

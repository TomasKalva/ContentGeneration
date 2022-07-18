using Assets.Util;
using ContentGeneration.Assets.UI.Model;
using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
            var faction = FactionManifestation.Faction;
            var affinity = faction.Affinity;
            var progress = FactionManifestation.Progress;


            // Add effects to the item
            return envProgress =>
            {
                var annotatedEffectByUser = Concepts.Effects.GetRandom();
                var annotatedSelectorByUserByArgs = Concepts.Selectors.GetRandom();
                var selectorByUser = annotatedSelectorByUserByArgs.Item(new SelectorArgs(Color.yellow, Concepts.Textures.GetRandom()));

                return new ItemState()
                {
                    Name = annotatedEffectByUser.Name,
                    Description = $"So basically it {annotatedEffectByUser.Description} {annotatedSelectorByUserByArgs.Description}."
                }
                    .OnUse(ch => 
                    {
                        Debug.Log($"Procedural item is used by {ch.Agent.gameObject.name}");
                        var occurence = new Occurence(selectorByUser(ch), annotatedEffectByUser.Item(faction)(ch));
                        ch.World.AddOccurence(occurence);
                    });
            };

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
        public List<Annotated<EffectByFactionByUser>> Effects { get; }
        public List<Annotated<SelectorByUserByArgs>> Selectors { get; }
        public List<FlipbookTexture> Textures { get; }

        public FactionConcepts(List<Func<ProductionList>> productionLists, List<Func<CharacterState>> characterStates, List<Annotated<EffectByFactionByUser>> effects, List<Annotated<SelectorByUserByArgs>> selectors, List<FlipbookTexture> flipbookTextures)
        {
            ProductionLists = productionLists;
            CharacterStates = characterStates;
            Effects = effects;
            Selectors = selectors;
            Textures = flipbookTextures;
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

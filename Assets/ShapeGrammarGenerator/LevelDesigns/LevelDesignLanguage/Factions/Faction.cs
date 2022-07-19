﻿using Assets.Util;
using ContentGeneration.Assets.UI.Model;
using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ShapeGrammar.FactionsLanguage;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions
{


    /// <summary>
    /// Persistent over the entire game.
    /// </summary>
    class Faction
    {
        FactionConcepts Concepts { get; }

        public UniqueNameGenerator UniqueNameGenerator { get; }

        /// <summary>
        /// Affinity of player with the faction.
        /// </summary>
        public int Affinity { get; set; }



        public int StartingBranchProgress => Math.Min(3, Affinity / 4);

        public Faction(FactionConcepts concepts, UniqueNameGenerator uniqueNameGenerator)
        {
            Concepts = concepts;
            UniqueNameGenerator = uniqueNameGenerator;
            Affinity = 0;
        }

        public FactionManifestation GetFactionManifestation()
        {
            return new FactionManifestation(Concepts.TakeSubset(3, 3, 2, 2, 6, 6), this);
        }
    }

    /// <summary>
    /// Sequence of environments.
    /// </summary>
    class FactionManifestation
    {
        FactionConcepts Concepts { get; }
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
            return new FactionEnvironment(Concepts.TakeSubset(2, 2 + Progress, 2, 2, 3, 3), this);
        }

        public void ContinueManifestation(LevelConstructor levelConstructor, IEnumerable<FactionEnvironmentConstructor> branches)
        {
            Progress++;
            levelConstructor.AddEvent(
                new LevelConstructionEvent(
                    10 + Progress,
                    () =>
                    {
                        branches.GetRandom()(GetFactionEnvironment(), Progress);
                        return true;
                    })
                );
        }
    }

    /// <summary>
    /// One environment put in one level.
    /// </summary>
    class FactionEnvironment
    {
        FactionConcepts Concepts { get; }
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
        /// Returns a factory that returns the same items.
        /// </summary>
        public ProgressFactory<ItemState> CreateItemFactory()
        {
            /*
            var faction = FactionManifestation.Faction;
            var affinity = faction.Affinity;
            var progress = FactionManifestation.Progress;*/

            // Fix item properties
            var annotatedEffectByFactionEnvByUser = Concepts.Effects.GetRandom();
            var effectByUser = annotatedEffectByFactionEnvByUser.Item(this);

            var annotatedSelectorByArgsByUser = Concepts.Selectors.GetRandom();
            var selectorByUser = annotatedSelectorByArgsByUser.Item(new SelectorArgs(Color.yellow, Concepts.Textures.GetRandom()));

            var name = FactionManifestation.Faction.UniqueNameGenerator.GenerateUniqueName(Concepts.Adjectives, Concepts.Nouns);
            name = string.Concat(name[0].ToString().ToUpper(), name.Substring(1));

            // Add effects to the item
            return _ => // the effects are independent of the progress so that they can easily stack
            {
                return new ItemState()
                {
                    Name = name,
                    Description = $"So basically it {annotatedEffectByFactionEnvByUser.Description} {annotatedSelectorByArgsByUser.Description}."
                }
                    .OnUse(ch => 
                    {
                        Debug.Log($"Procedural item is used by {ch.Agent.gameObject.name}");
                        var occurence = new Occurence(selectorByUser(ch), effectByUser(ch));
                        ch.World.AddOccurence(occurence);
                    })
                    .SetConsumable();
            };
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
        public List<Annotated<EffectByFactionEnvironmentByUser>> Effects { get; }
        public List<Annotated<SelectorByArgsByUser>> Selectors { get; }
        public List<FlipbookTexture> Textures { get; }
        public List<string> Nouns { get; }
        public List<string> Adjectives { get; }

        public FactionConcepts(
            List<Func<ProductionList>> productionLists, 
            List<Func<CharacterState>> characterStates, 
            List<Annotated<EffectByFactionEnvironmentByUser>> effects, 
            List<Annotated<SelectorByArgsByUser>> selectors, 
            List<FlipbookTexture> flipbookTextures,
            List<string> nouns,
            List<string> adjectives)
        {
            ProductionLists = productionLists;
            CharacterStates = characterStates;
            Effects = effects;
            Selectors = selectors;
            Textures = flipbookTextures;
            Nouns = nouns;
            Adjectives = adjectives;
        }

        public FactionConcepts TakeSubset(int characterStatesCount, int effectsCount, int selectorsCount, int texturesCount, int nounsCount, int adjectivesCount)
        {
            return new FactionConcepts(
                    ProductionLists,
                    CharacterStates.Shuffle().Take(characterStatesCount).ToList(),
                    Effects.Shuffle().Take(effectsCount).ToList(),
                    Selectors.Shuffle().Take(selectorsCount).ToList(),
                    Textures.Shuffle().Take(texturesCount).ToList(),
                    Nouns.Shuffle().Take(nounsCount).ToList(),
                    Adjectives.Shuffle().Take(adjectivesCount).ToList()
                );
        }

        /*public Environment(int progress)
        {

        }*/
    }

    class UniqueNameGenerator
    {
        Dictionary<string, int> AlreadyGenerated { get; }

        public UniqueNameGenerator()
        {
            AlreadyGenerated = new Dictionary<string, int>();
        }

        public string GenerateUniqueName(List<string> adjectives, List<string> nouns)
        {
            int c = 100;
            string generated = "";
            while (c-- >= 0)
            {
                generated = $"{adjectives.GetRandom()} {nouns.GetRandom()}";
                if (!AlreadyGenerated.ContainsKey(generated))
                {
                    AlreadyGenerated.Add(generated, 0);
                    return generated;
                }
            }
            var n = ++AlreadyGenerated[generated];
            return $"{generated} {n}";
        }
    }
}
using Assets.Util;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
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
        public FactionConcepts Concepts { get; }

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
            return new FactionManifestation(Concepts.TakeSubset(3, 3), this);
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
            return new FactionEnvironment(Concepts.TakeSubset(2, 1 + Progress), this);
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
            var manifestationProgress = FactionManifestation.Progress;

            return _ =>
            {
                return FactionManifestation.Faction.Concepts.Spells[manifestationProgress].GetRandom()().SetStackable(1);
            };
        }



        /// <summary>
        /// Returns a factory that returns similar enemies.
        /// </summary>
        public ProgressFactory<CharacterState> CreateEnemyFactory()
        {
            var affinity = FactionManifestation.Faction.Affinity;
            var manifestationProgress = FactionManifestation.Progress;

            // Create stats of the enemy
            var scalingStats = new ScalingCharacterStats();

            // Create items for the enemy

            return progress =>
            {
                var character = Concepts.CharacterStates.GetRandom()();

                character.Stats = scalingStats.GetStats(manifestationProgress);
                character.SetOnDeath(() => GameViewModel.ViewModel.PlayerState.Spirit += character.Health.Maximum);

                // Create weapon for the enemy
                var leftWeaponF = Concepts.Weapons.GetRandom();
                var rightWeaponF = Concepts.Weapons.GetRandom();

                character.SetItemToSlot(SlotType.LeftWeapon, leftWeaponF());
                character.SetItemToSlot(SlotType.RightWeapon, rightWeaponF());

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
        public List<Func<WeaponItem>> Weapons { get; }
        public List<List<Func<ItemState>>> Spells { get; }

        public FactionConcepts(
            List<Func<ProductionList>> productionLists, 
            List<Func<CharacterState>> characterStates, 
            List<Func<WeaponItem>> weapons,
            List<List<Func<ItemState>>> spells)
        {
            ProductionLists = productionLists;
            CharacterStates = characterStates;
            Weapons = weapons;
            Spells = spells;
        }

        public FactionConcepts TakeSubset(int characterStatesCount, int weaponsCount)
        {
            return new FactionConcepts(
                    ProductionLists,
                    CharacterStates.Shuffle().Take(characterStatesCount).ToList(),
                    Weapons.Shuffle().Take(weaponsCount).ToList(),
                    Spells
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

    delegate ByUser<Effect> EffectByFactionEnvironmentByUser(FactionEnvironment factionEnv);

    class FactionScalingEffectLibrary
    {
        public List<Annotated<EffectByFactionEnvironmentByUser>> EffectsByUser { get; }

        float EffectPower(FactionEnvironment factionEnv, CharacterState user)
        {
            var affinity = factionEnv.FactionManifestation.Faction.Affinity;
            var manifProgress = factionEnv.FactionManifestation.Progress;
            var vers = user.Stats.Versatility;
            return affinity + 7 * manifProgress + vers;
        }

        Annotated<EffectByFactionEnvironmentByUser> FromPower(string name, string description, Func<float, Effect> powerToEffect)
        {

            return new Annotated<EffectByFactionEnvironmentByUser>(name, description, faction => user =>
            {
                var power = EffectPower(faction, user);
                return powerToEffect(power);
            });
        }

        public FactionScalingEffectLibrary(EffectLibrary eff)
        {
            EffectsByUser = new List<Annotated<EffectByFactionEnvironmentByUser>>()
            {
                FromPower("Heal", "heals", p => eff.Heal(5f + 5f * p)),
                FromPower("Chaos", "gives chaose damage", p => eff.Damage(new DamageDealt(DamageType.Chaos, 10f + 5f * p))),
                FromPower("Dark", "gives dark damage", p => eff.Damage(new DamageDealt(DamageType.Dark, 10f + 5f * p))),
                FromPower("Divine", "gives divine damage", p => eff.Damage(new DamageDealt(DamageType.Divine, 10f + 5f * p))),
                FromPower("Give spirit", "gives spirit to", p => eff.GiveSpirit(10f + 2f * p)),
                FromPower("Bleed", "applies bleeding to", p => eff.Bleed(5f + 2f * p, 2f)),
                FromPower("Boost stamina regeneration", "boosts stamina regeneration to", p => eff.BoostStaminaRegen(5f + 2f * p, 2f)),
                FromPower("Regenerate health", "regenerates health to", p => eff.RegenerateHealth(5f + 2f * p, 2f)),
            };
        }
    }

    class ScalingCharacterStats
    {
        public CharacterStats GetStats(int manifestationProgress)
        {
            var stats = new CharacterStats()
            {
                Will = 4 * manifestationProgress,
                Strength = 5 + 5 * manifestationProgress,
                Versatility = 5 * manifestationProgress
            };

            var statChanges = CharacterStats.StatChanges();
            var randomisedStats = new Stat[]
            {
                Stat.Endurance,
                Stat.Agility,
                Stat.Posture,
                Stat.Resistances,
            };
            var increasedStats = randomisedStats.Take(manifestationProgress).ToHashSet();

            statChanges.Where(sc => increasedStats.Contains(sc.Stat))
                .ForEach(sc => sc.Manipulate(stats, 4 + 4 * manifestationProgress));

            return stats;

        }
    }
}

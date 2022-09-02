using Assets.Characters.Items.ItemClasses;
using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
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
using static Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions.SelectorLibrary;
using static InteractiveObject;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage
{

    class OutOfDepthEncountersLanguage : LDLanguage
    {
        public OutOfDepthEncountersLanguage(LanguageParams tools) : base(tools) { }

        public CharacterStats GetStats(int level)
        {
            var stats = new CharacterStats()
            {
                Will = 5 + 3 * level,
                Strength = 10 + 3 * level,
                Versatility = 5 + 5 * level,
                Endurance = 30 + 5 * level,
                Agility = 50 + 5 * level,
                Posture = 10 + level,
                Resistances = 3 + 3 * level
            };

            return stats;
        }

        public IEnumerable<Func<WeaponItem>> GetWeapon(int level)
        {
            return new List<Func<WeaponItem>>()
            { 
                () => Lib.Items.Katana().AddUpgradeEffect(user => target => Lib.Effects.Bleed(3 + 2 * level, 6)),
                () => Lib.Items.Mace().AddUpgradeEffect(user => target => Lib.Effects.DamagePosture(5 + 10 * level)),
                () => Lib.Items.SculptureClub().AddUpgradeEffect(user => target => Lib.Effects.PushFrom(50f + 50f * level)(user)),
                () => Lib.Items.MayanKnife().AddUpgradeEffect(user => target => Lib.Effects.Heal(3 + 3 * level)(user)),
            };
        }

        public CharacterState EnhanceEnemy(CharacterState enemy, int level) 
        {
            var weaponF = GetWeapon(level).GetRandom();
            return enemy
                .SetStats(GetStats(level))
                .SetLeftWeapon(weaponF())
                .SetRightWeapon(weaponF())
                .AddOnDeath(() => GameViewModel.ViewModel.PlayerState.Spirit += 3 * enemy.Health.Maximum)
                .SetCreatingStrategy(new CreateIfCondition(() => enemy.DeathCount == 0));
        }

        public IEnumerable<Func<ItemState>> UpgradeRewards(int _)
        {
            return 
                new Stat[3]
                { 
                    Stat.Will, 
                    Stat.Strength, 
                    Stat.Endurance 
                }
                    .Select<Stat, Func<ItemState>>(stat => 
                        () => Lib.Items.NewItem($"Experience: {stat.ToString()}", $"There are no secrets behind the power of the Ancients. Once finally put to rest after millenia of slow decomposition, their experience might serve as a gentel warnings to those who take over.")
                            .OnUse(ch => CharacterStats.StatChanges[stat].Manipulate(ch.Stats, 3))
                            .SetConsumable()
                    );
        }

        public class Encounter
        {
            public IEnumerable<CharacterState> Enemies { get; }

            public Encounter(params CharacterState[] enemies)
            {
                Enemies = enemies;
            }
        }

        public IEnumerable<Func<Encounter>> Encounters()
            => new List<Func<Encounter>>()
            {
                () => new Encounter(
                    Lib.Enemies.DragonMan()
                        .DropItem(
                            () =>
                                Lib.InteractiveObjects.Item(
                                    Lib.Items.NewItem(
                                        "Stubbornness of Half Dragon",
                                        "Once a noble man with no ambitions other than turning himself into a cold blooded beast. Upon fracturing last semblances of Common Sense his wish was finally granted."
                                    )
                                    .SetStackable(1)
                                    .OnUse(Lib.Effects.Heal(1000))
                            )
                        )
                ),
                () => new Encounter(
                    Lib.Enemies.Sculpture()
                        .DropItem(
                            () =>
                                Lib.InteractiveObjects.Item(
                                    Lib.Items.NewItem(
                                        "Serenity of Sculpture",
                                        "Sculptures belong to the few entities originating to this place. They ranked among the few supporters of Ariamel and his Master Plans, before his unfortunate descent into insanity."
                                    )
                                    .SetStackable(1)
                                    .OnUse(Lib.Effects.GiveSpirit(1000))
                            )
                        )
                ),
                () => new Encounter(
                    Lib.Enemies.SkinnyWoman()
                        .DropItem(
                            () => 
                                Lib.InteractiveObjects.Item(
                                    Lib.Items.NewItem(
                                        "Complacency of Headless Lady",
                                        "Although a rare phoenomenon in the Lands of Abundance, Headless Ladies are a natural speciment in these parts. Their rejection from Source due to their high affiliation with witchcraft is a likely reason for their migration."
                                    )
                                    .SetStackable(1)
                                    .OnUse(Lib.Effects.RegenerateHealth(2, 60))
                            )
                        )
                    ),
            };

        public void DifficultEncounter(int level)
        {
            // Create first path to the encounter
            Env.Line(Gr.PrL.Town(), NodesQueries.All, 2, out var path);

            // Place the encounter
            var enemies = Encounters().GetRandom()().Enemies;
            var arena = path.AreasList[1];
            enemies.Select(enemy => EnhanceEnemy(enemy, level)).ForEach(enemy => arena.AddEnemy(enemy));

            // Create a locked area after the encounter
            var key = L.PatternLanguage.CreateLockItems(State.UniqueNameGenerator.UniqueName("Integral part"), 1, "The pathway rarely opens without the detachment of Integrand from its Integree being achieved.", out var unlock).First();
            L.PatternLanguage.LockedArea(_ => path.LastArea().Node.ToEnumerable(), unlock, out var locked);

            // Give enemy key to the area
            var mainEnemy = enemies.First();
            mainEnemy
                .AddOnDeath(() => Msg.Show("Ancient disintegrated"))
                .DropItem(() => Lib.InteractiveObjects.Item(key));

            // Place rewards
            var rewards = UpgradeRewards(level).ToList();
            rewards.Shuffle().Take(2).ForEach(reward =>
                locked.Get.AddInteractiveObject(Lib.InteractiveObjects.Item(reward())));
        }
    }
}

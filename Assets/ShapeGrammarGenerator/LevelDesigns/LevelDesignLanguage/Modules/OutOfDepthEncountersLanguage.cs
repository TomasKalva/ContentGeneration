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
                .AddOnDeath(() => GameViewModel.ViewModel.PlayerState.Spirit += 3 * enemy.Health.Maximum);
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
                        () => Lib.Items.NewItem($"Glory: {stat.ToString()}", $"Greatly increases {stat}")
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
                ),
                () => new Encounter(
                    Lib.Enemies.Sculpture()
                ),
                () => new Encounter(
                    Lib.Enemies.Dog(),
                    Lib.Enemies.SkinnyWoman()
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
            var key = L.PatternLanguage.CreateLockItems(State.UniqueNameGenerator.UniqueName("Soulbound Key"), 1, "Unlocks a door", out var unlock).First();
            L.PatternLanguage.LockedArea(_ => path.LastArea().Node.ToEnumerable(), unlock, out var locked);

            // Give enemy key to the area
            var mainEnemy = enemies.First();
            mainEnemy.DropItem(Lib.InteractiveObjects.Item(key));

            // Place rewards
            var rewards = UpgradeRewards(level).ToList();
            rewards.Shuffle().Take(2).ForEach(reward =>
                locked.Get.AddInteractiveObject(Lib.InteractiveObjects.Item(reward())));
        }
    }
}

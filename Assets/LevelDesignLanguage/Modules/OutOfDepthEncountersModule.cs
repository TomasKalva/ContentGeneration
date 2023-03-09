using OurFramework.UI;
using OurFramework.UI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using OurFramework.Environment.ShapeGrammar;
using OurFramework.Gameplay.Data;
using OurFramework.Util;
using OurFramework.Game;

namespace OurFramework.LevelDesignLanguage.CustomModules
{

    class OutOfDepthEncountersModule : LDLanguage
    {
        public OutOfDepthEncountersModule(LanguageParams parameters) : base(parameters) { }

        CharacterStats GetStats(int level)
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

        IEnumerable<Func<WeaponItem>> GetWeapon(int level)
        {
            return new List<Func<WeaponItem>>()
            { 
                () => Lib.Items.Katana().AddUpgradeEffect(user => target => Lib.Effects.Bleed(3 + 2 * level, 6)),
                () => Lib.Items.Mace().AddUpgradeEffect(user => target => Lib.Effects.DamagePosture(5 + 10 * level)),
                () => Lib.Items.SculptureClub().AddUpgradeEffect(user => target => Lib.Effects.PushFrom(50f + 50f * level)(user)),
            };
        }

        CharacterState EnhanceEnemy(CharacterState enemy, int level) 
        {
            var weaponF = GetWeapon(level).GetRandom();
            return enemy
                .SetStats(GetStats(level))
                .SetLeftWeapon(weaponF())
                .SetRightWeapon(weaponF())
                .AddOnDeath(() => GameViewModel.ViewModel.PlayerState.Spirit += 3 * enemy.Health.Maximum)
                .SetCreatingStrategy(new CreateIfCondition(() => enemy.DeathCount == 0));
        }

        IEnumerable<Func<ItemState>> UpgradeRewards()
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

        IEnumerable<Func<CharacterState>> Enemies()
            => new List<Func<CharacterState>>()
            {
                () => Lib.Enemies.DragonMan()
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
                ,
                () => Lib.Enemies.Sculpture()
                        .DropItem(
                            () =>
                                Lib.InteractiveObjects.Item(
                                    Lib.Items.NewItem(
                                        "Serenity of Sculpture",
                                        "Sculptures belong to the few entities originating to the World. They ranked among the few supporters of Ariamel and his Master Plans, before his unfortunate descent into insanity."
                                    )
                                    .SetStackable(1)
                                    .OnUse(Lib.Effects.GiveSpirit(1000))
                            )
                        )
                ,
                () => Lib.Enemies.SkinnyWoman()
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
                    ,
            };

        public void DifficultEncounter(int level)
        {
            // Create first path to the encounter
            Env.Line(Gr.PrL.Town(), NodesQueries.All, 2, out var path);

            // Place the encounter
            var enemy = Enemies().GetRandom()();
            var arena = path.AreasList[1];
            EnhanceEnemy(enemy, level);
            arena.AddEnemy(enemy);

            // Create a locked area after the encounter
            var key = M.LockingModule.CreateLockItems(State.UniqueNameGenerator.UniqueName("Integral part"), 1, "The pathway never opens without the detachment of Integrand from its Integree being achieved.", out var unlock).First();
            M.LockingModule.LockedArea(_ => path.LastArea().Node.ToEnumerable(), unlock, out var locked);

            // Give enemy key to the area
            enemy
                .AddOnDeath(() => Msg.Show("Ancient disintegrated"))
                .DropItem(() => Lib.InteractiveObjects.Item(key));

            // Place rewards
            var rewards = UpgradeRewards().ToList();
            rewards.Shuffle().Take(2).ForEach(reward =>
                locked.Area.AddInteractiveObject(Lib.InteractiveObjects.Item(reward())));
        }

        public void AddLightMaceEncounter()
        {
            var enemy = Lib.Enemies.SkinnyWoman()
                .DropItem(
                    () =>
                        Lib.InteractiveObjects.Item(Lib.Items.LightMace())
                        );
            State.LC.AddNecessaryEvent($"Light mace encounter", 70, level => M.OutOfDepthEncountersModule.LightMaceEncounter(level, enemy), true, _ => enemy.DeathCount == 0);
        }

        void LightMaceEncounter(int level, CharacterState enemy)
        {
            // Create first path to the encounter
            Env.One(Gr.PrL.Town(), NodesQueries.All, out var area);

            // Place the encounter
            enemy
                .SetStats(new CharacterStats()
                {
                    Will = 20 + 10 * level,
                    Strength = 5 + 5 * level,
                    Posture = 10
                })
                .SetCreatingStrategy(new CreateIfCondition(() => enemy.DeathCount == 0))
                .SetLeftWeapon(Lib.Items.LightMace())
                ;
            area.Area.AddEnemy(enemy);
        }
    }
}

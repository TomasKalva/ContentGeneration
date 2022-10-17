using OurFramework.Characters.Items.ItemClasses;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using System;
using System.Collections.Generic;

namespace OurFramework.LevelDesignLanguage
{
    class EnemyMaker
    {
        public Func<int, CharacterStats> CharacterStats { get; }
        public IEnumerable<Func<WeaponItem>> Weapons { get; }
        public IEnumerable<Func<CharacterState>> Enemies { get; }
        public IEnumerable<Func<InteractiveObjectState>> Rewards { get; }

        public EnemyMaker(Func<int, CharacterStats> characterStats, IEnumerable<Func<WeaponItem>> weapons, IEnumerable<Func<CharacterState>> enemies, IEnumerable<Func<InteractiveObjectState>> rewards)
        {
            CharacterStats = characterStats;
            Weapons = weapons;
            Enemies = enemies;
            Rewards = rewards;
        }

        public Func<CharacterState> GetRandomEnemy(int level)
        {
            var weaponF = Weapons.GetRandom();
            var enemyF = Enemies.GetRandom();
            return () =>
            {
                var enemy = enemyF();
                return enemy
                    .SetStats(CharacterStats(level))
                    .SetLeftWeapon(weaponF())
                    .SetRightWeapon(weaponF())
                    .AddOnDeath(() =>
                    {
                        GameViewModel.ViewModel.PlayerState.Spirit += enemy.Health.Maximum;
                    })
                    .DropItem(Rewards.GetRandom());
            };
        }
    }
}

using OurFramework.Characters.SpellClasses;
using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

    public class SpellItems
    {
        Spells spells;
        VFXs vfxs;

        public SpellItems(Spells spells, VFXs vfxs)
        {
            this.spells = spells;
            this.vfxs = vfxs;
        }

        public IEnumerable<Func<ItemState>> AllSpellItems() =>
            new Func<ItemState>[]
            {
                    Fireball,
                    Triangle,
                    FlamesOfHeaven,
                    FlameOfHeaven,
                    PillarsOfHeaven,
                    ConsecratedGround,
                    Firefall,
                    HeavenlyFlameCloud,
                    Cloud,
                    WaveOfChaos,
                    FireBolt,
                    FlameBolt,
                    ChaosBolt,
                    CircleOfChaos,
                    SquareOfChaos,
                    Inferno,
                    Refreshment,
                    Replenishment,
            };

        public List<List<Func<ItemState>>> AllSpellsByPower() =>
            new List<List<Func<ItemState>>>()
                {
                    new List<Func<ItemState>>()
                    {
                        FireBolt,
                        Replenishment,
                    },
                    new List<Func<ItemState>>()
                    {
                        FlameBolt,
                        Fireball,
                        Cloud,
                        PillarsOfHeaven,
                        ConsecratedGround,
                        Refreshment,
                    },
                    new List<Func<ItemState>>()
                    {
                        ChaosBolt,
                        Firefall,
                        SquareOfChaos,
                        FlameOfHeaven,
                        HeavenlyFlameCloud,
                        Triangle,
                    },
                    new List<Func<ItemState>>()
                    {
                        CircleOfChaos,
                        FlamesOfHeaven,
                        Inferno,
                        WaveOfChaos,
                    },
                };

        public ItemState FireBolt()
            => new ItemState()
            {
                Name = "Fire Bolt",
                Description = "When the nature changed, fire bolts were among the first to notice due to their swiftness."
            }
            .OnUse(ch => spells.Bolt(vfxs.Lightning, Color.yellow, vfxs.LightningTexture, 0.6f, 7f,
                new DamageDealt(DamageType.Chaos, 10f + 5f * ch.Stats.Versatility))(ch));

        public ItemState FlameBolt()
            => new ItemState()
            {
                Name = "Flame Bolt",
                Description = "More powerfull version of fire bolt."
            }
            .OnUse(ch => spells.Bolt(vfxs.Lightning, Color.yellow, vfxs.LightningTexture, 0.8f, 7f,
                new DamageDealt(DamageType.Chaos, 13f + 6f * ch.Stats.Versatility))(ch));

        public ItemState ChaosBolt()
            => new ItemState()
            {
                Name = "Chaos Bolt",
                Description = "Made of pure chaos capable of piercing into any unsuspecting body."
            }
            .OnUse(ch => spells.Bolt(vfxs.Lightning, Color.yellow, vfxs.LightningTexture, 1f, 9f,
                new DamageDealt(DamageType.Chaos, 21f + 10f * ch.Stats.Versatility))(ch));

        public ItemState SquareOfChaos()
            => new ItemState()
            {
                Name = "Square of Chaos",
                Description = "Four flames, each representing one of the principial witches burnt for practicing the forbidden arts of chaos."
            }
            .OnUse(ch => spells.CircleBorder(vfxs.Fire, Color.yellow, vfxs.FireTexture, 2.5f, 4, 10f, 180f, ch.Agent.transform.forward.XZ(),
                new DamageDealt(DamageType.Chaos, 10f + 3f * ch.Stats.Versatility))(ch));

        public ItemState CircleOfChaos()
            => new ItemState()
            {
                Name = "Circle of Chaos",
                Description = ""
            }
            .OnUse(ch => spells.CircleBorder(vfxs.Fire, Color.yellow, vfxs.FireTexture, 2.5f, 24, 10f, 180f, ch.Agent.transform.forward.XZ(),
                new DamageDealt(DamageType.Chaos, 10f + 3f * ch.Stats.Versatility))(ch));

        public ItemState Inferno()
            => new ItemState()
            {
                Name = "Inferno",
                Description = "Let the chaos engulf your body."
            }
            .OnUse(ch => spells.CircleBorder(
                vfxF: () => vfxs.MovingCloud().SetHalfWidth(1.2f),
                color: Color.yellow,
                texture: vfxs.FireTexture,
                radius: 0.5f,
                sampleCount: 3,
                duration: 10f,
                halfArcSize: 180f,
                startDirection: ch.Agent.transform.forward.XZ(),
                damageDealt: new DamageDealt(DamageType.Chaos, 10f + 3f * ch.Stats.Versatility))(ch));

        public ItemState WaveOfChaos()
        {
            Func<Effect> arcMakerF = () =>
            {
                Vector3? userPosition = null; // Remember the position where user stood when casting 
                Vector2? arcDirection = null;
                int waveNumber = 0;
                return ch =>
                {
                    if (!userPosition.HasValue)
                    {
                        userPosition = ch.Agent.transform.position;
                        arcDirection = ch.Agent.transform.forward.XZ();
                    }

                    spells.CircleBorder(vfxs.Fire, Color.yellow, vfxs.FireTexture, userPosition.Value, 1.5f + waveNumber++ * 0.7f, 24 + 3 * waveNumber, 0.7f, 30f, arcDirection.Value,
                        new DamageDealt(DamageType.Chaos, 10f + 3f * ch.Stats.Versatility))(ch);
                };
            };

            Func<Effect> wavesF = () => spells.Periodically(arcMakerF(), 2f, 0.3f);

            return new ItemState()
            {
                Name = "Wave of Chaos",
                Description = "Chaos propagates at lazy pace rendering its victims unsuspecting of any disturbances."
            }
             .OnUse(ch => wavesF()(ch));
        }

        public ItemState Cloud()
            => new ItemState()
            {
                Name = "Cloud",
                Description = "Soothing cloud."
            }
            .OnUse(ch => spells.Cloud(() => vfxs.MovingCloud().SetHalfWidth(0.5f), Color.white, vfxs.WindTexture, 1f, 2f, 1000f, new DamageDealt(DamageType.Divine, 0f + 3f * ch.Stats.Versatility))(ch));

        public ItemState HeavenlyFlameCloud()
            => new ItemState()
            {
                Name = "Heavenly Flame Cloud",
                Description = "Burning cloud."
            }
            .OnUse(ch => spells.Cloud(() => vfxs.MovingCloud().SetHalfWidth(0.5f), Color.white, vfxs.FireTexture, 1f, 2f, 800f, new DamageDealt(DamageType.Divine, 15f + 5f * ch.Stats.Versatility))(ch));

        public ItemState Firefall()
            => new ItemState()
            {
                Name = "Firefall",
                Description = "."
            }
            .OnUse(ch => spells.Firefall(() => vfxs.MovingCloud().SetHalfWidth(2.0f), Color.yellow, vfxs.FireTexture, new DamageDealt(DamageType.Divine, 0f + 3f * ch.Stats.Versatility))(ch));

        public ItemState ConsecratedGround()
            => new ItemState()
            {
                Name = "Consecrated Ground",
                Description = "."
            }
            .OnUse(ch => spells.CircleArea(
                vfxF: vfxs.Fire,
                color: Color.white,
                texture: vfxs.FireTexture,
                center: ch.Agent.transform.position,
                radius: 4f,
                samplesCount: 10,
                damageDealt: new DamageDealt(DamageType.Chaos, 20f + 5f * ch.Stats.Versatility))(ch));

        public ItemState PillarsOfHeaven()
            => new ItemState()
            {
                Name = "Pillars of Heaven",
                Description = "Fragments of the original pillars that used to hold heaven safely in its proper place."
            }
            .OnUse(ch =>
                spells.CircleBorder(
                    vfxF: vfxs.Lightning,
                    color: Color.white,
                    texture: vfxs.WindTexture,
                    radius: 2.5f,
                    sampleCount: 8,
                    duration: 3f,
                    halfArcSize: 50f,
                    startDirection: ch.Agent.transform.forward.XZ(),
                    damageDealt: new DamageDealt(DamageType.Divine, 10f + 3f * ch.Stats.Versatility))(ch));

        public ItemState FlameOfHeaven()
            => new ItemState()
            {
                Name = "Flame of Heaven",
                Description = "Originally meant to heal rather than harm, Flame of Heaven now serves a new master."
            }
            .OnUse(ch =>
                spells.FlameOfHeaven(
                    vfxF: vfxs.Fire,
                    color: Color.white,
                    texture: vfxs.SmokeTexture,
                    forward: 1.3f,
                    scale: 4f,
                    radius: 0.01f,
                    samplesCount: 1,
                    damageDealt: new DamageDealt(DamageType.Divine, 10f + 3f * ch.Stats.Versatility))(ch));

        public ItemState FlamesOfHeaven()
            => new ItemState()
            {
                Name = "Flames of Heaven",
                Description = "Those who transcend now live in peace each captured within its own flame."
            }
            .OnUse(ch =>
                spells.FlameOfHeaven(
                    vfxF: vfxs.Fire,
                    color: Color.white,
                    texture: vfxs.SmokeTexture,
                    forward: 3f,
                    scale: 4f,
                    radius: 3f,
                    samplesCount: 3,
                    damageDealt: new DamageDealt(DamageType.Divine, 10f + 3f * ch.Stats.Versatility))(ch));

        public ItemState Triangle()
            => new ItemState()
            {
                Name = "Triangle",
                Description = "Only on special occassions does Chaos take on such a perfect shape."
            }
            .OnUse(ch => spells.TangentCircleBorder(
                vfxF: () => vfxs.MovingCloud().SetHalfWidth(0.3f),
                color: Color.yellow,
                texture: vfxs.FireTexture,
                radius: 0.7f,
                sampleCount: 3,
                duration: 10f,
                damageDealt: new DamageDealt(DamageType.Chaos, 10f + 3f * ch.Stats.Versatility))(ch));

        public ItemState Fireball()
            => new ItemState()
            {
                Name = "Fireball",
                Description = "Unstable in its very essence, this piece of fire is not held together by powers within our comprehension."
            }
            .OnUse(ch => spells.Bolt(vfxs.Fireball, Color.yellow, vfxs.FireTexture, 1f, 9f,
                new DamageDealt(DamageType.Chaos, 21f + 10f * ch.Stats.Versatility))(ch));

        public ItemState Refreshment()
            => new ItemState()
            {
                Name = "Refreshment",
                Description = "."
            }
            .OnUse(ch => spells.Refreshment(vfxs.Fire, new Color(0f, 0.7612882f, 1f, 1f), vfxs.LightningTexture, 1f + 0.5f * ch.Stats.Versatility)(ch));

        public ItemState Replenishment()
            => new ItemState()
            {
                Name = "Replenishment",
                Description = "."
            }
            .OnUse(ch => spells.Replenishment(5f + 1.5f * ch.Stats.Versatility)(ch));
    }

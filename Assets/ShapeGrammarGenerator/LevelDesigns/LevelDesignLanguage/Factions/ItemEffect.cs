using Assets.Util;
using ContentGeneration.Assets.UI.Model;
using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions.OccurenceManager;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions
{
    /*class ItemEffect
    {
        public 

        public abstract void Effect(CharacterState hitCharacter);
    }*/

    public delegate void Effect(CharacterState target);

    //public delegate Effect EffectByUser(CharacterState user);
    public delegate Selector SelectorByUser(CharacterState user);

    public delegate IEnumerable<CharacterState> SelectTargets();

    public delegate bool FinishedInTime(float deltaT);
    public delegate T ByTime<T>(float time);
    public delegate T ByUser<T>(CharacterState user);

    public class Selector
    {
        class Hit
        {
            public CharacterState Character;
            public float TimeUntilNextHit;

            public Hit(CharacterState character, float timeUntilNextHit)
            {
                Character = character;
                TimeUntilNextHit = timeUntilNextHit;
            }

            public bool TimeExpired => TimeUntilNextHit <= 0f;
        }

        HashSet<CharacterState> ImmuneCharacters { get; }
        IDistribution<float> TimeBetweenHits { get; }
        SelectTargets select;
        List<Hit> Hits;
        FinishedInTime finished;

        public Selector(IDistribution<float> timeBetweenHits, SelectTargets select, FinishedInTime finished)
        {
            Hits = new List<Hit>();
            ImmuneCharacters = new HashSet<CharacterState>();
            TimeBetweenHits = timeBetweenHits;
            this.select = select;
            this.finished = finished;
        }

        public void AddImmuneCharacter(CharacterState character)
        {
            ImmuneCharacters.Add(character);
        }

        void UpdateHits(float deltaT)
        {
            Hits.ForEach(hit => hit.TimeUntilNextHit -= deltaT);
            Hits.RemoveAll(hit => hit.Character.Agent == null || hit.TimeExpired);
        }

        bool CanBeHit(CharacterState characterState)
        {
            bool immune = ImmuneCharacters.Contains(characterState);
            bool hitAlready = Hits.Where(hit => hit.Character.Agent != null && hit.Character == characterState).Any();
            return !immune && !hitAlready;
        }

        public IEnumerable<CharacterState> Select(float deltaT)
        {
            UpdateHits(deltaT);
            var hit = select().Where(ch => CanBeHit(ch)).ToList();
            hit.ForEach(hitCh => 
                Hits.Add(new Hit(hitCh, TimeBetweenHits.Sample()))
                );
            return hit;
        }

        public bool Finished(float deltaT) => finished(deltaT);
    }

    public struct SelectorArgs
    {
        public Color Color { get; }
        public FlipbookTexture FlipbookTexture { get; }

        public SelectorArgs(Color color, FlipbookTexture flipbookTexture)
        {
            Color = color;
            FlipbookTexture = flipbookTexture;
        }
    }

    public delegate SelectorByUser SelectorByArgsByUser(SelectorArgs args);

    public class SelectorLibrary
    {
        Libraries lib;

        public SelectorLibrary(Libraries lib)
        {
            this.lib = lib;
        }

        public Selector ConstSelector(CharacterState target, float duration, IDistribution<float> timeBetweenHits)
        {
            var countdown = new CountdownTimer(duration);
            return new Selector(
                timeBetweenHits,
                () =>
                {
                    return target.ToEnumerable();
                },
                dt => countdown.Finished(dt)
            );
        }

        public SelectorByUser SelfSelector() =>
            ch => new Selector(
                new ConstDistr(1f),
                () =>
                {
                    return ch.ToEnumerable();
                },
                dt => true
            );

        public SelectorInitializator Initializator() => new SelectorInitializator();

        public delegate void SelectorInitializationOperation(CharacterState caster, Rigidbody selector);
        public class SelectorInitializator
        {
            /// <summary>
            /// These operations that will initialize the selector.
            /// </summary>
            List<SelectorInitializationOperation> initializationOperations;

            public SelectorInitializator()
            {
                this.initializationOperations = new List<SelectorInitializationOperation>();
            }

            public SelectorInitializator ConstPosition(Vector3 pos)
            {
                initializationOperations.Add((ch, s) => s.transform.position = pos);
                return this;
            }

            public SelectorInitializator FrontOfCharacter(float frontDist)
            {
                initializationOperations.Add((ch, s) => s.transform.position = ch.Agent.GetGroundPosition() + ch.Agent.movement.AgentForward * frontDist);
                return this;
            }

            public SelectorInitializator RightHandOfCharacter(float frontDist)
            {
                initializationOperations.Add((ch, s) => s.transform.position = ch.Agent.GetRightHandPosition() + ch.Agent.movement.AgentForward * frontDist + 0.2f * Vector3.down);
                return this;
            }

            public SelectorInitializator SetVelocity(Func<CharacterState, Vector3> directionF, float speed)
            {
                initializationOperations.Add((ch, s) => s.velocity = speed * directionF(ch));
                return this;
            }

            public SelectorInitializator RotatePitch(float angles)
            {
                initializationOperations.Add((ch, s) => s.transform.Rotate(new Vector3(angles, 0f, 0f)));
                return this;
            }

            public SelectorInitializator SetDirection(Vector3 direction)
            {
                initializationOperations.Add((ch, s) => s.transform.forward = direction);
                return this;
            }

            public SelectorInitializator Move(Vector3 direction)
            {
                initializationOperations.Add((ch, s) => s.transform.position = s.transform.position + direction);
                return this;
            }

            public SelectorInitializator Scale(float scale)
            {
                initializationOperations.Add((ch, s) => s.transform.localScale = new Vector3(scale, scale, scale));
                return this;
            }

            public void Initialize(CharacterState caster, Rigidbody selector)
            {
                initializationOperations.ForEach(op => op(caster, selector));
            }
        }

        public SelectorByArgsByUser GeometricSelector(Func<VFX> vfxF, float duration, SelectorInitializator selectorInitialization)
        {
            return args => ch =>
            {
                VFX vfx = vfxF();
                vfx.SetColor(args.Color);
                vfx.SetTexture(args.FlipbookTexture);

                var rb = vfx.gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.transform.forward = ch.Agent.transform.forward;

                selectorInitialization.Initialize(ch, rb);


                ColliderDetector collider = vfx.ColliderDetector;
                var countdown = new CountdownTimer(duration);
                var wallHitCountdown = new EventsCountdown(10);
                var ts = new GeometricTargetSelector(
                        vfx,
                        collider,
                        dt => countdown.Finished(dt) || wallHitCountdown.Finished(collider.Hits.SelectNN(hit => hit.gameObject).Where(go => go.layer == LayerMask.NameToLayer("StaticEnvironment")).Any())
                    );
                
                //vfx.transform.position = ch.Agent.transform.position;

                /*
                var movingBall = Libraries.GeometricSelectors.Ball()
                    .PutTo(ch.Agent.rightWeaponSlot)
                    .MoveDir(ch.Agent.movement.AgentForward)
                    .Speed()
                    .DestroyAtWallTouch()*/

                var selector = new Selector(
                    new ConstDistr(1f),
                    ts.SelectTargets,
                    dt => ts.Update(dt)
                );

                selector.AddImmuneCharacter(ch);
                return selector;
            };
        }

        public ByTime<SelectorByUser> WeaponSelector(ColliderDetector colliderDetector)
        {
            return duration => user =>
            {
                var countdown = new CountdownTimer(duration);
                var selector = new Selector(
                    new ConstDistr(10f),
                    () => colliderDetector.Hits.SelectNN(c => c?.GetComponentInParent<Agent>()?.CharacterState),
                    dt => countdown.Finished(dt)
                );
                selector.AddImmuneCharacter(user);
                return selector;
            };
        }
    }
    
    public class EffectLibrary
    {
        SelectorLibrary sel;

        public EffectLibrary(SelectorLibrary sel)
        {
            this.sel = sel;
        }

        public Effect Heal(float healing)
        {
            return ch => ch.Health += healing;
        }

        public Effect DamagePosture(float postureDamage)
        {
            return ch => ch.DamageTaken.AddDamage(postureDamage);
        }

        public Effect Damage(DamageDealt damage)
        {
            return ch => ch.TakeDamage(damage);
        }

        public Effect Push(Vector3 direction, float force)
        {
            return ch =>
            {
                var chAgent = ch.Agent;
                if (chAgent == null)
                    return;

                chAgent.movement.Impulse(force * direction);
            };
        }

        public ByUser<Effect> PushFrom(float force)
        {
            return user => ch =>
            {
                var userAgent = user.Agent;
                var chAgent = ch.Agent;
                if (userAgent == null || chAgent == null)
                    return;

                var direction = (chAgent.transform.position - userAgent.transform.position).normalized;
                chAgent.movement.Impulse(force * direction);
            };
        }

        public Effect GiveSpirit(float spirit)
        {
            return ch => ch.Spirit += spirit;
        }

        public Effect Bleed(float damagePerSecond, float timeS)
        {
            var tickLength = 0.1f;
            return ch => ch.World.CreateOccurence(
                    sel.ConstSelector(ch, timeS, new ConstDistr(tickLength)),
                    Damage(new DamageDealt(DamageType.Physical, damagePerSecond * tickLength))
            );
        }

        public Effect BoostStaminaRegen(float boostPerSecond, float timeS)
        {
            var tickLength = 0.1f;
            return ch => ch.World.CreateOccurence(
                    sel.ConstSelector(ch, timeS, new ConstDistr(tickLength)),
                    ch => ch.Stamina += boostPerSecond * tickLength
            );
        }

        public Effect RegenerateHealth(float boostPerSecond, float timeS)
        {
            var tickLength = 0.1f;
            return ch => ch.World.CreateOccurence(
                    sel.ConstSelector(ch, timeS, new ConstDistr(tickLength)),
                    Heal(boostPerSecond * tickLength)
            );
        }

        /// <summary>
        /// LevelConstructionEvent from function because it can potentialy contain level related state.
        /// </summary>
        public Effect StartQuestline(LevelConstructor levelConstructor, Func<LevelConstructionEvent> levelConstructionEventF)
        {
            return _ => levelConstructor.AddEvent(levelConstructionEventF());
        }
    }


    class Spells
    {
        EffectLibrary eff;
        SelectorLibrary sel;
        VFXs vfxs;

        public Spells(EffectLibrary eff, SelectorLibrary sel, VFXs vfxs)
        {
            this.eff = eff;
            this.sel = sel;
            this.vfxs = vfxs;
        }

        struct Point
        {
            public Vector3 Position;
            public Vector3 Normal;

            public Point(Vector3 position, Vector3 normal)
            {
                Position = position;
                Normal = normal;
            }
        }

        /// <summary>
        /// The center of the arc is on startDirection.
        /// </summary>
        IEnumerable<Point> EvenlySampleCircleBorder(Vector3 center, float radius, int samplesCount, float halfArcSizeDeg, Vector2 startDirection)
        {
            var startAngle = Mathf.Atan2(startDirection.y, startDirection.x);
            var arcSizeRad = halfArcSizeDeg * Mathf.Deg2Rad;
            return Enumerable.Range(0, samplesCount)
                .Select(i => (Mathf.PI * 2f * (i / (float)samplesCount) + 2 * Mathf.PI) % (2 * Mathf.PI))
                .Where(angle => angle < arcSizeRad || angle >= 2 * Mathf.PI - arcSizeRad)
                .Select(angle =>
                {
                    angle = angle + startAngle;
                    var cos = Mathf.Cos(angle);
                    var sin = Mathf.Sin(angle);
                    return new Point(center + radius * new Vector3(cos, 0f, sin), -new Vector3(cos, 0f, sin));
                });
        }

        IEnumerable<Vector3> EvenlySampleCircleArea(Vector3 center, float radius, int samplesCount)
        {
            return Enumerable.Range(0, samplesCount)
                .Select(_ => new Vector2(radius * Mathf.Sqrt(UnityEngine.Random.Range(0f, 1f)), 2 * Mathf.PI * UnityEngine.Random.Range(0f, 1f)))
                .Select(rTheta =>
                {
                    float r = rTheta.x;
                    float theta = rTheta.y;

                    var x = r * Mathf.Cos(theta);
                    var z = r * Mathf.Sin(theta);
                    return center + new Vector3(x, 0f, z);
                });
        }

        /// <summary>
        /// Shoots a bolt forward from character's hand. It deals damage upon impact.
        /// </summary>
        public Effect Bolt(Color color, FlipbookTexture texture, float scale, float speed, DamageDealt damageDealt)
        {
            return user => user.World.CreateOccurence(
                sel.GeometricSelector(vfxs.Lightning, 4f, sel.Initializator()
                    .RightHandOfCharacter(0f)
                    .SetVelocity(user => user.Agent.movement.AgentForward, speed)
                    .RotatePitch(-90f)
                    .Scale(scale)
                    )(new SelectorArgs(color, texture))(user),
                eff.Damage(damageDealt)
                );
        }

        /// <summary>
        /// Spawns the vfx in the given circle arc.
        /// </summary>
        public Effect CircleBorder(Func<VFX> vfxF, Color color, FlipbookTexture texture, Vector3 center, float radius, int sampleCount, float duration, float halfArcSize, Vector2 startDirection, DamageDealt damageDealt)
        {
            return user => EvenlySampleCircleBorder(center, radius, sampleCount, halfArcSize, startDirection)
                .ForEach(point => user.World.CreateOccurence(
                    sel.GeometricSelector(vfxF, duration, sel.Initializator()
                        .ConstPosition(point.Position)
                        .SetDirection(point.Normal) // face out of the circle center
                        )(new SelectorArgs(color, texture))(user),
                    eff.Damage(damageDealt)
                    )
                );
        }

        public Effect CircleBorder(Func<VFX> vfxF, Color color, FlipbookTexture texture, float radius, int sampleCount, float duration, float halfArcSize, Vector2 startDirection, DamageDealt damageDealt)
        {
            return user => CircleBorder(vfxF, color, texture, user.Agent.transform.position, radius, sampleCount, duration, halfArcSize, startDirection, damageDealt)(user);
        }

        public Effect Cloud(Func<VFX> vfxF, Color color, FlipbookTexture texture, float scale, float speed, float pushForce, DamageDealt damageDealt)
        {
            return user =>
            {
                var pushDirection = user.Agent.transform.forward;
                user.World.CreateOccurence(
                    sel.GeometricSelector(vfxF, 4f, sel.Initializator()
                        .FrontOfCharacter(1.3f)
                        .SetVelocity(user => user.Agent.movement.AgentForward, speed)
                        .Scale(scale)
                        )(new SelectorArgs(color, texture))(user),
                    eff.Damage(damageDealt),
                    eff.Push(pushDirection, pushForce)
                    );
            };
        }

        public Effect Firefall(Func<VFX> vfxF, Color color, FlipbookTexture texture, DamageDealt damageDealt)
        {
            return user =>
            {
                user.World.CreateOccurence(
                    sel.GeometricSelector(vfxF, 6f, sel.Initializator()
                        .FrontOfCharacter(1.3f)
                        .RotatePitch(-90)
                        .Move(1.5f * Vector3.up)
                        )(new SelectorArgs(color, texture))(user),
                    eff.Damage(damageDealt)
                    );
            };
        }

        public Effect CircleArea(Func<VFX> vfxF, Color color, FlipbookTexture texture, DamageDealt damageDealt)
        {
            return user => EvenlySampleCircleArea(user.Agent.transform.position, 4f, 10)
                .ForEach(pos =>
                user.World.CreateOccurence(
                    sel.GeometricSelector(vfxF, 6f, sel.Initializator()
                        .ConstPosition(pos)
                        )(new SelectorArgs(color, texture))(user),
                    eff.Damage(damageDealt)
                    )
            );
        }

        /// <summary>
        /// Periodically use effect for the given duration.
        /// passed to the effect.
        /// </summary>
        public Effect Periodically(Effect effect, float duration, float tickLength)
        {
            return ch => ch.World.CreateOccurence(
                    sel.ConstSelector(ch, duration, new ConstDistr(tickLength)),
                    effect
                    );
        }
}

    class SpellItems
    {
        Spells spells;
        VFXs vfxs;

        public SpellItems(Spells spells, VFXs vfxs)
        {
            this.spells = spells;
            this.vfxs = vfxs;
        }

        public ItemState FireBolt()
            => new ItemState()
            {
                Name = "Fire Bolt",
                Description = "When the nature changed, fire bolts were among the first to notice due to their swiftness."
            }
            .OnUse(ch => spells.Bolt(Color.yellow, vfxs.LightningTexture, 0.6f, 7f,
                new DamageDealt(DamageType.Chaos, 10f + 5f * ch.Stats.Versatility))(ch));

        public ItemState FlameBolt()
            => new ItemState()
            {
                Name = "Flame Bolt",
                Description = "More powerfull version of fire bolt."
            }
            .OnUse(ch => spells.Bolt(Color.yellow, vfxs.LightningTexture, 0.8f, 7f,
                new DamageDealt(DamageType.Chaos, 13f + 6f * ch.Stats.Versatility))(ch));

        public ItemState ChaosBolt()
            => new ItemState()
            {
                Name = "Chaos Bolt",
                Description = "Made of pure chaos capable of piercing into any unsuspecting body."
            }
            .OnUse(ch => spells.Bolt(Color.yellow, vfxs.LightningTexture, 1f, 9f,
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
            .OnUse(ch => spells.CircleBorder(() => vfxs.MovingCloud().SetHalfWidth(1.2f), Color.yellow, vfxs.FireTexture, 0.5f, 3, 10f, 180f, ch.Agent.transform.forward.XZ(),
                new DamageDealt(DamageType.Chaos, 10f + 3f * ch.Stats.Versatility))(ch));

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
            .OnUse(ch => spells.CircleArea(vfxs.Fire, Color.white, vfxs.FireTexture, new DamageDealt(DamageType.Chaos, 20f + 5f * ch.Stats.Versatility))(ch));

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

    }

    /*
    class OccurenceLibrary
    {
        EffectLibrary eff;

        public Occurence ScaledHealing(Func<Selector> selector)
        {
            var power = 5f;
            return new Occurence(selector(), eff.Heal(power));
        }


    }*/

    class OccurenceManager
    {
        List<Occurence> CurrentOccurences { get; set; }
        //HashSet<Occurence> FinishedOccurences { get; }

        public OccurenceManager()
        {
            CurrentOccurences = new List<Occurence>();
            //FinishedOccurences = new HashSet<Occurence>();
        }

        /// <summary>
        /// Something that happens inside of the world.
        /// </summary>
        class Occurence
        {
            Selector selector;
            Effect[] effects;

            public Occurence(Selector selector, params Effect[] effects)
            {
                this.selector = selector;
                this.effects = effects;
            }

            /// <summary>
            /// Returns true iff the occurence has finished.
            /// </summary>
            public bool Update(float deltaT)
            {
                var affectedCharacters = selector.Select(deltaT);
                affectedCharacters.ForEach(character =>
                    effects.ForEach(effect => effect(character)));

                return selector.Finished(deltaT);
            }
        }

        public void CreateOccurence(Selector selector, params Effect[] effects)
        {
            CurrentOccurences.Add(new Occurence(selector, effects));
        }

        public void Update(float deltaT)
        {
            // todo: somehow optimize this to avoid allocations each update
            CurrentOccurences = CurrentOccurences.Where(occurence => !occurence.Update(deltaT)).ToList();
            /*CurrentOccurences.ForEach(occurence =>
            {
                if (occurence.Update(deltaT))
                {
                    //FinishedOccurences.Add(occurence);
                }
            });*/
            //CurrentOccurences.RemoveAll(occurence => FinishedOccurences.Contains(occurence));
            //FinishedOccurences.Clear();
        }


    }
}

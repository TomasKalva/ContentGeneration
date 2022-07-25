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
                initializationOperations.Add((ch, s) => s.position = pos);
                return this;
            }

            public SelectorInitializator FrontOfCharacter(float frontDist)
            {
                initializationOperations.Add((ch, s) => s.position = ch.Agent.GetGroundPosition() + ch.Agent.movement.AgentForward * frontDist);
                return this;
            }

            public SelectorInitializator RightHandOfCharacter(float frontDist)
            {
                initializationOperations.Add((ch, s) => s.position = ch.Agent.GetRightHandPosition() + ch.Agent.movement.AgentForward * frontDist + 0.2f * Vector3.down);
                return this;
            }

            public SelectorInitializator Move(Func<CharacterState, Vector3> directionF, float speed)
            {
                initializationOperations.Add((ch, s) => s.velocity = speed * directionF(ch));
                return this;
            }

            public SelectorInitializator RotatePitch(float angles)
            {
                initializationOperations.Add((ch, s) => s.transform.Rotate(new Vector3(angles, 0f, 0f)));
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

        
        public ByUser<Effect> Push(float force)
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

        IEnumerable<Vector3> EvenlySampleCircle(float radius, int samplesCount)
        {
            return Enumerable.Range(0, samplesCount + 1)
                .Select(i => Mathf.PI * 2f * (i / (float)samplesCount))
                .Select(angle => radius * new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
        }

        /// <summary>
        /// Shoots a bolt forward from character's hand. It deals damage upon impact.
        /// </summary>
        public Action<CharacterState> Bolt(Color color, FlipbookTexture texture, float scale, float speed, DamageDealt damageDealt)
        {
            return user => user.World.CreateOccurence(
                sel.GeometricSelector(vfxs.Lightning, 4f, sel.Initializator()
                    .RightHandOfCharacter(0f)
                    .Move(user => user.Agent.movement.AgentForward, speed)
                    .RotatePitch(-90f)
                    .Scale(scale)
                    )(new SelectorArgs(color, texture))(user),
                eff.Damage(damageDealt)
                );
        }

        /*
        public ByUser<Occurence> ShowCircle(VFX vfx, Color color, FlipbookTexture texture, float radius, DamageDealt damageDealt)
        {
            return user => new Occurence(
                sel.GeometricSelector(vfxs.Lightning, 4f, sel.Initializator()
                    .FrontOfCharacter(0f)
                    .RotatePitch(-90f)
                    .Scale(scale)
                    )(new SelectorArgs(color, texture))(user),
                eff.Damage(damageDealt)
                );
        }*/
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

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
    /*class ItemEffect
    {
        public 

        public abstract void Effect(CharacterState hitCharacter);
    }*/

    public delegate void Effect(CharacterState target);

    public delegate Effect EffectByUser(CharacterState user);
    public delegate Selector SelectorByUser(CharacterState user);

    public delegate IEnumerable<CharacterState> SelectTargets();

    public delegate bool FinishedInTime(float deltaT);

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
            Hits.RemoveAll(hit => hit.TimeExpired);
        }

        bool CanBeHit(CharacterState characterState)
        {
            bool immune = ImmuneCharacters.Contains(characterState);
            bool hitAlready = Hits.Where(hit => hit.Character == characterState).Any();
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

    /// <summary>
    /// Something that happens inside of the world.
    /// </summary>
    public class Occurence
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



        public delegate void InitializeSelector(CharacterState caster, Rigidbody selector);

        public InitializeSelector ConstPosition(Vector3 pos) 
        {
            return (ch, s) => s.position = pos;
        }

        public InitializeSelector FrontOfCharacter(float frontDist)
        {
            return (ch, s) => s.position = ch.Agent.GetGroundPosition() + ch.Agent.movement.AgentForward * frontDist;
        }

        public InitializeSelector RightHandOfCharacter(float frontDist)
        {
            return (ch, s) => s.position = ch.Agent.GetRightHandPosition() + ch.Agent.movement.AgentForward * frontDist + 0.2f * Vector3.down;
        }

        public InitializeSelector Move(Func<CharacterState, Vector3> directionF, float speed)
        {
            return (ch, s) => s.velocity = speed * directionF(ch);
        }

        public SelectorByArgsByUser GeometricSelector(Func<VFX> vfxF, float duration, InitializeSelector initSelector)
        {
            return args => ch =>
            {
                VFX vfx = vfxF();
                vfx.SetColor(args.Color);
                vfx.SetTexture(args.FlipbookTexture);

                var rb = vfx.gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                initSelector(ch, rb);

                rb.transform.forward = ch.Agent.transform.forward;

                ColliderDetector collider = vfx.ColliderDetector;
                var countdown = new CountdownTimer(duration);
                var wallHitCountdown = new EventsCountdown(10);
                var ts = new GeometricTargetSelector(
                        vfx,
                        collider,
                        dt => countdown.Finished(dt) || wallHitCountdown.Finished(collider.Hit.SelectNN(hit => hit.gameObject).Where(go => go.layer == LayerMask.NameToLayer("StaticEnvironment")).Any())
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
                //selector.AddImmuneCharacter(ch);
                return selector;
            };
        }

        /*
        public SelectorByUserByArgs GeometricStarSelector(Func<VFX> vfxF, int raysCount)
        {
            return args => ch =>
            {
                VFX vfx = vfxF();
                vfx.SetColor(args.Color);
                vfx.SetTexture(args.FlipbookTexture);
                
                Enumerable.Range(0, raysCount).ForEach(i =>
                {

                }
                var rb = vfx.gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                initSelector(ch, rb);

                rb.transform.forward = ch.Agent.transform.forward;

                ColliderDetector collider = vfx.ColliderDetector;
                var ts = new GeometricTargetSelector(
                        vfx,
                        collider,
                        t => false
                    );

                var selector = new Selector(
                    new ConstDistr(1f),
                    ts.SelectTargets,
                    dt => ts.Update(dt)
                );
                selector.AddImmuneCharacter(ch);
                return selector;
            };
        }*/


        /*
        public SelectorByUserByArgs FireSelector()
        {
            return args => ch =>
            {
                FireVFX fire = lib.VFXs.Fire();
                fire.SetColor(args.Color);
                fire.SetTexture(args.FlipbookTexture);

                ColliderDetector collider = fire.ColliderDetector;
                var ts = new GeometricTargetSelector(
                        fire,
                        collider,
                        t => false
                    );
                fire.transform.position = ch.Agent.transform.position;

                return new Selector(
                    new ConstDistr(1f),
                    ts.SelectTargets,
                    dt => ts.Update(dt)
                );
            };
        }

        public SelectorByUserByArgs MovingCloudSelector()
        {
            return args => ch =>
            {
                MovingCloudVFX movingCloud = lib.VFXs.MovingCloud();
                movingCloud.SetColor(args.Color);
                movingCloud.SetTexture(args.FlipbookTexture);
                ColliderDetector collider = movingCloud.ColliderDetector;
                var ts = new GeometricTargetSelector(
                        movingCloud,
                        collider,
                        t => false
                    );
                movingCloud.transform.position = ch.Agent.transform.position;

                return new Selector(
                    new ConstDistr(1f),
                    ts.SelectTargets,
                    dt => ts.Update(dt)
                );
            };
        }*/


        //public SelectTargets 

        //public SelectorByUser BallSelector() =>
        //    throw new NotImplementedException();
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
            return ch => ch.PostureManager.DamagePosture(postureDamage);
        }

        public Effect Damage(float damage)
        {
            return ch => ch.TakeDamage(damage);
        }

        /*
        public EffectByUser Push(float force)
        {
            return user => ch => 
        }*/

        public Effect GiveSpirit(float spirit)
        {
            return ch => ch.Prop.Spirit += spirit;
        }

        public Effect Bleed(float damagePerSecond, float timeS)
        {
            var tickLength = 0.1f;
            return ch => ch.World.AddOccurence(
                new Occurence(
                    sel.ConstSelector(ch, timeS, new ConstDistr(tickLength)),
                    Damage(damagePerSecond * tickLength)
                )
            );
        }

        public Effect BoostStaminaRegen(float boostPerSecond, float timeS)
        {
            var tickLength = 0.1f;
            return ch => ch.World.AddOccurence(
                new Occurence(
                    sel.ConstSelector(ch, timeS, new ConstDistr(tickLength)),
                    ch => ch.Stamina += boostPerSecond * tickLength
                )
            );
        }

        public Effect RegenerateHealth(float boostPerSecond, float timeS)
        {
            var tickLength = 0.1f;
            return ch => ch.World.AddOccurence(
                new Occurence(
                    sel.ConstSelector(ch, timeS, new ConstDistr(tickLength)),
                    Heal(boostPerSecond * tickLength)
                )
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

    delegate EffectByUser EffectByFactionEnvironmentByUser(FactionEnvironment factionEnv);

    class FactionScalingEffectLibrary
    {
        public List<Annotated<EffectByFactionEnvironmentByUser>> EffectsByUser { get; }

        float EffectPower(FactionEnvironment factionEnv, CharacterState user)
        {
            var affinity = factionEnv.FactionManifestation.Faction.Affinity;
            var manifProgress = factionEnv.FactionManifestation.Progress;
            var vers = user.Prop.Versatility;
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
                FromPower("Damage", "damages", p => eff.Damage(10f + 5f * p)),
                FromPower("Give spirit", "gives spirit to", p => eff.GiveSpirit(10f + 20f * p)),
                FromPower("Bleed", "applies bleeding to", p => eff.Bleed(5f + 2f * p, 2f)),
                FromPower("Boost stamina regeneration", "boosts stamina regeneration to", p => eff.BoostStaminaRegen(5f + 2f * p, 2f)),
                FromPower("Regenerate health", "regenerates health to", p => eff.RegenerateHealth(5f + 2f * p, 2f)),
            };
        }
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

        public void AddOccurence(Occurence occurence)
        {
            CurrentOccurences.Add(occurence);
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

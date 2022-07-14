using ContentGeneration.Assets.UI.Model;
using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class Selector
    {
        struct Hit
        {
            public CharacterState Character;
            public float TimeUntilNextHit;

            public bool TimeExpired => TimeUntilNextHit <= 0f;
        }

        HashSet<CharacterState> ImmuneCharacters { get; }
        IDistribution<float> TimeBetweenHits { get; }
        SelectTargets select;
        List<Hit> Hits;
        Func<bool> finished;

        public Selector(IDistribution<float> timeBetweenHits, SelectTargets select, Func<bool> finished)
        {
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
            return select().Where(ch => CanBeHit(ch));
        }

        public bool Finished() => finished();
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

            return selector.Finished();
        }
    }

    class SelectorLibrary
    {
        public SelectorByUser SelfSelector() => 
            ch => new Selector(
                new ConstDistr(1f),
                () =>
                {
                    return ch.ToEnumerable();
                },
                () => true
            );

        public SelectorByUser BallSelector() => 
            ch => new Selector(
                new ConstDistr(1f),
                () =>
                {
                    throw new NotImplementedException();
                },
                () => throw new NotImplementedException()
            );

    }

    class EffectLibrary
    {
        public Effect Heal(float healing)
        {
            return ch => ch.Prop.Health += healing;
        }

        public Effect Damage(float damage)
        {
            return ch => ch.Prop.Health -= damage;
        }

        public Effect GiveSpirit(float spirit)
        {
            return ch => ch.Prop.Spirit += spirit;
        }
    }

    delegate EffectByUser EffectByFactionByUser(Faction faction);

    class FactionScalingEffectLibrary
    {
        public List<EffectByFactionByUser> EffectsByUser { get; }

        float EffectPower(Faction faction, CharacterState user)
        {
            var affinity = faction.Affinity;
            var vers = user.Prop.Versatility;
            return affinity + vers;
        }

        EffectByFactionByUser FromPower(Func<float, Effect> powerToEffect)
        {
            return faction => user =>
            {
                var power = EffectPower(faction, user);
                return powerToEffect(power);
            };
        }

        public FactionScalingEffectLibrary(EffectLibrary eff)
        {
            EffectsByUser = new List<EffectByFactionByUser>()
            {
                FromPower(p => eff.Heal(5f * p)),
                FromPower(p => eff.Damage(5f * p)),
                FromPower(p => eff.GiveSpirit(20f * p)),
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
        List<Occurence> CurrentOccurences { get; }
        HashSet<Occurence> FinishedOccurences { get; }

        public OccurenceManager()
        {
            CurrentOccurences = new List<Occurence>();
            FinishedOccurences = new HashSet<Occurence>();
        }

        public void AddOccurence(Occurence occurence)
        {
            CurrentOccurences.Add(occurence);
        }

        public void Update(float deltaT)
        {
            CurrentOccurences.ForEach(occurence =>
            {
                if (occurence.Update(deltaT))
                {
                    FinishedOccurences.Add(occurence);
                }
            });
            CurrentOccurences.RemoveAll(occurence => FinishedOccurences.Contains(occurence));
            FinishedOccurences.Clear();
        }
    }
}

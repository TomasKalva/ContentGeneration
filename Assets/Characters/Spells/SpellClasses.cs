using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using ContentGeneration.Assets.UI.Model;
using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Characters.SpellClasses
{
    public delegate void Effect(CharacterState target);

    //public delegate Effect EffectByUser(CharacterState user);
    public delegate Selector SelectorByUser(CharacterState user);

    public delegate IEnumerable<CharacterState> SelectTargets();

    public delegate bool FinishedInTime(float deltaT);
    public delegate T ByTime<T>(float time);
    public delegate T ByUser<T>(CharacterState user);
    public delegate T ByTransform<T>(Vector3 position, Vector3 direction);

    public class Selector
    {
        /// <summary>
        /// Describes time elapsed since the hit of given character.
        /// </summary>
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

}

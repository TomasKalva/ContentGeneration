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

    delegate void Effect(CharacterState characterState);

    class Selector
    {
        struct Hit
        {
            public CharacterState Character;
            public float TimeUntilNextHit;

            public bool TimeExpired => TimeUntilNextHit <= 0f;
        }

        HashSet<CharacterState> ImmuneCharacters { get; }
        IDistribution<float> TimeBetweenHits { get; }
        Func<IEnumerable<CharacterState>> select;
        List<Hit> Hits;

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

        public void Update(float deltaT)
        {
            var affectedCharacters = selector.Select(deltaT);
            affectedCharacters.ForEach(character =>
                effects.ForEach(effect => effect(character)));
        }
    }
}

using Assets.Characters.SpellClasses;
using Assets.LevelDesignLanguage;
using System;
using UnityEngine;
using Util;

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
            return ch => ch.Poise -= postureDamage;
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
            return _ => levelConstructor.AddNecessaryEvent(levelConstructionEventF());
        }
    }

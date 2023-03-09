using Animancer;
using OurFramework.Gameplay.RealWorld;
using System.Collections.Generic;
using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
    public class AnimatedAct : Act
    {
        [SerializeField]
        protected ClipTransition anim;


        protected float timeElapsed;

        protected List<MovementConstraint> MovementContraints { get; private set; }

        public void Initialize(Agent agent)
        {
            // Set speed so that the animation takes duration seconds
            var speed = anim.Clip.length / Duration;
            anim.Speed = speed;
            MovementContraints = new List<MovementConstraint>();
        }

        public sealed override void StartAct(Agent agent)
        {
            Initialize(agent);
            timeElapsed = 0f;
            agent.CharacterState.Stamina -= cost;

            OnStart(agent);
        }

        public virtual void OnStart(Agent agent) { }

        public override bool UpdateAct(Agent agent, float dt)
        {
            timeElapsed += dt;
            OnUpdate(agent);
            return timeElapsed >= Duration;
        }

        public virtual void OnUpdate(Agent agent) { }

        public override void EndAct(Agent agent)
        {

        }

        protected void PlayAnimation(Agent agent)
        {
            agent.animancerAnimator.Play(anim, 0.1f);
        }

        protected AnimancerState PlayIfNotActive(Agent agent, float transitionTime)
        {
            if (!agent.animancerAnimator.IsPlaying(anim))
            {
                return agent.animancerAnimator.Play(anim, transitionTime);
            }
            return agent.animancerAnimator.States.Current;
        }

        protected void SetupMovementConstraints(Agent agent, params MovementConstraint[] movementConstraints)
        {
            MovementContraints.Clear();
            MovementContraints.AddRange(movementConstraints);
            agent.movement.AddMovementConstraints(movementConstraints);
        }
    }
}

using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
    public class Move : AnimatedAct
    {
        [SerializeField]
        protected float speed = 3f;

        [SerializeField]
        protected float animMetersPerSecond = 1f;

        protected Vector2 direction;

        public Vector2 Direction
        {
            get => direction.normalized;
            set => direction = value;
        }

        public bool SetDirection { get; set; } = true;

        float Speed => speed * speedMultiplier;

        float speedMultiplier = 1f;
        public void SetSpeedMultiplier(float speedMultitplier)
        {
            this.speedMultiplier = speedMultitplier;
            BaseDuration = animMetersPerSecond / Speed;
        }

        private void Awake()
        {
            BaseDuration = animMetersPerSecond / Speed;
        }

        public override bool UpdateAct(Agent agent, float dt)
        {
            PlayIfNotActive(agent, 0.1f);

            agent.movement.Move(Direction, Speed, SetDirection);
            return true;
        }
    }
}

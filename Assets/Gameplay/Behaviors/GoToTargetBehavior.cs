using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
    /// <summary>
    /// Agent goes to the target transform.
    /// </summary>
    public class GoToTargetBehavior : Behavior
    {
        Transform targetPoint;

        protected Vector3 TargetPoint => targetPoint ? targetPoint.position : Vector3.zero;

        float maxDistance;

        public GoToTargetBehavior(float maxDistance)
        {
            this.maxDistance = maxDistance;
        }

        bool ShouldGoToTarget(Agent agent)
        {
            return agent.CanMove && Vector3.Distance(agent.transform.position, TargetPoint) > maxDistance;
        }

        public override bool CanEnter(Agent agent)
        {
            return ShouldGoToTarget(agent);
        }

        public override int Priority(Agent agent) => 2;

        public override void Enter(Agent agent)
        {
            targetPoint = GameObject.FindGameObjectWithTag("Player").transform;
        }

        public override bool Update(Agent agent)
        {
            Vector3 direction = TargetPoint - agent.movement.transform.position;
            var moveDirection = new Vector2(direction.x, direction.z);
            agent.Run(moveDirection);

            return !ShouldGoToTarget(agent);
        }
    }
}
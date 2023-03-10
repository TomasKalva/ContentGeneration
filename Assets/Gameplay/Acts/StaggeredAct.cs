using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
    /// <summary>
    /// The agent gets starggered and can't move for a moment.
    /// </summary>
    public class StaggeredAct : AnimatedAct
    {
        public Vector3 PushForce { get; set; }

        public StaggeredAct()
        {
            actName = "Staggered";
            type = ActType.IDLE;
            priority = 0;
        }

        public override void OnStart(Agent agent)
        {
            PlayAnimation(agent);

            agent.movement.Impulse(PushForce);

            agent.movement.VelocityUpdater = new DontChangeVelocityUpdater(duration);
        }
    }
}
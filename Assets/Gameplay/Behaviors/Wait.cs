using OurFramework.Util;
using System;
using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
    /// <summary>
    /// The agent goes to the position and waits.
    /// </summary>
    public class Wait : Behavior
    {
        Func<Agent, Vector3> WaitPosition { get; }

        Func<Agent, bool> ShouldWait { get; }

        float MinDistance => 1.5f;

        public Wait(Func<Agent, bool> shouldWait, Func<Agent, Vector3> waitPosition)
        {
            ShouldWait = shouldWait;
            WaitPosition = waitPosition;
        }

        public override bool CanEnter(Agent agent)
        {
            return ShouldWait(agent);
        }

        public override int Priority(Agent agent) => 100;

        public override void Enter(Agent agent)
        {
        }

        public override bool Update(Agent agent)
        {
            var targetPoint = WaitPosition(agent);
            var toTarget = targetPoint - agent.transform.position;
            if (toTarget.magnitude > MinDistance &&
                agent.CanMove)
            {
                var direction = toTarget.XZ();
                agent.Run(direction);
            }

            return !ShouldWait(agent);
        }
    }
}

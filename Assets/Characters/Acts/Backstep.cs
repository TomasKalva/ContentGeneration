using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
    public class Backstep : AnimatedAct
    {
        [SerializeField, Curve(0f, 0f, 1f, 30f, true)]
        AnimationCurve speedF;

        public override void OnStart(Agent agent)
        {
            PlayAnimation(agent);

            var backDirection = -agent.movement.AgentForward;
            Direction3F directionF = () => backDirection;
            agent.movement.VelocityUpdater = new CurveVelocityUpdater(speedF, duration, directionF);

            SetupMovementConstraints(agent,
                new VelocityInDirection(directionF),
                new TurnToDirection(() => -backDirection.XZ())
                );
        }

        public override void EndAct(Agent agent)
        {
            MovementContraints.ForEach(con => con.Finished = true);
        }
    }
}

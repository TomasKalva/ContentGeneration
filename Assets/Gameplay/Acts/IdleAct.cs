
namespace OurFramework.Gameplay.RealWorld
{
    public class IdleAct : AnimatedAct
    {
        public IdleAct()
        {
            actName = "Idle";
            type = ActType.IDLE;
            priority = -100;
        }

        public override bool UpdateAct(Agent agent, float dt)
        {
            PlayIfNotActive(agent, 0.1f);

            return true;
        }
    }
}
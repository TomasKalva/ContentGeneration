
namespace OurFramework.Gameplay.RealWorld
{
    /// <summary>
    /// Move backward.
    /// </summary>
    public class MoveBack : Move
    {
        public override bool UpdateAct(Agent agent, float dt)
        {
            PlayIfNotActive(agent, 0.1f);

            agent.movement.Move(Direction, speed, false);
            agent.Turn(-Direction);
            return true;
        }
    }
}

namespace OurFramework.Gameplay.RealWorld
{
    /// <summary>
    /// The behaviour tells the agent how to act. Next behaviour is selected based on priority.
    /// </summary>
    public abstract class Behavior
    {
        public abstract bool CanEnter(Agent agent);
        /// <summary>
        /// Number in [0, 10]. The higher the more the agent wants to do this behaviour.
        /// </summary>
        public abstract int Priority(Agent agent);
        public virtual void Enter(Agent agent) { }
        /// <summary>
        /// Returs true if should exit.
        /// </summary>
        public virtual bool Update(Agent agent) => true;
        public virtual void Exit(Agent agent) { }
    }
}

using OurFramework.Gameplay.State;

namespace OurFramework.Gameplay.RealWorld
{
    /// <summary>
    /// The agent uses an item.
    /// </summary>
    public class UseItem : AnimatedAct
    {
        public Inventory Inventory { private get; set; }

        public override void OnStart(Agent agent)
        {
            PlayAnimation(agent);
        }

        public override void EndAct(Agent agent)
        {
            Inventory.UseItem();
        }
    }
}
using OurFramework.Gameplay.Data;

namespace OurFramework.Gameplay.RealWorld
{
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
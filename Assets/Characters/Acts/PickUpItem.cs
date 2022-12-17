
namespace OurFramework.Gameplay.RealWorld
{
    public class PickUpItem : AnimatedAct
    {
        public PhysicalItemState PhysicalItem { get; set; }

        public override void OnStart(Agent agent)
        {
            PlayAnimation(agent);
        }

        public override void EndAct(Agent agent)
        {
            PhysicalItem.PickUpItem(agent);
        }
    }
}

using OurFramework.Gameplay.RealWorld;

namespace OurFramework.Gameplay.Data
{
    public class PhysicalItemState : InteractiveObjectState<InteractiveObject>
    {
        public ItemState Item { get; set; }

        public void PickUpItem(Agent agent)
        {
            var added = agent.CharacterState.AddItem(Item);
            if (added)
            {
                Item = null;
                World.RemoveItem(this);
            }
        }
    }
}

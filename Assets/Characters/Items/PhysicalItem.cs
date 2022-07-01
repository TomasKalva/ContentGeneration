using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicalItemState : InteractiveObjectState<InteractiveObject>
{
    public ItemState Item { get; set; }
    bool beingPickedUp = false;

    public override void Interact(Agent agent)
    {
        if (!beingPickedUp)
        {
            agent.PickUpItem(this);
            beingPickedUp = true;
        }
    }

    public void PickUpItem(Agent agent)
    {
        var added = agent.CharacterState.AddItem(Item);
        if (added)
        {
            var world = GameObject.Find("World").GetComponent<World>();
            world.RemoveItem(this);
            //Object.Destroy(InteractiveObject.gameObject);
        }
    }
}

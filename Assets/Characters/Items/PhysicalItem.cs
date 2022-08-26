using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicalItemState : InteractiveObjectState<InteractiveObject>
{
    public ItemState Item { get; set; }
    
    public void PickUpItem(Agent agent)
    {
        var added = agent.CharacterState.AddItem(Item);
        if (added)
        {
            World.RemoveItem(this);
        }
    }
}

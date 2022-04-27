using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
public class PhysicalItem : InteractiveObject
{
public ItemState Item { get; set; }
bool beingPickeUp = false;

protected override void Initialize()
{
    //Item = GetComponent<ItemRef>().Item;
}

protected override void InteractLogic(Agent agent)
{
    if (!beingPickeUp)
    {
        agent.PickUpItem(this);
        beingPickeUp = true;
    }
}

public void PickUpItem(Agent agent)
{
    var added = agent.CharacterState.AddItem(Item);
    if (added)
    {
        Destroy(gameObject);
    }
}
}
*/

public class PhysicalItemState : InteractiveObjectState<InteractiveObject>
{
    public ItemState Item { get; set; }
    bool beingPickeUp = false;

    public override void Interact(Agent agent)
    {
        if (!beingPickeUp)
        {
            agent.PickUpItem(this);
            beingPickeUp = true;
        }
    }

    public void PickUpItem(Agent agent)
    {
        var added = agent.CharacterState.AddItem(Item);
        if (added)
        {
            Object.Destroy(InteractiveObject.gameObject);
        }
    }
}

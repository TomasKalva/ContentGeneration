using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ItemRef))]
public class PhysicalItem : InteractiveObject
{
    public ItemState Item { get; private set; }

    protected override void Initialize()
    {
        Item = GetComponent<ItemRef>().Item;
    }

    protected override void InteractLogic(Agent agent)
    {
        agent.PickUpItem(this);
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

using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalItem : InteractiveObject
{
    [SerializeField]
    public ItemState item;

    protected override void InteractLogic(Agent agent)
    {
        agent.PickUpItem(this);
    }

    public void PickUpItem(Agent agent)
    {
        var added = agent.CharacterState.AddItem(item);
        if (added)
        {
            Destroy(gameObject);
        }
    }
}

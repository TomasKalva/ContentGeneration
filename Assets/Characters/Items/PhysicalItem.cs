using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalItem : InteractiveObject
{
    [SerializeField]
    ItemState Item;

    protected override void InteractLogic(Agent agent)
    {
        var added = agent.CharacterState.AddItem(Item);
        if (added)
        {
            Destroy(gameObject);
        }
    }
}

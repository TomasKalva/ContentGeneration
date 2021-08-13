using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemRef : MonoBehaviour
{
    public abstract ItemState Item { get; }
}

public abstract class ItemRef<ItemT> : ItemRef where ItemT : ItemState
{
    [SerializeField]
    ItemT item;

    public override ItemState Item => item;
}


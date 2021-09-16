using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class WeaponItem : ItemState
{
    public WeaponItem(string name, string description, Transform realObject)
    {
        Name = name;
        Description = description;
        RealObject = realObject;
    }
}

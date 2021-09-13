using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class BlueIchorEssence : ItemState
{
    public BlueIchorEssence()
    {
        Name = "Blue Ichor Essence";
        Description = "Very blue";
    }

    public override void OnUpdate(CharacterState character)
    {
        character.Will += ExtensionMethods.PerFixedSecond(2f);
    }
}

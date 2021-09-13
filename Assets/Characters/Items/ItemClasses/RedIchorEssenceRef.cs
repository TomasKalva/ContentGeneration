using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class RedIchorEssence : ItemState
{
    public RedIchorEssence()
    {
        Name = "Red Ichor Essence";
        Description = "Super ichor";
    }

    public override void OnUpdate(CharacterState character)
    {
        character.Will += ExtensionMethods.PerFixedSecond(2f);
    }
}

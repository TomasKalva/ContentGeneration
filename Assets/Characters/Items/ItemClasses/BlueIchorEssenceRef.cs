using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BlueIchorEssenceRef : ItemRef<BlueIchorEssence> { }

[Serializable]
public class BlueIchorEssence : ItemState
{
    public override void OnUpdate(CharacterState character)
    {
        character.Will += ExtensionMethods.PerFixedSecond(2f);
    }
}

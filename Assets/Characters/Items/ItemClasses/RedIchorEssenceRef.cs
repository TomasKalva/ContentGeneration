using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RedIchorEssenceRef : ItemRef<RedIchorEssence> { }

[Serializable]
public class RedIchorEssence : ItemState
{
    public override void OnUpdate(CharacterState character)
    {
        character.Will += ExtensionMethods.PerFixedSecond(2f);
    }
}

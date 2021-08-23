using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FreeWillRef : ItemRef<FreeWill> { }

[Serializable]
public class FreeWill : ItemState
{
    public override void OnUpdate(CharacterState character)
    {
        character.Will += ExtensionMethods.PerFixedSecond(2f);
    }
}
// Red Ichor Essence
// Once noble blood of the Great One now serves as beverage for those in need

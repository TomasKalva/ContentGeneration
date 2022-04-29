using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class FreeWill : ItemState
{
    public FreeWill()
    {
        Name = "Free Will";
        Description = "Costs nothing";
        OnUpdateDelegate = character =>
        {
            if (character.Agent != null &&
                !character.Agent.acting.Busy)
            {
                character.Will += ExtensionMethods.PerFixedSecond(2f);
            }
        };
    }
}
// Red Ichor Essence
// Once noble blood of the Great One now serves as beverage for those in need

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
                character.Stamina += ExtensionMethods.PerFixedSecond(2f);
            }
        };
    }
}

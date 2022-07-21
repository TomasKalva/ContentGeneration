using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class WeaponItem : ItemState
{
    float BaseDamage { get; }
    public Weapon Weapon { get; }
    Effect Effect { get; }

    public WeaponItem(string name, string description, Weapon weapon, Effect effect)
    {
        Name = name;
        Description = description;
        Weapon = weapon.AddEffect(effect);
        Effect = effect;
        OnUseDelegate =
            character =>
            {
                Debug.Log($"{Name} is being used");
                character.SetItemToSlot(SlotType.RightWeapon, this);
            };
    }
}

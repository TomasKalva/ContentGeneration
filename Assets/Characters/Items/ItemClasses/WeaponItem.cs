using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class WeaponItem : ItemState
{
    float Damage { get; }
    Weapon Weapon { get; }

    public WeaponItem(string name, string description, Transform realObject)
    {
        Name = name;
        Description = description;
        RealObject = realObject;
        OnUseDelegate =
            character =>
            {
                Debug.Log($"{Name} is being used");
                character.SetItemToSlot(SlotType.RightWeapon, this);
            };
    }

    public void DealDamage(CharacterState owner)
    {
        /*owner.World.AddOccurence(
            new Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions.Occurence(
                Weapon.GetSelector(),
                ))*/
    }
}

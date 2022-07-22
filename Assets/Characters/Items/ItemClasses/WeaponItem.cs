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
    List<Effect> Effects { get; set; }

    public WeaponItem(string name, string description, Weapon weapon, Effect effect)
    {
        Name = name;
        Description = description;
        Weapon = weapon; 
        weapon.WeaponItem = this;

        Effects = new List<Effect>();
        AddEffect(effect);
        
        OnUseDelegate =
            character =>
            {
                Debug.Log($"{Name} is being used");
                character.SetItemToSlot(SlotType.RightWeapon, this);
            };
    }

    public WeaponItem AddEffect(Effect effect)
    {
        Effects.Add(effect);
        return this;
    }

    public void DealDamage(CharacterState owner, float damageDuration)
    {
        owner.World.AddOccurence(
            new Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions.Occurence(
                Weapon.HitSelectorByDuration(damageDuration)(owner),
                Effects.ToArray()
                )
            );
    }
}

using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class WeaponItem : ItemState
{
    float BaseDamage { get; }
    public Weapon Weapon { get; }
    ByUser<Effect>[] BaseEffects { get; set; }
    List<ByUser<Effect>> UpgradeEffects { get; set; }

    public WeaponItem(string name, string description, Weapon weapon, IEnumerable<ByUser<Effect>> baseEffects)
    {
        Name = name;
        Description = description;
        Weapon = weapon; 
        weapon.WeaponItem = this;

        BaseEffects = baseEffects.ToArray();
        UpgradeEffects = new List<ByUser<Effect>>();

        OnUseDelegate =
            character =>
            {
                Debug.Log($"{Name} is being used");
                character.SetItemToSlot(SlotType.RightWeapon, this);
            };
    }

    public WeaponItem AddUpgradeEffect(ByUser<Effect> effect)
    {
        UpgradeEffects.Add(effect);
        return this;
    }

    public void DealDamage(CharacterState owner, float damageDuration)
    {
        owner.World.AddOccurence(
            new Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions.Occurence(
                Weapon.HitSelectorByDuration(damageDuration)(owner),
                BaseEffects.Select(effectByUser => effectByUser(owner)).ToArray()
                )
            );
    }
}

public enum DamageType
{
    Physical,
    Chaos,
    Dark,
    Divine
}

public struct DamageDealt
{
    public DamageType Type;
    public float Amount;

    public DamageDealt(DamageType type, float amount)
    {
        Type = type;
        Amount = amount;
    }
}

public class Defense
{
    public DamageType Type;
    public float ReductionPercentage;

    public Defense(DamageType type, float reductionPercentage)
    {
        Type = type;
        ReductionPercentage = reductionPercentage;
    }

    public DamageDealt DamageAfterDefense(DamageDealt incomingDamage) 
    {
        return incomingDamage.Type == Type ?
            // Reduce damage if it has the same type
            new DamageDealt(incomingDamage.Type, incomingDamage.Amount* (1f - 0.01f * ReductionPercentage)) :
            // Return the same damage otherwise
            incomingDamage;
    }
}
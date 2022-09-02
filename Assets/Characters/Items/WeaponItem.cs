using Assets.Characters.Items.ItemClasses;
using Assets.Characters.SpellClasses;
using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using ContentGeneration.Assets.UI.Model;
using ShapeGrammar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


namespace Assets.Characters.Items.ItemClasses
{
    public class WeaponItem : EquipmentItem<Weapon>
    {
        public Weapon Weapon
        {
            get
            {
                if (_cachedEquipment == null)
                {
                    _cachedEquipment = EquipmentMaker();
                    _cachedEquipment.WeaponItem = this;
                }
                return _cachedEquipment;
            }
        }

        ByUser<Effect>[] BaseEffects { get; set; }
        List<ByUser<Effect>> UpgradeEffects { get; set; }

        public WeaponItem(string name, string description, GeometryMaker<Weapon> weaponMaker, IEnumerable<ByUser<Effect>> baseEffects) : base(name, description, weaponMaker)
        {
            BaseEffects = baseEffects.ToArray();
            UpgradeEffects = new List<ByUser<Effect>>();
        }

        public WeaponItem AddUpgradeEffect(ByUser<Effect> effect)
        {
            UpgradeEffects.Add(effect);
            return this;
        }

        public void DealDamage(CharacterState owner, float damageDuration)
        {
            owner.World.CreateOccurence(
                    Weapon.HitSelectorByDuration(damageDuration)(owner),
                    BaseEffects.Concat(UpgradeEffects).Select(effectByUser => effectByUser(owner)).ToArray()
                );
        }
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
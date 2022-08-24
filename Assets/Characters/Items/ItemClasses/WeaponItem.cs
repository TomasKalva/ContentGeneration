using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using ContentGeneration.Assets.UI.Model;
using ShapeGrammar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class EquipmentItem<EquipmentT> : ItemState where EquipmentT : Equipment
{
    protected GeometryMaker<EquipmentT> WeaponMaker { get; }
    protected EquipmentT _cachedEquipment { get; set; }

    public EquipmentItem(string name, string description, GeometryMaker<EquipmentT> equipmentMaker)
    {
        Name = name;
        Description = description;
        WeaponMaker = equipmentMaker;
        //equipmentMaker.WeaponItem = this;
    }
}

public class AccessoryItem : EquipmentItem<Accessory>
{
    public Accessory Accessory
    {
        get
        {
            if (_cachedEquipment == null)
            {
                _cachedEquipment = WeaponMaker();
                _cachedEquipment.AccessoryItem = this;
            }
            return _cachedEquipment;
        }
    }

    public AccessoryItem(string name, string description, GeometryMaker<Accessory> accessoryMaker) : base(name, description, accessoryMaker)
    {
    }
}

public class MaterialItem : ItemState
{
    public Material Material { get; }

    public MaterialItem(string name, string description, Material material)
    {
        Name = name;
        Description = description;
        Material = material;
    }
}

public class WeaponItem : EquipmentItem<Weapon>
{
    public Weapon Weapon 
    {
        get
        {
            if (_cachedEquipment == null)
            {
                _cachedEquipment = WeaponMaker();
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
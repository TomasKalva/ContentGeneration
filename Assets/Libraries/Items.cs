using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using OurFramework.Gameplay.Data;

namespace OurFramework.Libraries
{
    public class Items : ScriptableObject
    {
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Items")]
        public static void CreateMyAsset()
        {
            Items asset = ScriptableObject.CreateInstance<Items>();

            string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Items.asset");
            AssetDatabase.CreateAsset(asset, name);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
#endif

        [SerializeField]
        Weapons weapons;

        [SerializeField]
        Accessories accessories;

        [SerializeField]
        Materials materials;

        EffectLibrary Effects;

        public void SetLibraries(EffectLibrary effects)
        {
            Effects = effects;
        }

        /// <summary>
        /// Good push force values between 1 and 10.
        /// </summary>
        public IEnumerable<ByUser<Effect>> BaseWeaponEffects(ByUser<float> damageScaling, ByUser<float> pushForceScaling)
        {
            yield return ch => Effects.Damage(new DamageDealt(DamageType.Physical, damageScaling(ch)));
            yield return ch => Effects.PushFrom(100f * pushForceScaling(ch))(ch);
        }

        public WeaponItem SculptureClub() => new WeaponItem("Sculpture Club", "Made of idk stone", weapons.SculptureClub,
            BaseWeaponEffects(ch => 11 + 2 * ch.Stats.Strength, ch => 5f))
            .SetWearable(SlotType.Weapon) as WeaponItem;

        public WeaponItem MayanKnife() => new WeaponItem("Mayan Knife", "Every Mayan has one", weapons.MayanKnife,
            BaseWeaponEffects(ch => 11 + 2 * ch.Stats.Strength, ch => 1f))
            .SetWearable(SlotType.Weapon) as WeaponItem;

        public WeaponItem MayanSword() => new WeaponItem("Mayan Sword", "Like a knife but bigger", weapons.MayanSword,
            BaseWeaponEffects(ch => 11 + 2 * ch.Stats.Strength, ch => 1f))
            .SetWearable(SlotType.Weapon) as WeaponItem;

        public WeaponItem Scythe() => new WeaponItem("Scythe", "Harvesting tool", weapons.Scythe,
            BaseWeaponEffects(ch => 11 + 2 * ch.Stats.Strength, ch => 1f))
            .SetWearable(SlotType.Weapon) as WeaponItem;

        public WeaponItem Mace() => new WeaponItem("Mace", "Mace", weapons.Mace,
            BaseWeaponEffects(ch => 11 + 2 * ch.Stats.Strength, ch => 1f))
            .SetWearable(SlotType.Weapon) as WeaponItem;

        public WeaponItem Katana() => new WeaponItem("Katana", "Katana", weapons.Katana,
            BaseWeaponEffects(ch => 11 + 2 * ch.Stats.Strength, ch => 1f))
            .SetWearable(SlotType.Weapon) as WeaponItem;

        public IEnumerable<Func<WeaponItem>> AllBasicWeapons() => new List<Func<WeaponItem>>()
    {
        SculptureClub,
        MayanKnife,
        MayanSword,
        Scythe,
        Mace,
        Katana,
    };

        public WeaponItem LightMace() => new WeaponItem("Light Mace", "Mace imbued with fire.", weapons.LightMace,
            BaseWeaponEffects(ch => 15 + 2 * ch.Stats.Strength + ch.Stats.Versatility, ch => 1f))
            .SetWearable(SlotType.Weapon) as WeaponItem;

        public IEnumerable<Func<WeaponItem>> GoodWeaponsForEnemies() => new List<Func<WeaponItem>>()
    {
        SculptureClub,
        MayanSword,
        Mace,
        Katana,
    };

        public AccessoryItem Ring() => new AccessoryItem("Ring", "Circle", accessories.Ring)
            .SetWearable(SlotType.Wrist) as AccessoryItem;

        public AccessoryItem TwoRings() => new AccessoryItem("Two Ring", "Two is more than one.", accessories.TwoRings)
            .SetWearable(SlotType.Wrist) as AccessoryItem;

        public AccessoryItem Watches() => new AccessoryItem("Watches", "Circle", accessories.Watches)
            .SetWearable(SlotType.Wrist) as AccessoryItem;

        public AccessoryItem Handcuff() => new AccessoryItem("Handcuff", "Circle", accessories.Handcuff)
            .SetWearable(SlotType.Wrist) as AccessoryItem;

        public AccessoryItem Nails() => new AccessoryItem("Nails", "Circle", accessories.Nails)
            .SetWearable(SlotType.Wrist) as AccessoryItem;

        public AccessoryItem HairRubber() => new AccessoryItem("Hair Rubber", "Circle", accessories.HairRubber)
            .SetWearable(SlotType.Wrist) as AccessoryItem;

        public IEnumerable<Func<AccessoryItem>> AllWristItems() => new List<Func<AccessoryItem>>()
    {
        Ring,
        TwoRings,
        Watches,
        Handcuff,
        Nails,
        HairRubber,
    };

        public AccessoryItem Circle() => new AccessoryItem("Circle", "Circle", accessories.Circle)
            .SetWearable(SlotType.Head) as AccessoryItem;

        public AccessoryItem Crown() => new AccessoryItem("Crown", "Circle", accessories.Crown)
            .SetWearable(SlotType.Head) as AccessoryItem;

        public AccessoryItem Eggs() => new AccessoryItem("Eggs", "Circle", accessories.Eggs)
            .SetWearable(SlotType.Head) as AccessoryItem;

        public AccessoryItem ExtraHead() => new AccessoryItem("Extra Head", "Circle", accessories.ExtraHead)
            .SetWearable(SlotType.Head) as AccessoryItem;

        public AccessoryItem TowerHorns() => new AccessoryItem("Tower Horns", "Circle", accessories.TowerHorns)
            .SetWearable(SlotType.Head) as AccessoryItem;

        public IEnumerable<Func<AccessoryItem>> AllHeadItems() => new List<Func<AccessoryItem>>()
    {
        Circle,
        Crown,
        Eggs,
        ExtraHead,
        TowerHorns,
    };


        public MaterialItem StoneSkin() => new MaterialItem("Stone Skin", "Circle", materials.stone)
            .OnEquip(ch => ch.Stats.Versatility += 10)
            .OnUnequip(ch => ch.Stats.Versatility -= 10)
            .SetWearable(SlotType.Skin) as MaterialItem;

        public MaterialItem BrickSkin() => new MaterialItem("Brick Skin", "Circle", materials.bricks)
            .OnEquip(ch => ch.Stats.Strength += 10)
            .OnUnequip(ch => ch.Stats.Strength -= 10)
            .SetWearable(SlotType.Skin) as MaterialItem;

        public MaterialItem WaterSkin() => new MaterialItem("Water Skin", "Circle", materials.water)
            .OnEquip(ch => ch.Stats.Will += 10)
            .OnUnequip(ch => ch.Stats.Will -= 10)
            .SetWearable(SlotType.Skin) as MaterialItem;

        public MaterialItem TiledSkin() => new MaterialItem("Tiled Skin", "Circle", materials.tiles)
            .OnEquip(ch => ch.Stats.Resistances += 10)
            .OnUnequip(ch => ch.Stats.Resistances -= 10)
            .SetWearable(SlotType.Skin) as MaterialItem;

        public MaterialItem WoodenSkin() => new MaterialItem("Wooden Skin", "Circle", materials.wood)
            .OnEquip(ch => ch.Stats.Endurance += 10)
            .OnUnequip(ch => ch.Stats.Endurance -= 10)
            .SetWearable(SlotType.Skin) as MaterialItem;

        public IEnumerable<Func<MaterialItem>> AllSkinItems() => new List<Func<MaterialItem>>()
    {
        StoneSkin,
        BrickSkin,
        WaterSkin,
        TiledSkin,
        WoodenSkin,
    };

        public ItemState Dew() => NewItem("Dew", "Originating from the morning garden, Dew keeps its position of a great refereshment.")
            .SetStackable(1)
            .OnUse(Effects.RegenerateHealth(8f, 5f));

        public ItemState HoneyBall() => NewItem("Honey Ball", "Sweet honey can heal wounds.")
            .SetStackable(1)
            .OnUse(Effects.Heal(25f));

        public ItemState VibrantMemory() => NewItem("Vibrant Memory", "A memory blurring the border between life and death. Can be relived.")
            .SetStackable(1)
            .OnUse(ch => ch.AddItem(AllBasicWeapons().GetRandom()()));

        public ItemState FadingMemory() => NewItem("Fading Memory", "A short lived memory that does not indicate anything worthwile. Recalling hurts.")
            .SetStackable(1)
            .OnUse(ch =>
            {
                Effects.Damage(new DamageDealt(DamageType.Dark, 15f))(ch);
                ch.AddItem(VibrantMemory());
            });

        public ItemState Smile() => NewItem("Smile", "Smiling is one of the fundamental practices of Eternal Life movement. Has strong beneficial effects, but should be used with caution.")
            .SetStackable(1)
            .OnUse(ch =>
            {
                Effects.Heal(1000f)(ch);
                Effects.RegenerateHealth(5f, 60f);
            });

        public ItemState Coconut() => NewItem("Coconut", "What hides inside is not just a tasty snack.")
            .SetStackable(1)
            .OnUse(ch =>
            {
                ch.AddItem(AllWristItems().Concat(AllHeadItems()).GetRandom()());
                Effects.BoostStaminaRegen(2f, 20f)(ch);
            });

        public IEnumerable<Func<ItemState>> MiscellaneousItems() => new List<Func<ItemState>>()
    {
        Dew,
        HoneyBall,
    };


        public IEnumerable<Func<ItemState>> RareItems() => new List<Func<ItemState>>()
    {
        Smile,
        FadingMemory,
        Coconut,
    };

        public AccessoryItem FreeWill() => new AccessoryItem("Free Will", "Costs nothing", accessories.Circle)
            .OnUpdate(
            character =>
                {
                    if (character.Agent != null &&
                        !character.Agent.acting.Busy)
                    {
                        character.Stamina += ExtensionMethods.PerFixedSecond(2f);
                    }
                }
            )
            .SetWearable(SlotType.Head) as AccessoryItem;

        public ItemState NewItem(string name, string description) =>
            new ItemState()
            {
                Name = name,
                Description = description,
            };
    }
}

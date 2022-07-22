using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

    EffectLibrary Effects;

    public void SetLibraries(EffectLibrary effects)
    {
        Effects = effects;
    }

    public ItemState FreeWill() => new FreeWill();

    /// <summary>
    /// Good push force values between 1 and 10.
    /// </summary>
    public IEnumerable<ByUser<Effect>> BaseWeaponEffects(ByUser<float> damageScaling, ByUser<float> pushForceScaling)
    {
        yield return ch => Effects.Damage(new DamageDealt(DamageType.Physical, damageScaling(ch)));
        yield return ch => Effects.Push(100f * pushForceScaling(ch))(ch);
    }

    public ItemState SculptureClub() => new WeaponItem("Sculpture Club", "Made of idk stone", weapons.SculptureClub(), 
        BaseWeaponEffects(ch => 10 + 2 * ch.Stats.Strength, ch => 5f));

    public ItemState MayanKnife() => new WeaponItem("Mayan Knife", "Every Mayan has one", weapons.MayanKnife(),
        BaseWeaponEffects(ch => 10 + 2 * ch.Stats.Strength, ch => 1f));

    public ItemState MayanSword() => new WeaponItem("Mayan Sword", "Like a knife but bigger", weapons.MayanSword(),
        BaseWeaponEffects(ch => 10 + 2 * ch.Stats.Strength, ch => 1f));

    public ItemState Scythe() => new WeaponItem("Scythe", "Harvesting tool", weapons.Scythe(),
        BaseWeaponEffects(ch => 10 + 2 * ch.Stats.Strength, ch => 1f));

    public ItemState Mace() => new WeaponItem("Mace", "Mace", weapons.Mace(),
        BaseWeaponEffects(ch => 10 + 2 * ch.Stats.Strength, ch => 1f));

    public ItemState Katana() => new WeaponItem("Katana", "Katana", weapons.Katana(),
        BaseWeaponEffects(ch => 10 + 2 * ch.Stats.Strength, ch => 10f));

    public ItemState NewItem(string name, string description) => 
        new ItemState() 
        { 
            Name = name, 
            Description = description,
        };
}

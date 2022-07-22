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

    public IEnumerable<ByUser<Effect>> BaseWeaponEffects()
    {
        yield return ch => Effects.Damage(new Damage(DamageType.Physical, 10 + 2 * ch.Stats.Strength));
    }

    public ItemState SculptureClub() => new WeaponItem("Sculpture Club", "Made of idk stone", weapons.SculptureClub(), BaseWeaponEffects());
    public ItemState MayanKnife() => new WeaponItem("Mayan Knife", "Every Mayan has one", weapons.MayanKnife(), BaseWeaponEffects());
    //public ItemState Fireball() => new WeaponItem("Fireball", "It's a fireball", weapons.Fireball().transform);
    public ItemState MayanSword() => new WeaponItem("Mayan Sword", "Like a knife but bigger", weapons.MayanSword(), BaseWeaponEffects());
    public ItemState Scythe() => new WeaponItem("Scythe", "Harvesting tool", weapons.Scythe(), BaseWeaponEffects());
    public ItemState Mace() => new WeaponItem("Mace", "Mace", weapons.Mace(), BaseWeaponEffects());
    public ItemState Katana() => new WeaponItem("Katana", "Katana", weapons.Katana(), BaseWeaponEffects());

    public ItemState NewItem(string name, string description) => 
        new ItemState() 
        { 
            Name = name, 
            Description = description,
        };
}

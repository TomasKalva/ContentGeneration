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
    SelectorLibrary Selectors;

    public void SetLibraries(SelectorLibrary selectors, EffectLibrary effects)
    {
        Selectors = selectors;
        Effects = effects;
    }

    public ItemState FreeWill() => new FreeWill();

    public ItemState SculptureClub() => new WeaponItem("Sculpture Club", "Made of idk stone", weapons.SculptureClub().transform);
    public ItemState MayanKnife() => new WeaponItem("Mayan Knife", "Every Mayan has one", weapons.MayanKnife().transform);
    //public ItemState Fireball() => new WeaponItem("Fireball", "It's a fireball", weapons.Fireball().transform);
    public ItemState MayanSword() => new WeaponItem("Mayan Sword", "Like a knife but bigger", weapons.MayanSword().transform);
    public ItemState Scythe() => new WeaponItem("Scythe", "Harvesting tool", weapons.Scythe().transform);
    public ItemState Mace() => new WeaponItem("Mace", "Mace", weapons.Mace().transform);
    public ItemState Katana() => new WeaponItem("Katana", "Katana", weapons.Katana().transform);

    public ItemState NewItem(string name, string description) => 
        new ItemState() 
        { 
            Name = name, 
            Description = description, 
            // todo: add some default object model
            RealObject = weapons.Katana().transform 
        };
}

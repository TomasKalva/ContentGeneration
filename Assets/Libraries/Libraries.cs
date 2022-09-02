using Assets.Libraries;
using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Libraries : ScriptableObject
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/Libraries")]
    public static void CreateMyAsset()
    {
        Libraries asset = ScriptableObject.CreateInstance<Libraries>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Libraries.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
#endif

    public Items Items;
    public Enemies Enemies;
    public InteractiveObjects InteractiveObjects;
    public Objects Objects;
    public Weapons Weapons;
    public VFXs VFXs;
    public GeometricSelectors GeometricSelectors;

    public EffectLibrary Effects;
    public SelectorLibrary Selectors;
    public Spells Spells;
    public SpellItems SpellItems;

    public void Initialize()
    {
        Selectors = new SelectorLibrary(this);
        Effects = new EffectLibrary(Selectors);
        Weapons.SetLibraries(Selectors);
        Items.SetLibraries(Effects);
        Spells = new Spells(Effects, Selectors, VFXs);
        SpellItems = new SpellItems(Spells, VFXs);
    }
}

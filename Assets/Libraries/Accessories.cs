using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Accessories : ScriptableObject
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/Accessories")]
    public static void CreateMyAsset()
    {
        Accessories asset = ScriptableObject.CreateInstance<Accessories>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Accessories.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
#endif

    [SerializeField]
    Accessory ring;

    [SerializeField]
    Accessory twoRings;

    [SerializeField]
    Accessory watches;

    [SerializeField]
    Accessory handcuff;

    [SerializeField]
    Accessory nails;

    [SerializeField]
    Accessory hairRubber;

    Accessory CreateAccessory(Accessory prefab) => Instantiate(prefab);

    public Accessory Ring() => CreateAccessory(ring);
    public Accessory TwoRings() => CreateAccessory(twoRings);
    public Accessory Watches() => CreateAccessory(watches);
    public Accessory Handcuff() => CreateAccessory(handcuff);
    public Accessory Nails() => CreateAccessory(nails);
    public Accessory HairRubber() => CreateAccessory(hairRubber);
}

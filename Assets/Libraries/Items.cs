using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Items : ScriptableObject
{
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

    [SerializeField]
    Weapons weapons;

    public ItemState BlueIchorEssence() => new BlueIchorEssence();
    public ItemState RedIchorEssence() => new RedIchorEssence();
    public ItemState FreeWill() => new FreeWill();

    public ItemState SculptureClub() => new WeaponItem("Sculpture Club", "Made of idk stone", weapons.sculptureClub.transform);
    public ItemState MayanKnife() => new WeaponItem("Mayan Knife", "Every Mayan has one", weapons.mayanKnife.transform);
    public ItemState MayanSword() => new WeaponItem("Mayan Sword", "Like a knife but bigger", weapons.mayanSword.transform);
}

using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Weapons : ScriptableObject
{
    [MenuItem("Assets/Create/Weapons")]
    public static void CreateMyAsset()
    {
        Weapons asset = ScriptableObject.CreateInstance<Weapons>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Weapons.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    [SerializeField]
    Weapon sculptureClub;

    [SerializeField]
    Weapon mayanKnife;

    [SerializeField]
    Weapon mayanSword;

    public Weapon MayanSword()
    {
        var sword = Instantiate(mayanSword);
        sword.Damage = 12f;
        return sword;
    }

    public Weapon MayanKnife()
    {
        var knife = Instantiate(mayanKnife);
        knife.Damage = 5f;
        return knife;
    }

    public Weapon SculptureClub()
    {
        var club = Instantiate(sculptureClub);
        club.Damage = 22f;
        return club;
    }
}

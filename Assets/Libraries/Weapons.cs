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

    public Weapon sculptureClub;

    public Weapon mayanKnife;

    public Weapon mayanSword;
}

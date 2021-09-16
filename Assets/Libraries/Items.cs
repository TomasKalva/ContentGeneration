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

    public ItemState BlueIchorEssence() => new BlueIchorEssence();
    public ItemState RedIchorEssence() => new RedIchorEssence();
    public ItemState FreeWill() => new FreeWill();
}

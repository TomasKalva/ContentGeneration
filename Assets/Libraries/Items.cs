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

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/NewScripableObject.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    [SerializeField]
    PhysicalItem physicalItemPrefab;

    PhysicalItem CreateItem(ItemState itemState)
    {
        var newPhysicalItem = Instantiate(physicalItemPrefab);
        newPhysicalItem.Item = itemState;
        return newPhysicalItem;
    }

    public PhysicalItem BlueIchorEssence() => CreateItem(new BlueIchorEssence());
    public PhysicalItem RedIchorEssence() => CreateItem(new RedIchorEssence());
    public PhysicalItem FreeWill() => CreateItem(new FreeWill());
}

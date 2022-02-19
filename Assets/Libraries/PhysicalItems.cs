using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// DEPRECATED!!!
/// </summary>
public class PhysicalItems : ScriptableObject
{
    [MenuItem("Assets/Create/PhysicalItems")]
    public static void CreateMyAsset()
    {
        PhysicalItems asset = ScriptableObject.CreateInstance<PhysicalItems>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/NewScripableObject.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
    
    [SerializeField]
    InteractiveObject physicalItemPrefab;

    InteractiveObject CreateItem(ItemState itemState)
    {
        return null;
        /*
        var newPhysicalItem = Instantiate(physicalItemPrefab);
        newPhysicalItem.Item = itemState;
        return newPhysicalItem;
        */
    }

    public InteractiveObject BlueIchorEssence() => CreateItem(new BlueIchorEssence());
    public InteractiveObject RedIchorEssence() => CreateItem(new RedIchorEssence());
    public InteractiveObject FreeWill() => CreateItem(new FreeWill());
}

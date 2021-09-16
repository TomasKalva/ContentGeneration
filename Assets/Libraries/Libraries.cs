using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Libraries : ScriptableObject
{
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

    public Items Items;
    public Enemies Enemies;
    public InteractiveObjects InteractiveObjects;
    public Modules Modules;
    public Objects Objects;
    public PhysicalItems PhysicalItems;
    public Weapons Weapons;
}

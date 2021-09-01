using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Style : ScriptableObject
{
    [MenuItem("Assets/Create/Style")]
    public static void CreateMyAsset()
    {
        Style asset = ScriptableObject.CreateInstance<Style>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Style.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    [SerializeField]
    Transform wall;

    public Transform GetObject(ObjectType objectType)
    {
        return wall;
    }
}

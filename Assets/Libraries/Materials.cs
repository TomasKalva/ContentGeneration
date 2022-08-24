using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Materials : ScriptableObject
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/Materials")]
    public static void CreateMyAsset()
    {
        Materials asset = ScriptableObject.CreateInstance<Materials>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Materials.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
#endif

    [SerializeField]
    public Material stone;

    [SerializeField]
    public Material bricks;

    [SerializeField]
    public Material water;

    [SerializeField]
    public Material tiles;

    [SerializeField]
    public Material wood;
}
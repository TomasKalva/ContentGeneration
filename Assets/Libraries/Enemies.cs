using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Enemies : ScriptableObject
{
    [MenuItem("Assets/Create/Enemies")]
    public static void CreateMyAsset()
    {
        Enemies asset = ScriptableObject.CreateInstance<Enemies>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/NewScripableObject.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    public HumanAgent human;
    public SculptureAgent sculpture;
    public BlobAgent blob;
    public MayanAgent mayan;

}

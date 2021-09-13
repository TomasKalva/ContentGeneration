using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Styles : ScriptableObject
{
    [MenuItem("Assets/Create/Styles")]
    public static void CreateMyAsset()
    {
        Styles asset = ScriptableObject.CreateInstance<Styles>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Styles.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    [SerializeField]
    public Style gothic;

    [SerializeField]
    public Style greek;

    [SerializeField]
    public Style mayan;

    [SerializeField]
    public Style garden;
}

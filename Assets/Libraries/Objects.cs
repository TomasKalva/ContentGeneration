using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Objects : ScriptableObject
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/Objects")]
    public static void CreateMyAsset()
    {
        Objects asset = ScriptableObject.CreateInstance<Objects>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Objects.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
#endif

    //public Transform spawnPoint;
    public Transform farmer;
}
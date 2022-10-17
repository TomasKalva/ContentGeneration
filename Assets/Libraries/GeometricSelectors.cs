using UnityEditor;
using UnityEngine;

public class GeometricSelectors : ScriptableObject
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/GeometricSelectors")]
    public static void CreateMyAsset()
    {
        GeometricSelectors asset = ScriptableObject.CreateInstance<GeometricSelectors>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/GeometricSelectors.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
#endif

    [SerializeField]
    Libraries lib;
    /*
    public FireVFX Fire()
    {
        return Instantiate(fireVFX);
    }*/
}
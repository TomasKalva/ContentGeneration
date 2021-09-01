using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    Transform notExistingObj;

    [SerializeField]
    ObjectStyle[] objectStyles;

    public Transform GetObject(ObjectType objectType)
    {
        var objStyle = objectStyles.Where(objSt => objSt.objectType == objectType).FirstOrDefault();
        var obj = objStyle != null ? objStyle.obj : notExistingObj;
        return Instantiate(obj);
    }
}

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
    ObjectStyle<ObjectType>[] objectStyles;

    [SerializeField]
    ObjectStyle<CharacterType>[] characterStyles;

    public Transform GetObject<T>(ObjectStyle<T>[] objectStyles, Transform defaultT, T objectType)
    {
        var objStyle = objectStyles.Where(objSt => objSt.objectType.Equals(objectType)).FirstOrDefault();
        var obj = objStyle != null ? objStyle.obj : defaultT;
        return obj;
    }

    public Transform GetObject(ObjectType objectType)
    {
        /*
        var objStyle = objectStyles.Where(objSt => objSt.objectType == objectType).FirstOrDefault();
        var obj = objStyle != null ? objStyle.obj : notExistingObj;*/
        var obj = GetObject<ObjectType>(objectStyles, notExistingObj, objectType);
        return Instantiate(obj);
    }

    public Transform GetCharacter(CharacterType characterType)
    {
        var obj = GetObject<CharacterType>(characterStyles, notExistingObj, characterType);
        return Instantiate(obj);
    }
}

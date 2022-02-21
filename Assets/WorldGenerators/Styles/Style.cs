using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Style : ScriptableObject
{
#if UNITY_EDITOR
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
#endif

    [SerializeField]
    Transform notExistingObj;

    [SerializeField]
    ObjectStyle<ObjectType>[] objectStyles;

    [SerializeField]
    ObjectStyle<CharacterType>[] characterStyles;

    public Transform GetObject<T>(ObjectStyle<T>[] objectStyles, Transform defaultT, T objectType)
    {
        var objStyle = objectStyles.Where(objSt => objSt.objectType.Equals(objectType)).GetRandom();
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
        var objPrefab = GetObject<CharacterType>(characterStyles, notExistingObj, characterType);
        var obj = Instantiate(objPrefab);

        var agent = obj.GetComponent<Agent>();
        agent.CharacterState.Health = 100f;

        return obj;
    }
}

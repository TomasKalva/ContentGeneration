using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ShapeGrammar;

public class ShapeGrammarObjectStyle : ScriptableObject
{
    [MenuItem("Assets/Create/ShapeGrammarStyle")]
    public static void CreateMyAsset()
    {
        ShapeGrammarObjectStyle asset = ScriptableObject.CreateInstance<ShapeGrammarObjectStyle>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/ShapeGrammarStyle.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    [SerializeField]
    Transform notExistingObj;

    [SerializeField]
    ObjectStyle<FACE_HOR>[] faceHorObjectStyles;

    [SerializeField]
    ObjectStyle<FACE_VER>[] faceVerObjectStyles;

    [SerializeField]
    ObjectStyle<CORNER>[] cornerObjectStyles;

    [SerializeField]
    ObjectStyle<CUBE>[] cubeObjectStyles;

    [SerializeField]
    ObjectStyle<CharacterType>[] characterStyles;

    public Transform GetObject<T>(ObjectStyle<T>[] objectStyles, Transform defaultT, T objectType)
    {
        var objStyle = objectStyles.Where(objSt => objSt.objectType.Equals(objectType)).GetRandom();
        var obj = objStyle != null ? objStyle.obj : defaultT;
        return obj;
    }

    public Transform GetFaceHor(FACE_HOR objectType)
    {
        var obj = GetObject<FACE_HOR>(faceHorObjectStyles, notExistingObj, objectType);
        return Instantiate(obj);
    }

    public Transform GetFaceVer(FACE_VER objectType)
    {
        var obj = GetObject<FACE_VER>(faceVerObjectStyles, notExistingObj, objectType);
        return Instantiate(obj);
    }

    public Transform GetCorner(CORNER objectType)
    {
        var obj = GetObject<CORNER>(cornerObjectStyles, notExistingObj, objectType);
        return Instantiate(obj);
    }

    public Transform GetCube(CUBE objectType)
    {
        var obj = GetObject<CUBE>(cubeObjectStyles, notExistingObj, objectType);
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

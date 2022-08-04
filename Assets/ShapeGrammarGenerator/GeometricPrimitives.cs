using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ShapeGrammar;

public class GeometricPrimitives : ScriptableObject
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/GeometricPrimitives")]
    public static void CreateMyAsset()
    {
        GeometricPrimitives asset = ScriptableObject.CreateInstance<GeometricPrimitives>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/GeometricPrimitives.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
#endif

    public Transform WallDoor;
    public Transform BrickWall;
    public Transform CobblestoneFloor;
    public Transform Railing;
    public Transform RailingPillar;
    public Transform BeamBottom;
    public Transform BeamMiddle;
    public Transform BeamTop;
    public Transform Stairs;

}

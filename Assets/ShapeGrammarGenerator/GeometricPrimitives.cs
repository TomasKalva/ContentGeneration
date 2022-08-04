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

    public GeometricPrimitive WallDoor;
    public GeometricPrimitive BrickWall;
    public GeometricPrimitive CobblestoneFloor;
    public GeometricPrimitive Railing;
    public GeometricPrimitive RailingPillar;
    public GeometricPrimitive BeamBottom;
    public GeometricPrimitive BeamMiddle;
    public GeometricPrimitive BeamTop;
    public GeometricPrimitive Stairs;

}

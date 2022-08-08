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

    [SerializeField]
    public GeometricPrimitive wallDoor;
    [SerializeField]
    public GeometricPrimitive brickWall;
    [SerializeField]
    public GeometricPrimitive oneSidedWall;
    [SerializeField]
    public GeometricPrimitive cobblestoneFloor;
    [SerializeField]
    public GeometricPrimitive oneSidedFloor;
    [SerializeField]
    public GeometricPrimitive oneSidedCeiling;
    [SerializeField]
    public GeometricPrimitive railing;
    [SerializeField]
    public GeometricPrimitive cladding;
    [SerializeField]
    public GeometricPrimitive railingPillar;
    [SerializeField]
    public GeometricPrimitive beamBottom;
    [SerializeField]
    public GeometricPrimitive beamMiddle;
    [SerializeField]
    public GeometricPrimitive beamTop;
    [SerializeField]
    public GeometricPrimitive stairs;

}

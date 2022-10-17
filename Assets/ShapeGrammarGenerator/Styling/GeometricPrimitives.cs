using UnityEditor;
using UnityEngine;

namespace OurFramework.Environment.StylingAreas
{
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
        public GeometricPrimitive empty;
        [SerializeField]
        public GeometricPrimitive wallDoor;
        [SerializeField]
        public GeometricPrimitive wallHole;
        [SerializeField]
        public GeometricPrimitive railingDoor;

        #region Walls
        [SerializeField]
        public GeometricPrimitive brickWall;
        [SerializeField]
        public GeometricPrimitive woodenWall;
        [SerializeField]
        public GeometricPrimitive tiledWall;
        [SerializeField]
        public GeometricPrimitive stoneWall;
        [SerializeField]
        public GeometricPrimitive cementedWall;
        [SerializeField]
        public GeometricPrimitive pipedWall;
        [SerializeField]
        public GeometricPrimitive barkWall;
        [SerializeField]
        public GeometricPrimitive bigBrickWall;
        #endregion


        #region Railings
        [SerializeField]
        public GeometricPrimitive railing;
        [SerializeField]
        public GeometricPrimitive cladding;
        #endregion


        #region Floors
        [SerializeField]
        public GeometricPrimitive woodenFullFloor;
        [SerializeField]
        public GeometricPrimitive stoneTiledFloor;
        [SerializeField]
        public GeometricPrimitive cobblestoneFloor;
        [SerializeField]
        public GeometricPrimitive ornamentedFloor;
        [SerializeField]
        public GeometricPrimitive carpetFloor;
        [SerializeField]
        public GeometricPrimitive evenTiledFloor;
        [SerializeField]
        public GeometricPrimitive grassFloor;
        [SerializeField]
        public GeometricPrimitive dirtFloor;
        [SerializeField]
        public GeometricPrimitive marbleFloor;
        [SerializeField]
        public GeometricPrimitive oneSidedCeiling;
        #endregion


        #region Pillars
        [SerializeField]
        public GeometricPrimitive railingPillar;
        [SerializeField]
        public GeometricPrimitive beamBottom;
        [SerializeField]
        public GeometricPrimitive beamMiddle;
        [SerializeField]
        public GeometricPrimitive beamTop;
        [SerializeField]
        public GeometricPrimitive beamBottomTop;
        #endregion


        #region Stairs
        [SerializeField]
        public GeometricPrimitive stairs;
        #endregion

        #region Objects
        [SerializeField]
        public GeometricPrimitive gableRoof;
        [SerializeField]
        public GeometricPrimitive pointyRoof;
        [SerializeField]
        public GeometricPrimitive crossRoof;
        [SerializeField]
        public GeometricPrimitive oneDirectionRoof;

        [SerializeField]
        public GeometricPrimitive curvedGableRoof;
        [SerializeField]
        public GeometricPrimitive curvedPointyRoof;
        [SerializeField]
        public GeometricPrimitive curvedCrossRoof;
        [SerializeField]
        public GeometricPrimitive curvedOneDirectionRoof;

        [SerializeField]
        public GeometricPrimitive bridgeTop;
        [SerializeField]
        public GeometricPrimitive bridgeBottom;
        #endregion
    }
}

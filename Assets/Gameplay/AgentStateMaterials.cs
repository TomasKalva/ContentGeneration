using UnityEditor;
using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
    public class MakeScriptableObject
    {
#if UNITY_EDITOR
        [MenuItem("Assets/Create/AgentStateMaterial")]
        public static void CreateMyAsset()
        {
            AgentStateMaterials asset = ScriptableObject.CreateInstance<AgentStateMaterials>();

            string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/NewScripableObject.asset");
            AssetDatabase.CreateAsset(asset, name);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
#endif
    }

    public class AgentStateMaterials : ScriptableObject
    {
        public Material[] materials;
    }
}

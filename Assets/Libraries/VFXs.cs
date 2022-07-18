using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class VFXs : ScriptableObject
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/VFXs")]
    public static void CreateMyAsset()
    {
        VFXs asset = ScriptableObject.CreateInstance<VFXs>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/VFXs.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
#endif

    [SerializeField]
    FireVFX fireVFX;

    public FireVFX Fire()
    {
        var fire = Instantiate(fireVFX);
        fire.gameObject.SetActive(false);
        fire.gameObject.SetActive(true);
        return fire;
    }
}
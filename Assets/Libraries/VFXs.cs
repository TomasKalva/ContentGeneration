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
    FlipbookTexture windTexture;

    [SerializeField]
    FireVFX fireVFX;

    [SerializeField]
    MovingCloudVFX movingCloudVFX;

    [SerializeField]
    FireballVFX fireballVFX;

    [SerializeField]
    LightningVFX lightningVFX;

    public FlipbookTexture WindTexture => windTexture;

    public FireVFX Fire() => Instantiate(fireVFX);
    public MovingCloudVFX MovingCloud() => Instantiate(movingCloudVFX);
    public FireballVFX Fireball() => Instantiate(fireballVFX);
    public LightningVFX Lightning() => Instantiate(lightningVFX);
}
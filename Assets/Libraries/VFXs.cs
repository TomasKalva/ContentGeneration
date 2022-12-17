using OurFramework.Gameplay.RealWorld;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OurFramework.Gameplay.Libraries
{
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
        FlipbookTexture fireTexture;

        [SerializeField]
        FlipbookTexture smokeTexture;

        [SerializeField]
        FlipbookTexture lightningTexture;

        [SerializeField]
        FireVFX fireVFX;

        [SerializeField]
        MovingCloudVFX movingCloudVFX;

        [SerializeField]
        FireballVFX fireballVFX;

        [SerializeField]
        LightningVFX lightningVFX;

        public FlipbookTexture WindTexture => windTexture;
        public FlipbookTexture FireTexture => fireTexture;
        public FlipbookTexture SmokeTexture => smokeTexture;
        public FlipbookTexture LightningTexture => lightningTexture;

        public FireVFX Fire() => Instantiate(fireVFX);
        public MovingCloudVFX MovingCloud() => Instantiate(movingCloudVFX);
        public FireballVFX Fireball() => Instantiate(fireballVFX);
        public LightningVFX Lightning() => Instantiate(lightningVFX);

        public List<VFX> TestVFXs(Transform parent)
        {
            var textures = new[]{
            WindTexture,
            FireTexture,
            SmokeTexture,
            LightningTexture,
        };
            var vfxs = new Func<VFX>[]
            {
            Fire,
            MovingCloud,
            Fireball,
            Lightning,
            };
            var color = new Color(1f, 1f, 1f, 1f);

            var vfxList = new List<VFX>();
            textures.ForEach((texture, x) =>
                vfxs.ForEach((vfxF, z) =>
                {
                    var vfx = vfxF();
                    vfx.SetColor(color);
                    vfx.SetTexture(texture);
                    vfx.transform.SetParent(parent);
                    vfx.transform.position = 6f * new Vector3(x, 0f, z);
                    vfxList.Add(vfx);
                }));

            return vfxList;
        }
    }
}

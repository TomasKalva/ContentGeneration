using OurFramework.Util;
using UnityEngine;

namespace OurFramework.Game
{
    /// <summary>
    /// Sky made of multiple planes with a custom shader.
    /// </summary>
    public class EnvironmentMap : MonoBehaviour
    {
        [SerializeField]
        Transform skyParent;

        Renderer[] skyboxPlanes;

        [SerializeField]
        Light sun;

        public Light Sun => sun;

        Color DefaultAmbientLight { get; set; }

        void Awake()
        {
            skyboxPlanes = skyParent.GetComponentsInChildren<Renderer>();
            DefaultAmbientLight = new Color(0.5446778f, 0.6146909f, 0.754717f, 1.0f);//  RenderSettings.ambientLight;
        }

        public void SetSkyVariability(float value)
        {
            skyboxPlanes = skyParent.GetComponentsInChildren<Renderer>();
            skyboxPlanes.ForEach(skyboxPlane => skyboxPlane.material.SetFloat("_InterpolateSky", value));
        }

        public void SetSkyBrightness(float value)
        {
            RenderSettings.ambientLight = value * DefaultAmbientLight;
        }
    }
}

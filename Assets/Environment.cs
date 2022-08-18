using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
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
        DefaultAmbientLight = RenderSettings.ambientLight;
    }

    public void SetSkyVariability(float value)
    {
        skyboxPlanes.ForEach(skyboxPlane => skyboxPlane.material.SetFloat("_InterpolateSky", value));
    }

    public void SetSkyBrightness(float value)
    {
        RenderSettings.ambientLight = value * DefaultAmbientLight;
        //skyboxPlanes.ForEach(skyboxPlane => skyboxPlane.material.SetFloat("_InterpolateSky", value));
    }
}

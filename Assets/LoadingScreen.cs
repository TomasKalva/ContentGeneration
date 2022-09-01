using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    Camera cam;

    [SerializeField]
    LayerMask Everything;

    [SerializeField]
    LayerMask LoadingLayer;

    [SerializeField]
    Renderer LoadingScreenRenderer;

    public void SetOpacity(float value)
    {
        LoadingScreenRenderer.material.SetFloat("_Opacity", value);
    }

    public void StartLoading()
    {
        cam.cullingMask = LoadingLayer;
    }

    public void EndLoading()
    {
        cam.cullingMask = Everything & ~LoadingLayer;
    }
}

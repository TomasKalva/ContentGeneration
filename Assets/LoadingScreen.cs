using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    Camera LoadingScreenCamera;

    [SerializeField]
    Renderer LoadingScreenRenderer;

    public void SetOpacity(float value)
    {
        LoadingScreenRenderer.material.SetFloat("_Opacity", value);
    }

    public void StartLoading()
    {
        LoadingScreenCamera.gameObject.SetActive(true);
    }

    public void EndLoading()
    {
        LoadingScreenCamera.gameObject.SetActive(false);
    }
}
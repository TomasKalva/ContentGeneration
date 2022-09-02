using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFX : MonoBehaviour, IDestroyable
{
    [SerializeField]
    protected VisualEffect visualEffect;

    /// <summary>
    /// Disables its renderer on start so shouldn't be put on the same object as visual effect.
    /// </summary>
    [SerializeField]
    protected ColliderDetector colliderDetector;
    public ColliderDetector ColliderDetector => colliderDetector;

    #region Parametrization

    [SerializeField]
    string colorName;

    [SerializeField]
    string textureName;

    [SerializeField]
    string textureWidth;

    [SerializeField]
    string textureHeight;

    public Color Color
    {
        set
        {
            visualEffect.SetVector4(colorName, value);
        }
    }

    Texture Texture
    {
        set
        {
            visualEffect.SetTexture(textureName, value);
        }
    }

    int TextureWidth
    {
        set
        {
            visualEffect.SetInt(textureWidth, value);
        }
    }

    int TextureHeight
    {
        set
        {
            visualEffect.SetInt(textureHeight, value);
        }
    }

    public virtual void SetColor(Color color)
    {
        Color = color;
    }

    public virtual void SetTexture(FlipbookTexture flipbookTexture)
    {
        Texture = flipbookTexture.Texture;
        TextureWidth = flipbookTexture.Width;
        TextureHeight = flipbookTexture.Height;
    }

    #endregion

    public virtual void Destroy(float timeS)
    {
        Destroy(gameObject, timeS);
    }
}

public interface IDestroyable
{
    /// <summary>
    /// The object will be destroyed in timeS seconds.
    /// </summary>
    void Destroy(float timeS);
}

/// <summary>
/// Describes texture made of multiple smaller textures in grid.
/// </summary>
[Serializable]
public class FlipbookTexture
{
    [SerializeField]
    Texture texture;
    [SerializeField]
    int width;
    [SerializeField]
    int height;

    public Texture Texture => texture;
    public int Width => width;
    public int Height => height;
}

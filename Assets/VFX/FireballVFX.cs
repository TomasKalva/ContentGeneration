using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FireballVFX : VFX
{
    [SerializeField]
    string colorName;

    [SerializeField]
    string textureName;

    public Color Color
    {
        set
        {
            visualEffect.SetVector4(colorName, value);
        }
    }

    public Texture Texture
    {
        set
        {
            visualEffect.SetTexture(textureName, value);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXTesting : MonoBehaviour
{
    [SerializeField]
    Transform parent;

    [SerializeField]
    VFXs VFXs;

    [SerializeField]
    Color color;

    List<VFX> vfxs;

    // Start is called before the first frame update
    void Start()
    {
        vfxs = VFXs.TestVFXs(parent);
    }

    // Update is called once per frame
    void Update()
    {
        vfxs.ForEach(vfx => vfx.SetColor(color));
    }
}

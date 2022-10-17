using UnityEngine;
using UnityEngine.VFX;

public class Kiln : InteractiveObject
{
    [SerializeField]
    VisualEffect fire;

    public void BurstFire()
    {
        fire.SendEvent("FireBurst");
        Debug.Log("FireBurst");
    }
}

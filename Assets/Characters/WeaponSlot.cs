using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

public class WeaponSlot : MonoBehaviour
{
    [SerializeField]
    Weapon weapon;

    public Weapon Weapon 
    {
        get => weapon;
        set
        {
            DestroyWeapon();
            CreateWeapon(value);
        }
    }

    void DestroyWeapon()
    {
        if (weapon == null) return;

        Destroy(weapon);
    }

    void CreateWeapon(Weapon weaponPrefab)
    {
        if (weaponPrefab == null) return;

        var newWeapon = Instantiate(weaponPrefab);
        newWeapon.transform.SetParent(transform);
        weapon = newWeapon;
    }
}

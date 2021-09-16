using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

public class WeaponSlot : MonoBehaviour
{
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

        Destroy(weapon.gameObject);
    }

    void CreateWeapon(Weapon weaponPrefab)
    {
        if (weaponPrefab == null)
        {
            weapon = null;
            return;
        }

        var newWeapon = Instantiate(weaponPrefab);
        newWeapon.transform.SetParent(transform);
        weapon = newWeapon;
    }

    private void Awake()
    {
        weapon = GetComponentInChildren<Weapon>();
    }
}

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
            RemoveWeapon();
            AddWeapon(value);
        }
    }

    void RemoveWeapon()
    {
        if (weapon == null) return;

        weapon.transform.SetParent(null);
        weapon.Active = false;
        //Destroy(weapon.gameObject);
    }

    void AddWeapon(Weapon newWeapon)
    {
        if (newWeapon == null)
        {
            this.weapon = null;
            return;
        }

        //var weapon = Instantiate(weapon, transform);
        newWeapon.transform.SetParent(transform);
        newWeapon.FindOwner();
        newWeapon.Active = true;

        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        newWeapon.transform.localScale = Vector3.one;

        this.weapon = newWeapon;
    }

    private void Awake()
    {
        weapon = GetComponentInChildren<Weapon>();
    }
}

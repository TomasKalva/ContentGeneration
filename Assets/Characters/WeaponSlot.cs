using ContentGeneration.Assets.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

public class WeaponSlot : MonoBehaviour
{
    [SerializeField]
    float weaponScale = 1f;

    /// <summary>
    /// To manage destruction of weapons correctly.
    /// </summary>
    public World World { private get; set; }

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

        World.PutToCache(weapon.transform);
        weapon.gameObject.SetActive(false);
    }

    void AddWeapon(Weapon newWeapon)
    {
        if (newWeapon == null)
        {
            this.weapon = null;
            return;
        }

        newWeapon.transform.SetParent(transform);
        newWeapon.gameObject.SetActive(true);

        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        newWeapon.transform.localScale = weaponScale * Vector3.one;

        newWeapon.Show();

        this.weapon = newWeapon;
    }

    private void Awake()
    {
    }
}

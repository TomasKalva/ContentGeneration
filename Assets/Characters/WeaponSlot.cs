using ContentGeneration.Assets.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

public class WeaponSlot : EquipmentSlot<Weapon>
{
    /*
    Weapon weapon;

    public override Transform Equipment 
    {
        get => equipment;
        set
        {
            RemoveWeapon();
            AddWeapon(value);
        }
    }

    void RemoveWeapon()
    {
        if (equipment == null) return;

        World.PutToCache(equipment.transform);
        equipment.gameObject.SetActive(false);
    }

    void AddWeapon(Transform newEquipment)
    {
        if (newEquipment == null)
        {
            this.equipment = null;
            return;
        }

        newEquipment.transform.SetParent(transform);
        newEquipment.gameObject.SetActive(true);

        newEquipment.transform.localPosition = Vector3.zero;
        newEquipment.transform.localRotation = Quaternion.identity;
        newEquipment.transform.localScale = weaponScale * Vector3.one;

        this.equipment = newEquipment;
    }*/
}

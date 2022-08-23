﻿using ContentGeneration.Assets.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

public class EquipmentSlot<EquipmentT> : MonoBehaviour where EquipmentT : Equipment
{
    [SerializeField]
    float equipmentScale = 1f;

    /// <summary>
    /// To manage destruction of equipment correctly.
    /// </summary>
    public World World { private get; set; }

    Transform equipmentTransform;
    EquipmentT equipment;

    public EquipmentT Equipment 
    {
        get => equipment;
        set
        {
            RemoveEquipment();

            equipment = value;
            equipmentTransform = value?.transform;
            AddEquipment(equipmentTransform);
        }
    }

    void RemoveEquipment()
    {
        if (equipmentTransform == null) return;

        World.PutToCache(equipmentTransform.transform);
        equipmentTransform.gameObject.SetActive(false);
    }

    void AddEquipment(Transform newWeapon)
    {
        if (newWeapon == null)
        {
            this.equipmentTransform = null;
            return;
        }

        newWeapon.transform.SetParent(transform);
        newWeapon.gameObject.SetActive(true);

        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        newWeapon.transform.localScale = equipmentScale * Vector3.one;

        this.equipmentTransform = newWeapon;
    }
}

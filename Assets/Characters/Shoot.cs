using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : Attack
{
    [SerializeField]
    Rigidbody ammo;

    [SerializeField]
    Transform gun;

    [SerializeField]
    float speed;

    [SerializeField]
    float warmup;

    [SerializeField]
    float cooldown;

    public override IEnumerator Act()
    {
        yield return new WaitForSeconds(warmup);
        var bullet = Instantiate(ammo);
        bullet.transform.position = gun.position + gun.forward * 0.5f;
        bullet.transform.eulerAngles = gun.eulerAngles;
        bullet.velocity = gun.transform.forward * speed;
        yield return new WaitForSeconds(cooldown);
    }
}
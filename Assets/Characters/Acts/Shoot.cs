using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : AnimatedAct
{
    [SerializeField]
    Projectile projectile;

    [SerializeField]
    Transform gun;

    [SerializeField]
    float speed;

    public override void StartAct(Agent agent)
    {
        timeElapsed = 0f;

        var bullet = Instantiate(projectile);
        bullet.transform.position = gun.position + gun.forward * 0.5f;
        bullet.transform.eulerAngles = gun.eulerAngles;
        bullet.GetComponent<Rigidbody>().velocity = gun.transform.forward * speed;
        bullet.Active = true;
        bullet.Owner = agent;
    }
}
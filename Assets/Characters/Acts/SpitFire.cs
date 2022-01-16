using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpitFire : MultiEventAct
{
    [SerializeField]
    Projectile projectile;

    [SerializeField]
    Transform gun;

    [SerializeField]
    float speed;

    [SerializeField]
    float startT;

    [SerializeField]
    float endT;

    [SerializeField]
    int shotsCount;

    public override void OnStart(Agent agent)
    {
        base.OnStart(agent);

        timedActions = new List<TimeAction>();
        timedActions.AddRange(Enumerable.Range(0, shotsCount).Select(i => new TimeAction(startT + i / (float)shotsCount * (endT - startT), () => DoShot(agent))));
    }

    void DoShot(Agent agent)
    {
        var direction = gun.up;
        var bullet = Instantiate(projectile);
        bullet.transform.position = gun.position + direction * 0.5f;
        bullet.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        bullet.GetComponent<Rigidbody>().velocity = direction * speed;
        bullet.Active = true;
        bullet.Owner = agent;
    }
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAct : Act
{
    public override IEnumerator Perform(Movement movement)
    {
        /*yield return new WaitForSeconds(warmup);
        var bullet = Instantiate(ammo);
        bullet.transform.position = gun.position + gun.forward * 0.5f;
        bullet.transform.eulerAngles = gun.eulerAngles;
        bullet.velocity = gun.transform.forward * speed;
        yield return new WaitForSeconds(cooldown);*/
        yield return new WaitForSeconds(5); 
    }
}

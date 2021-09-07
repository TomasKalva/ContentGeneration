using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAlmond : Almond
{
    [SerializeField]
    public AnimancerComponent animancerAnimator;

    [SerializeField]
    ClipTransition openDoor;

    [SerializeField]
    ClipTransition closeDoor;

    bool open = true;

    protected override void Activate()
    {
        var animToPlay = open ? closeDoor : openDoor;
        animancerAnimator.Play(animToPlay, 0.1f);
    }
}

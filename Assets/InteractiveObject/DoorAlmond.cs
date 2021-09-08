using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAlmond : Almond
{
    [SerializeField]
    public AnimancerComponent animancerAnimator;

    [SerializeField]
    AnimationClip openDoor;

    [SerializeField]
    AnimationClip closeDoor;

    bool open = false;

    protected override void Activate()
    {
        var animToPlay = open ? closeDoor : openDoor;
        open = !open;

        var state = animancerAnimator.Play(animToPlay);

        WorldEventsLog.Get.Log("Door activated", LogPriority.Info);
    }
}

public enum LogPriority
{
    Info,
}

public class WorldEventsLog
{
    public static WorldEventsLog Get { get; }

    static WorldEventsLog()
    {
        Get = new WorldEventsLog(true);
    }

    bool showMessages;

    private WorldEventsLog(bool showMessages) 
    {
        this.showMessages = showMessages;
    }

    public void Log(string msg, LogPriority priority)
    {
        Debug.Log(msg);
    }
}
using Animancer;
using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractiveObject
{
    [SerializeField]
    public AnimancerComponent animancerAnimator;

    [SerializeField]
    AnimationClip openDoor;

    [SerializeField]
    AnimationClip closeDoor;

    bool open = false;

    private void Awake()
    {
        State = new DoorState(this);
    }

    public void SwitchPosition()
    {
        var animToPlay = open ? closeDoor : openDoor;
        open = !open;

        var state = animancerAnimator.Play(animToPlay);

        WorldEventsLog.Get.Log("Door activated", LogPriority.Info);
    }
}

public class DoorState : InteractiveObjectState<Door>
{
    public DoorState(Door door)
    {
        IntObj = door;
        SetInteraction(ins => ins.Act("Open/Close", (door, _) => door.IntObj.SwitchPosition()));
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
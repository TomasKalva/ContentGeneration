using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomModule : Module
{
    [SerializeField]
    DirectionObject[] walls;

    public override bool ReachableFrom(Vector3Int dir)
    {
        return true;
    }
}

[Serializable]
public class DirectionObject
{
    public Vector3Int direction;

    public AttachmentPoint obj;
}
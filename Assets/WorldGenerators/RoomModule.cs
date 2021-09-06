using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomModule : Module
{
    [SerializeField]
    DirectionObject[] walls;
}

[Serializable]
public class DirectionObject
{
    public Vector3Int direction;

    public AttachmentPoint obj;
}
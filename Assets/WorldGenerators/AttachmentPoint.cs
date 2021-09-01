using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentPoint: MonoBehaviour
{
    public ObjectType ObjectType { get; }

    public Transform Obj { get; private set; }

    public AttachmentPoint(ObjectType objectType)
    {
        ObjectType = objectType;
    }

    public void SetObject(Style style)
    {
        Obj = style.GetObject(ObjectType);
    }
}

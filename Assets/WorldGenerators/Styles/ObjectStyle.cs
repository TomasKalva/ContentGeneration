using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectStyle<ObjectTypeT>
{
    [SerializeField]
    public ObjectTypeT objectType;

    [SerializeField]
    public Transform obj;
}

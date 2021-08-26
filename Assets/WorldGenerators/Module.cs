using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Module : MonoBehaviour
{
    public Vector3Int coords;

    public bool empty;

    public void Init(Vector3Int coords)
    {
        this.coords = coords;
    }
}

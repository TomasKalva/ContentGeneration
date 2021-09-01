using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentPoint: MonoBehaviour
{
    public ObjectType objectType;

    Transform obj;

    public AttachmentPoint(ObjectType objectType)
    {
        this.objectType = objectType;
    }

    public void SetObject(Style style)
    {
        obj = style.GetObject(objectType);
        obj.SetParent(transform);
        obj.SetPositionAndRotation(transform.position, transform.rotation);
    }

    public void RotateTowards(Vector3Int direction)
    {
        var angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    void OnDrawGizmos()
    {
        // Draw a yellow plane at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawSphere(Vector3.zero, 0.5f);
        Gizmos.DrawRay(Vector3.zero, Vector3.forward);
    }
}

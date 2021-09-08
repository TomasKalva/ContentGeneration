using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentPoint: MonoBehaviour
{
    public ObjectType objectType;

    public Vector3Int PointingDirection { get; private set; }

    ActionObject activatorObject;

    ActionObject almondObject;

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

        var activator = obj.GetComponent<Activator>();
        if(activator != null)
        {
            activator.SetActionObject(activatorObject);
        }

        var almond = obj.GetComponent<Almond>();
        if (almond != null)
        {
            almond.SetActionObject(almondObject);
        }
    }

    public void SetActivator(ActionObject activator)
    {
        this.activatorObject = activator;
    }

    public void SetAlmond(ActionObject almond)
    {
        this.almondObject = almond;
    }

    public void RotateTowards(Vector3Int direction)
    {
        var angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
        PointingDirection = direction;
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

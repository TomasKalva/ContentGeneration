using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField]
    List<Transform> objects;

    [SerializeField]
    Vector3 rotationSpeed;

    // Start is called before the first frame update
    void Awake()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var angle = rotationSpeed * Time.fixedDeltaTime;
        foreach(var obj in objects)
        {
            var localPos = transform.worldToLocalMatrix.MultiplyPoint(obj.position);
            var newPos = Quaternion.Euler(angle) * localPos;
            obj.position = transform.localToWorldMatrix.MultiplyPoint(newPos);

            obj.Rotate(angle);
            //obj.Rotate(new Vector3(angle, 0f), );
        }
    }
}

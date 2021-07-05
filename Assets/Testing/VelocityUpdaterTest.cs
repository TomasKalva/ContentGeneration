using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityUpdaterTest : MonoBehaviour
{
    [SerializeField, Curve(0f, 0f, 1f, 45f, true)]
    AnimationCurve speedF;

    [SerializeField]
    Rigidbody prefab;

    List<Rigidbody> objects = new List<Rigidbody>();

    int n = 10;

    float t = 0f;

    // Start is called before the first frame update
    void Start()
    {
        for(int i =0; i < n; i++)
        {
            var obj = Instantiate(prefab);
            obj.transform.parent = this.transform;
            obj.transform.localPosition = new Vector3(i * 1, 0, 0);
            objects.Add(obj);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (var rig in objects)
        {
            var speed0 = speedF.Evaluate((t - Time.fixedDeltaTime));
            var speed1 = speedF.Evaluate(t);

            //movement.Impulse((speed1 - speed0) * Vector3.up /* movement.InputToWorld(direction)*/);
            rig.velocity += (speed1 - speed0) * Vector3.up;
        }
        t = (t + Time.fixedDeltaTime) % 1.0f;
    }
}

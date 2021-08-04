using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : MonoBehaviour
{

    List<InteractiveObject> interactiveObjects;

    // Start is called before the first frame update
    void Start()
    {
       interactiveObjects = new List<InteractiveObject>(FindObjectsOfType<InteractiveObject>()); 
    }

    public IEnumerable<InteractiveObject> ObjectsCloseTo(Vector3 point, float dist)
    {
        return interactiveObjects.Where(o => (o.transform.position - point).sqrMagnitude <= dist * dist);
    }
}

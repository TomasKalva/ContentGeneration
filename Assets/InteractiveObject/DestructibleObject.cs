using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [SerializeField]
    Transform toDestroy;

    [SerializeField]
    Transform brokenObjectPref;

    ColliderDetector detector;

    private void Start()
    {
        detector = GetComponentInChildren<ColliderDetector>();
        if(detector == null)
        {
            Debug.LogError($"{this} doesn't have a collider detector!");
        }
    }

    private void Update()
    {
        if (detector.other)
        {
            Debug.Log("Collision!!!");
            var rb = detector.other.GetComponent<Rigidbody>();
            var agent = detector.other.GetComponentInParent<Agent>();

            if (!agent)
            {
                Debug.LogError($"{detector.other} doesn't have Agent!");
            }
            if(agent.State == AgentState.DAMAGE)
            {
                var brokenObject = Instantiate(brokenObjectPref);
                brokenObject.position = transform.position;
                brokenObject.rotation = transform.rotation;

                Destroy(toDestroy.gameObject);
                Destroy(gameObject);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ColliderDetector : MonoBehaviour
{
    [SerializeField]
    LayerMask detectionMask = -1;

    public Collider other;

    public bool Triggered => other != null;

    public bool Show { get; set; }

    private void Start()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null && !Show)
        {
            var options = GameObject.Find("GameOptions").GetComponent<GameOptions>();
            if (!options.showDetectors)
            {
                renderer.enabled = false;
            }
            else
            {
                renderer.enabled = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Enter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Exit(other);
    }

    private void Enter(Collider other)
    {
        if (detectionMask == (detectionMask | (1 << other.gameObject.layer)))
        {
            this.other = other;
        }
    }

    private void Exit(Collider other)
    {
        if (detectionMask == (detectionMask | (1 << other.gameObject.layer)))
        {
            this.other = null;
        }
    }
}

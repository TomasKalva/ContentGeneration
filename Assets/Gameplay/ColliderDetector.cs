using OurFramework.Game;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Detects colliders that collide with this.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ColliderDetector : MonoBehaviour
{
    [SerializeField]
    LayerMask detectionMask = -1;

    public Collider other;
    List<Collider> hits;

    public IEnumerable<Collider> Hits => hits.Where(x => x != null);

    public bool Triggered => other != null;

    public bool Show { get; set; }

    public delegate void Collision(Collider other);

    public event Collision OnEnter = delegate { };

    private void Awake()
    {
        hits = new List<Collider>();
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
            if (other != null)
            {
                OnEnter(other);
                this.other = other;
                hits.Add(other);
            }
        }
    }

    private void Exit(Collider other)
    {
        if (detectionMask == (detectionMask | (1 << other.gameObject.layer)))
        {
            hits.Remove(other);
            this.other = null;
        }
    }
}

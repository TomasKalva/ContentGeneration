using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Detector))]
public class Weapon : MonoBehaviour
{
    Detector detector;

    List<Agent> currentlyHit;

    [SerializeField]
    float damage;

    Agent owner;

    bool _active;

    public bool Active
    {
        get
        {
            return _active;
        }
        set
        {
            _active = value;
            if (!_active)
            {
                currentlyHit.Clear();
            }
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        detector = GetComponent<Detector>();
        currentlyHit = new List<Agent>();
        owner = GetComponentInParent<Agent>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Active && detector.Triggered)
        {
            var hitAgent = detector.other.GetComponentInParent<Agent>();
            if (hitAgent != owner && !currentlyHit.Contains(hitAgent))
            {
                hitAgent.CharacterState.Health -= damage;
                currentlyHit.Add(hitAgent);
            }
        }
    }
}

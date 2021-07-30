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
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Active && detector.triggered)
        {
            var hitAgent = detector.other.GetComponentInParent<Agent>();
            if (!currentlyHit.Contains(hitAgent))
            {
                hitAgent.character.Health -= damage;
                currentlyHit.Add(hitAgent);
            }
        }
    }
}

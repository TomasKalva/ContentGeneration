using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageDealer : MonoBehaviour
{
    [SerializeField]
    protected float damage;

    List<Agent> currentlyHit;

    Agent _owner;

    public Agent Owner
    {
        get => _owner;
        set =>  _owner = value;
    }
    void Awake()
    {
        currentlyHit = new List<Agent>();
        if (_owner == null)
        {
            _owner = GetComponentInParent<Agent>();
        }
        Initialize();
    }

    void FixedUpdate()
    {
        foreach(var hitAgent in HitAgents())
        {
            if (hitAgent != Owner && !currentlyHit.Contains(hitAgent))
            {
                Damage(hitAgent);
                currentlyHit.Add(hitAgent);
            }
        }
    }

    protected void ResetHitAgents()
    {
        currentlyHit.Clear();
    }

    protected abstract IEnumerable<Agent> HitAgents();

    protected abstract void Damage(Agent agent);

    protected abstract void Initialize();
}

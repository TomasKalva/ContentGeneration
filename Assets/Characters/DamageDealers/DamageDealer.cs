using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageDealer : MonoBehaviour
{
    [SerializeField]
    float _damage;
    public float Damage 
    { 
        get => _damage; 
        set => _damage = value; 
    }

    List<Agent> currentlyHit;

    Agent _owner;

    public Agent Owner
    {
        get => _owner;
        set =>  _owner = value;
    }

    public abstract bool Active { get; set; }

    GameOptions options;

    void Awake()
    {
        currentlyHit = new List<Agent>();
        if (_owner == null)
        {
            FindOwner();
        }
        options = GameObject.Find("GameOptions").GetComponent<GameOptions>();
        Initialize();
    }

    public void FindOwner()
    {
        _owner = GetComponentInParent<Agent>();
    }

    void FixedUpdate()
    {
        foreach(var hitAgent in HitAgents())
        {
            if (hitAgent == null || !hitAgent.gameObject.activeSelf)
                continue;

            if (hitAgent != Owner && !currentlyHit.Contains(hitAgent))
            {

                if(!options.friendlyFire && hitAgent.gameObject.layer == Owner.gameObject.layer)
                {
                    continue;
                }
                DealDamage(hitAgent);
                currentlyHit.Add(hitAgent);
            }
        }
        OnFixedUpdate();
    }

    protected void ResetHitAgents()
    {
        currentlyHit.Clear();
    }

    protected abstract IEnumerable<Agent> HitAgents();

    protected virtual void DealDamage(Agent agent)
    {
        agent.CharacterState.TakeDamage(new Damage(DamageType.Physical, Damage));
    }

    protected abstract void Initialize();

    protected virtual void OnFixedUpdate() { }

    public virtual Vector3 PushForce(Transform enemy) => Vector3.zero;
}

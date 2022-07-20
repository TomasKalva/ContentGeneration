using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public ColliderDetector Detector { get; private set; }

    [SerializeField]
    float pushForceIntensity = 500f;

    SelectorByUser HitSelector { get; set; }
    List<Effect> Effects { get; set; }

    public Weapon SetHitSelector(SelectorByUser selector)
    {
        HitSelector = selector;
        return this;
    }

    public bool Active
    {
        set
        {
            Detector.enabled = value;
        }
    }

    private void Awake()
    {
        Detector = GetComponent<ColliderDetector>();
        Effects = new List<Effect>();
    }

    public Weapon AddEffect(Effect effect)
    {
        Effects.Add(effect);
        return this;
    }

    protected IEnumerable<Agent> HitAgents()
    {
        return Detector.Hit.Select(hit => hit.GetComponentInParent<Agent>());
        /*if (Active && detector.Triggered)
        {
            return new Agent[1] { detector.other.GetComponentInParent<Agent>() };
        }
        else
        {
            return Enumerable.Empty<Agent>();
        }*/
    }

    public void DealDamage(Agent owner)
    {
        var ownerState = owner.CharacterState;
        ownerState.World.AddOccurence(
            new Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions.Occurence(
                HitSelector(ownerState),
                Effects.ToArray()
                )
            );
        /*owner.World.AddOccurence(
            new Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions.Occurence(
                Weapon.GetSelector(),
                ))*/
    }
    /*
    public override Vector3 PushForce(Transform enemy)
    {
        return pushForceIntensity * (enemy.position - Owner.transform.position).normalized;
    }*/
}

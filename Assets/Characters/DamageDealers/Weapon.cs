using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public ColliderDetector Detector { get; private set; }

    [SerializeField]
    float pushForceIntensity = 500f;

    ByTime<SelectorByUser> HitSelectorByDuration { get; set; }
    List<Effect> Effects { get; set; }

    public Weapon SetHitSelector(ByTime<SelectorByUser> selectorByDuration)
    {
        HitSelectorByDuration = selectorByDuration;
        return this;
    }

    bool _active;

    public bool Active
    {
        get
        {
            return _active;
        }
        set
        {
            Detector.enabled = value;
            _active = value;
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

    public void DealDamage(Agent owner, float damageDuration)
    {
        var ownerState = owner.CharacterState;
        ownerState.World.AddOccurence(
            new Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions.Occurence(
                HitSelectorByDuration(damageDuration)(ownerState),
                Effects.ToArray()
                )
            );
    }
}

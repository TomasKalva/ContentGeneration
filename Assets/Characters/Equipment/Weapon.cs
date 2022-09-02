using Assets.Characters.Items.ItemClasses;
using Assets.Characters.SpellClasses;
using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Weapon : Equipment
{
    public ColliderDetector Detector { get; private set; }
    Renderer Renderer { get; set; }

    public ByTime<SelectorByUser> HitSelectorByDuration { get; set; }

    public WeaponItem WeaponItem { private get; set; }

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
        Detector = GetComponentInChildren<ColliderDetector>();
        Renderer = GetComponentInChildren<Renderer>();
    }

    private void Start()
    {
        // Renderer needs to be manually activated because collider detector deactivated it
        if (Renderer != null)
        {
            Renderer.enabled = true;
        }
    }

    public void DealDamage(Agent owner, float damageDuration)
    {
        WeaponItem.DealDamage(owner.CharacterState, damageDuration);
    }
}

using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IDestroyable
{
    /// <summary>
    /// The object will be destroyed in timeS seconds.
    /// </summary>
    void Destroy(float timeS);
}

/// <summary>
/// Handles collisions detection, duration and destruction of objects the selector is composed of.
/// </summary>
public class GeometricTargetSelector
{
    ColliderDetector colliderDetector;
    IDestroyable destroyable;
    FinishedInTime finishedInTime;

    public SelectTargets SelectTargets { get; }

    public GeometricTargetSelector(IDestroyable destroyable, ColliderDetector colliderDetector, FinishedInTime finishedInTime)
    {
        this.destroyable = destroyable;
        this.colliderDetector = colliderDetector;
        this.finishedInTime = finishedInTime;
        SelectTargets = () => colliderDetector.Hit.SelectNN(c => c.GetComponentInParent<Agent>()?.CharacterState);
    }

    public bool Update(float deltaT)
    {
        bool finished = finishedInTime(deltaT);
        if (finished)
        {
            destroyable.Destroy(1f);
        }
        return finished;
    }
}

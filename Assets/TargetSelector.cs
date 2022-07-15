using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    public SelectTargets SelectTargets { get; }
    Action OnEnd { get; }
    FinishedInTime update { get; }

    public bool MyUpdate(float deltaT)
    {
        bool finished = update(deltaT);
        if (finished)
        {
            OnEnd();
        }
        return finished;
    }
}

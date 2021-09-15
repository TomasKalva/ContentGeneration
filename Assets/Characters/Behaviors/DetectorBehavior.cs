using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DetectorBehavior : Behavior
{
    ColliderDetector[] detectors;

    ActFactory actFactory;

    Act currentAct;

    public DetectorBehavior(ActFactory actFactory, params ColliderDetector[] detectors)
    {
        this.detectors = detectors;
        this.actFactory = actFactory;
        this.currentAct = null;
    }

    public Transform DetectedTarget()
    {
        return detectors.Select(detector => detector.other).FirstOrDefault()?.transform;
    }

    public override bool CanEnter(Agent agent)
    {
        return detectors.Any(detector => detector.Triggered && agent != detector.other.GetComponentInParent<Agent>());
    }
    /// <summary>
    /// Number in [0, 10]. The higher the more the agent wants to do this behaviour.
    /// </summary>
    public override int Priority(Agent agent) => 5;
    public override void Enter(Agent agent)
    {
        this.currentAct = actFactory();
        this.currentAct.TargetPosition = new TargetPosition(DetectedTarget(), Vector3.zero);
        agent.acting.SelectAct(currentAct);
    }

    public override bool UpdateBehavior(Agent agent)
    {
        return currentAct.ActEnded;
    }
}

public delegate Act ActFactory();
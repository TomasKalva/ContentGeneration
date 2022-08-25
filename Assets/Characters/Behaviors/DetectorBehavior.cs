using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DetectorBehavior : Behavior
{
    ColliderDetector[] detectors;

    ActSelector actSelector;

    Act act;

    public DetectorBehavior(ActSelector actSelector, params ColliderDetector[] detectors)
    {
        this.detectors = detectors;
        this.actSelector = actSelector;
        this.act = actSelector();
    }

    public Transform DetectedTarget()
    {
        //return detectors.SelectNN(detector => detector.other).FirstOrDefault()?.transform;
        var maybeOther = detectors.Select(detector =>
        {
            if (detector != null)
            {
                return detector.other;
            }
            return null;
        }).Where(other => other != null).FirstOrDefault();

        if (maybeOther == null) {
            return null;
        }
        return maybeOther.transform;
    }

    public override bool CanEnter(Agent agent)
    {
        return act.CanBeUsed(agent) && detectors.Any(detector => detector.Triggered && agent != detector.other.GetComponentInParent<Agent>());
    }
    /// <summary>
    /// Number in [0, 10]. The higher the more the agent wants to do this behaviour.
    /// </summary>
    public override int Priority(Agent agent) => 5;
    public override void Enter(Agent agent)
    {
        this.act = actSelector();
        this.act.TargetPosition = new TargetPosition(DetectedTarget(), Vector3.zero);
        //agent.acting.SelectAct(currentAct);
    }

    public override bool Update(Agent agent)
    {
        return act.ActEnded;
    }
}

public delegate Act ActSelector();
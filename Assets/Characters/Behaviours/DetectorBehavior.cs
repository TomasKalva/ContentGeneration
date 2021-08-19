using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DetectorBehavior : Behavior
{
    [SerializeField]
    ColliderDetector[] detectors;

    [SerializeField]
    Act act;

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
        agent.acting.SelectAct(act);
    }
    public override bool UpdateBehavior(Agent agent)
    {
        return agent.acting.ActEnded;
    }
}

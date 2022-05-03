using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Behavior
{
    public abstract bool CanEnter(Agent agent);
    /// <summary>
    /// Number in [0, 10]. The higher the more the agent wants to do this behaviour.
    /// </summary>
    public abstract int Priority(Agent agent);
    public virtual void Enter(Agent agent) { }
    /// <summary>
    /// Returs true if should exit.
    /// </summary>
    public virtual bool Update(Agent agent) => true;
    public virtual void Exit(Agent agent) { }
}

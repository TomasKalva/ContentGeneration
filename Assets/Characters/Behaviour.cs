using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Behaviour : MonoBehaviour
{
    public abstract bool CanEnter(Agent agent);
    /// <summary>
    /// Number in [0, 10]. The higher the more the agent wants to do this behaviour.
    /// </summary>
    public abstract int Priority(Agent agent);
    public abstract void Enter(Agent agent);
    public abstract void Update(Agent agent);
    public abstract void Exit(Agent agent);
}

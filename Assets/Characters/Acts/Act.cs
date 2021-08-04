using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActType
{
    IDLE,
    DEFFENSIVE,
    OFFENSIVE,
}

public class Act : MonoBehaviour
{
    [SerializeField]
    Detector detector;

    [SerializeField]
    public string actName;

    [SerializeField]
    public ActType type;

    [SerializeField]
    public int priority;

    public bool CanBeUsed()
    {
        return detector && detector.triggered;
    }

    public virtual void StartAct(Agent agent) { }
    /// <summary>
    /// Returns true if finished.
    /// </summary>
    public virtual bool UpdateAct(Agent agent) => true;
    public virtual void EndAct(Agent agent) { }

    public virtual IEnumerator Perform(Agent agent)
    {
        Debug.Log("Started act");
        yield return new WaitForSeconds(5f);
        Debug.Log("Ended act");
    }
}

public class IdleAct : Act
{
    public IdleAct()
    {
        actName = "Idle";
        type = ActType.IDLE;
        priority = -100;
    }

    public override IEnumerator Perform(Agent agent)
    {
        yield return null;
    }
}
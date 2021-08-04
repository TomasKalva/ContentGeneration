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
        return detector && detector.Triggered;
    }

    public virtual void StartAct(Agent agent) 
    { 
        //Debug.Log("Started act"); 
    }
    /// <summary>
    /// Returns true if finished.
    /// </summary>
    public virtual bool UpdateAct(Agent agent) => true;
    public virtual void EndAct(Agent agent) 
    {
        //Debug.Log("Ended act"); 
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

    public override void StartAct(Agent agent) { }
    public override void EndAct(Agent agent) { }
}
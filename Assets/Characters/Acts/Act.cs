﻿using System.Collections;
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
    ColliderDetector detector;

    [SerializeField]
    public string actName;

    [SerializeField]
    public ActType type;

    [SerializeField]
    public int priority;

    /// <summary>
    /// True if the act just ended.
    /// </summary>
    public bool ActEnded { get; set; }

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
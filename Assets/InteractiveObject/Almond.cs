using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Does something. Receiver of activation from Activator.
/// </summary>
public abstract class Almond : MonoBehaviour
{
    ActionObject actionObject;

    public void SetActionObject(ActionObject actionObject)
    {
        this.actionObject = actionObject;
        actionObject.SetAction(Activate);
    }

    protected abstract void Activate();
}

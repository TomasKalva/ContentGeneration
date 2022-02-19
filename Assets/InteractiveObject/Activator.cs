using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Activates Almond.
/// </summary>
public class Activator : InteractiveObjectState
{
    ActionObject actionObject;

    public void SetActionObject(ActionObject actionObject)
    {
        this.actionObject = actionObject;
    }

    public override void Interact(Agent agent)
    {
        if (actionObject != null)
        {
            actionObject.Use();
        }
    }
}

public delegate bool Condition();

public class ActionObject
{
    Action action;
    Condition condition;
    string errorMessage;

    public ActionObject(Condition condition, string errorMessage)
    {
        this.condition = condition;
        this.errorMessage = errorMessage;
    }

    public void SetAction(Action action)
    {
        this.action = action;
    }

    public void Use()
    {
        if (action != null && condition != null)
        {
            if (condition())
            {
                action();
            }
            else
            {
                GameViewModel.ViewModel.Message = errorMessage;
            }
        }
    }
}

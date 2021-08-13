using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public InteractiveObjectState state;

    // Start is called before the first frame update
    void Awake()
    {
        state = GetComponent<InteractiveObjectState>();
    }

    public virtual void Interact(Agent agent)
    {
        GameViewModel.ViewModel.Message = state.MessageOnInteract;
        InteractLogic(agent);
    }

    protected virtual void InteractLogic(Agent agent)
    {
        Debug.Log("Interacted");
    }
}

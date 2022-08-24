using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public InteractiveObjectState _state;
    public InteractiveObjectState State
    {
        get => _state;
        set
        {
            _state = value;
            if(_state.InteractiveObject != this)
            {
                _state.InteractiveObject = this;
            }
        }
    }

    public void Interact(Agent agent)
    {
        GameViewModel.ViewModel.Message = State.MessageOnInteract;
        
        State.Interact(agent);
    }

    public void OptionalInteract(Agent agent, int optionIndex)
    {
        State.OptionalInteract(agent, optionIndex);
    }
}

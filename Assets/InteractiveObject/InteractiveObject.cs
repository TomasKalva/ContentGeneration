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

    public World World { get; private set; }

    public Reality Reality { get; private set; }
    
    // Start is called before the first frame update
    void Awake()
    {
        World = GameObject.Find("World").GetComponent<World>();
        Reality = GameObject.Find("Reality").GetComponent<Reality>();
        Initialize();
    }

    protected virtual void Initialize() { }

    public void Interact(Agent agent)
    {
        GameViewModel.ViewModel.Message = State.MessageOnInteract;
        
        State?.Interact(agent);
    }
}
